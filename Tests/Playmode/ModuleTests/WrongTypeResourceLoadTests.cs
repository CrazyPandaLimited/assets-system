using System;
using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System.Collections;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.PandaTasks;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
#if !UNITY_EDITOR
    [Ignore("")]
#endif
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class WrongTypeResourceLoadTests
    {

        [UnityTest]
        public IEnumerator FailLoadFromResourceFolderWrongType()
        {
            string url = "Cube";
            object owner = new object();
            TestBuilder testBuilder = new TestBuilder( 3 );
            var loadFromResourceFolderTree = testBuilder.BuildLoadFromResourceFolderTree();
            testBuilder.AddExceptionsHandlingForAllNodes();

            testBuilder.AssetsStorage.LinkTo( loadFromResourceFolderTree.DefaultInput );
            
            var promise = testBuilder.AssetsStorage.LoadAssetAsync<Texture>( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );            
            Assert.AreEqual( PandaTaskStatus.Rejected, promise.Status );
            Assert.NotNull( promise.Error );
            Assert.True( promise.Error is InvalidCastException);
        }

        private IEnumerator WaitForPromiseEnging<TTaskType>( IPandaTask<TTaskType> promise )
        {
            var timeoutTime = DateTime.Now.AddSeconds( 5f );
            while( promise.Status == PandaTaskStatus.Pending && DateTime.Now < timeoutTime )
            {
                yield return null;
            }
        }


        public class ResorcesFolderLoadAlwaysAsGameObjectProcessorTest : ResorcesFolderLoadProcessor
        {
            protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
            {
                if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
                {
                    Object asset = null;
                    if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
                    {
                        var subAssets = Resources.LoadAll( body.Url, typeof(GameObject) );
                        var subAssetName = header.MetaData.GetMeta<string>( MetaDataReservedKeys.GET_SUB_ASSET );

                        foreach( var subAsset in subAssets )
                        {
                            if( subAsset.name == subAssetName )
                            {
                                asset = subAsset;
                                break;
                            }
                        }
                    }
                    else
                    {
                        asset = Resources.Load( body.Url, typeof( GameObject ) );
                    }

                    if( asset == null )
                    {
                        header.AddException( new AssetNotLoadedException( $"Asset not found in project", this, header, body ) );
                        SendException( header, body );
                        return;
                    }

                    SendOutput( header, new AssetLoadingRequest<Object>( body, asset ) );
                    return;
                }

                if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
                {
                    header.AddException( new AssetNotLoadedException( $"Async loading for subAssets not supported by Unity3d API", this, header, body ) );
                    SendException( header, body );
                    return;
                }

                ConfigureLoadingProcess( new RequestProcessorData( Resources.LoadAsync( body.Url, typeof( GameObject ) ), header, body ) );
            }
            
        }

        public class TestBuilder
        {
            protected RequestsQueue _requestsQueue;
            protected RequestToPromiseMap _promiseMap;

            private List<IFlowNode> _allProcessors;

            //Caches
            public AssetsWithRefcountCacheController OtherAssetsCache { get; private set; }
            public AssetsFromBundlesWithRefcountCacheController AssetsFromBundlesCache { get; private set; }
            public BundlesCacheWithRefcountCacheController BundlesCache { get; private set; }

            //Manifests
            public AssetsManifest<AssetInfo> AssetsManifestFromResourcesFolder { get; private set; }
            public AssetBundleManifest AssetBundleManifest { get; private set; }

            public AssetsStorage AssetsStorage { get; private set; }

            public TestBuilder( int maxWorkingRequests )
            {
                _requestsQueue = new RequestsQueue( maxWorkingRequests );

                AssetsManifestFromResourcesFolder = new AssetsManifest<AssetInfo>();

                OtherAssetsCache = new AssetsWithRefcountCacheController();
                _promiseMap = new RequestToPromiseMap();

                AssetBundleManifest = new AssetBundleManifest();
                BundlesCache = new BundlesCacheWithRefcountCacheController();
                AssetsFromBundlesCache = new AssetsFromBundlesWithRefcountCacheController( AssetBundleManifest, BundlesCache );

                AssetsStorage = new AssetsStorage( _promiseMap );
                _allProcessors = new List<IFlowNode>();
            }

            /// <summary>
            ///Always should be last tree part because we don't have instruments to check contains asset in Resources folder or not
            /// </summary>
            /// <returns></returns>
            public CheckAssetInCacheWithRefcountProcessor BuildLoadFromResourceFolderTree()
            {
                var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( OtherAssetsCache );
                _allProcessors.Add( cacheChecker );

                var getFromCache = new GetAssetFromCacheWithRefcountProcessor<Object>( OtherAssetsCache );
                _allProcessors.Add( getFromCache );

                var loader = new ResorcesFolderLoadAlwaysAsGameObjectProcessorTest();
                _allProcessors.Add( loader );

                var addToCache = new AddAssetToCacheWithRefcountProcessor<Object>( OtherAssetsCache );
                _allProcessors.Add( addToCache );

                var finishNode = new AssetLoadingRequestEndPointProcessor<Object>( _promiseMap );
                _allProcessors.Add( finishNode );

                var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
                _allProcessors.Add( finishNodeException );

                cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
                getFromCache.DefaultOutput.LinkTo( finishNode );
                getFromCache.ExceptionOutput.LinkTo( finishNodeException );

                cacheChecker.NotExistInCacheOutput.LinkTo( loader );
                loader.DefaultOutput.LinkTo( addToCache );
                loader.ExceptionOutput.LinkTo( finishNodeException );
                addToCache.DefaultOutput.LinkTo( finishNode );
                addToCache.ExceptionOutput.LinkTo( finishNodeException );

                return cacheChecker;
            }

            public void AddExceptionsHandlingForAllNodes( params IFlowNode[] additionalNodes )
            {
                PrintExceptionForNodes( _allProcessors );
                PrintExceptionForNodes( additionalNodes );
            }

            public AssetLoadingRequestEndPointProcessor<TBodyType> GetNewAssetLoadingRequestEndpoint<TBodyType>()
            {
                var processor = new AssetLoadingRequestEndPointProcessor<TBodyType>( _promiseMap );
                _allProcessors.Add( processor );
                return processor;
            }

            public IEnumerable<T> GetExistingNodes<T>() where T : IFlowNode
            {
                var nodeType = typeof( T );
                return _allProcessors.Where( node => node.GetType() == nodeType ).Cast<T>();
            }

            public UrlRequestEndPointProcessor GetNewUrlRequestEndpoint()
            {
                var processor = new UrlRequestEndPointProcessor( _promiseMap );
                _allProcessors.Add( processor );
                return processor;
            }

            private void PrintExceptionForNodes( IEnumerable<IFlowNode> nodes )
            {
                if( nodes == null )
                {
                    return;
                }

                foreach( var node in nodes )
                {
                    node.OnStatusChanged += PrintExceptionForNode;
                }
            }

            private void PrintExceptionForNode( object sender, FlowNodeStatusChangedEventArgs args )
            {
                if( args.NewStatus == FlowNodeStatus.Failed )
                {
                    var exception = args.Node.Exception;
                    Debug.LogException( exception );

                    if( _promiseMap.Has( args.Header.Id ) )
                    {
                        _promiseMap.Get( args.Header.Id ).SetError( exception );
                    }
                }
            }
        }
    }
}