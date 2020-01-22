using System;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetFromBundleLoadProcessor : AbstractRequestProcessor< AssetBundleRequest, UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected IAssetsStorage _assetsStorage;
        protected ICacheControllerWithAssetReferences _bundlesCacheWithRefcounts;
        protected AssetBundleManifest _manifest;
        #endregion

        #region Constructors
        public AssetFromBundleLoadProcessor( IAssetsStorage assetsStorage, AssetBundleManifest manifest, ICacheControllerWithAssetReferences bundlesCacheWithRefcounts )
        {
            _assetsStorage = assetsStorage ?? throw new ArgumentNullException( nameof(assetsStorage) );
            _manifest = manifest ?? throw new ArgumentNullException( nameof(manifest) );
            _bundlesCacheWithRefcounts = bundlesCacheWithRefcounts ?? throw new ArgumentNullException( nameof(bundlesCacheWithRefcounts) );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            bool isSyncRequest = header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
            var loadingTasks = new List< IPandaTask< AssetBundle > >();
            var progressTrackers = new List< ProgressTracker< float > >();
            var cancelTocken = new CancellationTokenSource();
            IPandaTask< AssetBundle > mainBundleTask = null;

            var assetInfo = _manifest.AssetInfos[ body.Url ];
            var bundleInfo = _manifest.GetBundleByAssetName( body.Url );

            object bundlesHolder = new object();

            var metadataForBundlesRequest = isSyncRequest ? new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) : new MetaData();
            metadataForBundlesRequest.SetMeta( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY, bundlesHolder );

            List< string > bundlesToLoad = assetInfo.Dependencies == null ? new List< string >() : assetInfo.Dependencies;
            bundlesToLoad.Add( bundleInfo.Name );


            ProgressTracker< float > progressTracker = null;

            foreach( var dependentBundle in bundlesToLoad )
            {
                progressTracker = new ProgressTracker< float >();
                loadingTasks.Add( _assetsStorage.LoadAssetAsync< AssetBundle >( dependentBundle, metadataForBundlesRequest, cancelTocken.Token, progressTracker ) );
                progressTrackers.Add( progressTracker );
            }

            mainBundleTask = loadingTasks[ loadingTasks.Count - 1 ];


            header.CancellationToken.Register( () => { cancelTocken.Cancel(); } );

            CompositeProgressTracker compositeProgressTracker = new CompositeProgressTracker( progressTrackers );

            compositeProgressTracker.OnProgressChanged += ( sender, args ) => { body.ProgressTracker.ReportProgress( args.progress ); };

            new WhenAllPandaTask( loadingTasks ).Done( () =>
            {
                foreach( var loadedBundle in bundlesToLoad )
                {
                    _bundlesCacheWithRefcounts.Get< AssetBundle >( loadedBundle, bundlesHolder );
                }

                if( isSyncRequest )
                {
                    UnityEngine.Object asset = null;
            
                    if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
                    {
                        var subAssets = mainBundleTask.Result.LoadAssetWithSubAssets( body.Url, body.AssetType );
                        var subAssetName = header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );

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
                        asset = mainBundleTask.Result.LoadAsset( body.Url, body.AssetType );
                    }
                    
                    OnAssetLoaded( header, body, asset, bundlesToLoad, bundlesHolder );
                }
                else
                {
                    if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
                    {
                        ConfigureLoadingProcess( new AssetFromBundleProcessorData( mainBundleTask.Result.LoadAssetWithSubAssetsAsync( body.Url, body.AssetType ), header, body, bundlesToLoad, bundlesHolder ) );
                    }
                    else
                    {
                        ConfigureLoadingProcess( new AssetFromBundleProcessorData( mainBundleTask.Result.LoadAssetAsync( body.Url, body.AssetType ), header, body, bundlesToLoad, bundlesHolder ) );
                    }

                }
            } ).Fail( exception =>
            {
                header.AddException( exception );
                OnAssetLoaded( header, body, null, bundlesToLoad, bundlesHolder );
            } );

            return FlowMessageStatus.Accepted;
        }

        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( 0f );

        protected override void OnErrorLoading( RequestProcessorData data )
        {
            var processorData = ( AssetFromBundleProcessorData ) data;
            ReleaseAssetBundlesData( processorData.Bundles, processorData.Target );
        }

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( 1.0f );
            var processorData = ( AssetFromBundleProcessorData ) data;
            ReleaseAssetBundlesData( processorData.Bundles, processorData.Target );
            
            UnityEngine.Object asset = null;
            
            if( data.Header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                var subAssets = data.RequestLoadingOperation.allAssets;
                var subAssetName = data.Header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );

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
                asset = data.RequestLoadingOperation.asset;
            }
            
            _defaultConnection.ProcessMessage( data.Header, new AssetLoadingRequest< Object >( data.Body.Url, data.Body.AssetType, data.Body.ProgressTracker, asset ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.asset == null )
            {
                data.Header.AddException( new AssetSystemException( "Asset not loaded from bundle" ) );
                _exceptionConnection.ProcessMessage( data.Header, new AssetLoadingRequest< Object >( data.Body.Url, data.Body.AssetType, data.Body.ProgressTracker, data.RequestLoadingOperation.asset ) );
                return false;
            }

            return true;
        }
        #endregion

        #region Private Members
        private void OnAssetLoaded( MessageHeader header, UrlLoadingRequest body, Object asset, List< string > bundlesNames, object bundlesHolder )
        {
            ReleaseAssetBundlesData( bundlesNames, bundlesHolder );

            if( asset == null )
            {
                header.AddException( new AssetSystemException( "Asset not loaded from bundle" ) );
                _exceptionConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
                return;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
        }

        private void ReleaseAssetBundlesData( IReadOnlyList< string > bundles, object target )
        {
            foreach( var loadedBundle in bundles )
            {
                _bundlesCacheWithRefcounts.ReleaseReference( loadedBundle, target );
            }
        }
        #endregion

        protected sealed class AssetFromBundleProcessorData : RequestProcessorData
        {
            public IReadOnlyList< string > Bundles { get; }
            public object Target { get; }

            public AssetFromBundleProcessorData( AssetBundleRequest requestLoadingOperation, MessageHeader header, UrlLoadingRequest body, IReadOnlyList< string > bundles, object target ) : base( requestLoadingOperation, header, body )
            {
                Bundles = bundles ?? throw new ArgumentNullException( nameof(bundles) );
                Target = target ?? throw new ArgumentNullException( nameof(Target) );
            }
        }
    }
}
