using CrazyPanda.UnityCore.PandaTasks.Progress;
using System.Linq;
using System.Threading;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class LoadAssetFromBundleProcessorTests : BaseProcessorModuleWithOneOutTest< AssetFromBundleLoadProcessor, UrlLoadingRequest, AssetLoadingRequest< UnityEngine.Object > >
    {
        private AssetsStorage _storageWithBundlesProcessors;
        private AssetBundleManifest _manifest;
        private CacheControllerForTests _cacheController;
        private RequestToPromiseMap _requestToPromiseMap;

        string bundleName = "experiment_aliceslots.bundle";

        string bundleName2 = "bundletest_0.bundle";
        string bundleName3 = "bundletest_1.bundle";

        private string assetInTestBundle0 = "Assets/AssetsInBundle/TestPrefab1.prefab";
        private string assetInTestBundle1 = "Assets/AssetsInBundle/TestPrefab2.prefab";

        private string assetName = "assets/assetbundlesexperiments/stuff/aliceslots/images/gem_alice.png";
        private string prefabAssetName = "assets/assetbundlesexperiments/stuff/aliceslots/prefabs/prefabalice.prefab";


        protected override void InternalSetup()
        {
            AssetBundle.UnloadAllAssetBundles( true );

            _manifest = new AssetBundleManifest();
            _manifest.BundleInfos.Add( bundleName, new BundleInfo( new GameAssetType( "Image" ), bundleName ) { AssetInfos = new List< string > { assetName, prefabAssetName } } );

            _manifest.AssetInfos.Add( assetName, new AssetInBundleInfo( assetName ) );
            _manifest.AssetInfos.Add( prefabAssetName, new AssetInBundleInfo( prefabAssetName ) );
            _manifest.RecalculateCache();

            var bundleLoader = new BundlesFromLocalFolderLoadProcessor( $"{Application.dataPath}/UnityCoreSystems/Systems/Tests/ResourcesSystem/Bundle", _manifest );

            _requestToPromiseMap = new RequestToPromiseMap();
            _storageWithBundlesProcessors = new AssetsStorage( _requestToPromiseMap );
            _storageWithBundlesProcessors.RegisterOutConnection( bundleLoader );

            bundleLoader.RegisterDefaultConnection( new AssetLoadingRequestEndPointProcessor< AssetBundle >( _requestToPromiseMap ) );

            _cacheController = new CacheControllerForTests();

            _workProcessor = new AssetFromBundleLoadProcessor(_storageWithBundlesProcessors, _manifest, _cacheController );
        }


        [ UnityTest ]
        public IEnumerator SuccesLoadAssetAsync()
        {
            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), sendedBody.Asset.GetType() );
        }


        [ Test ]
        public void SuccesLoadAssetSync()
        {
            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), sendedBody.Asset.GetType() );
        }


        [ UnityTest ]
        public IEnumerator SuccesLoadAssetWithDependentBundlesAsync()
        {
            var _manifestPart = new AssetBundleManifest();
            _manifestPart.BundleInfos.Add( bundleName2, new BundleInfo( new GameAssetType( "Image" ), bundleName2 ) { AssetInfos = new List< string > { assetInTestBundle0 } } );

            _manifestPart.BundleInfos.Add( bundleName3, new BundleInfo( new GameAssetType( "Image" ), bundleName3 ) { AssetInfos = new List< string > { assetInTestBundle1 } } );

            _manifestPart.AssetInfos.Add( assetName, new AssetInBundleInfo( assetName ) { Dependencies = new List< string > { bundleName2, bundleName3 } } );
            _manifestPart.RecalculateCache();


            _manifest.AddManifestPart( _manifestPart, true );

            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                yield return null;
            }

            Assert.AreEqual( 3, _cacheController.AddedReferences.Count );
            Assert.AreEqual( 3, _cacheController.RemovedReferences.Count );
            Assert.AreEqual( 0, _cacheController.ResultReferencesStatus.Count );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), sendedBody.Asset.GetType() );
        }

        [ Test ]
        public void SuccesLoadAssetWithDependentBundlesSync()
        {
            var _manifestPart = new AssetBundleManifest();
            _manifestPart.BundleInfos.Add( bundleName2, new BundleInfo( new GameAssetType( "Image" ), bundleName2 ) { AssetInfos = new List< string > { assetInTestBundle0 } } );

            _manifestPart.BundleInfos.Add( bundleName3, new BundleInfo( new GameAssetType( "Image" ), bundleName3 ) { AssetInfos = new List< string > { assetInTestBundle1 } } );

            _manifestPart.AssetInfos.Add( assetName, new AssetInBundleInfo( assetName ) { Dependencies = new List< string > { bundleName2, bundleName3 } } );
            _manifestPart.RecalculateCache();


            _manifest.AddManifestPart( _manifestPart, true );

            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName.Replace( ".bundle", "" ) ) );
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName2.Replace( ".bundle", "" ) ) );
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName3.Replace( ".bundle", "" ) ) );


            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), sendedBody.Asset.GetType() );

            Assert.AreEqual( 3, _cacheController.AddedReferences.Count );
            Assert.AreEqual( 3, _cacheController.RemovedReferences.Count );
            Assert.AreEqual( 0, _cacheController.ResultReferencesStatus.Count );
        }
        
        private class CacheControllerForTests : ICacheControllerWithAssetReferences
        {
            public List< string > AddedReferences = new List< string >();
            public List< string > RemovedReferences = new List< string >();
            public List< string > ResultReferencesStatus = new List< string >();

            public void Add( string assetName, object asset )
            {
                AddedReferences.Add( assetName );
                ResultReferencesStatus.Add( assetName );
            }

            public void Add< T >( T asset, string assetName, object reference )
            {
                AddedReferences.Add( assetName );
                ResultReferencesStatus.Add( assetName );
            }

            public List< string > GetAllAssetsNames()
            {
                return new List< string >();
            }

            public List< object > GetReferencesByAssetName( string assetName )
            {
                return new List< object >();
            }

            public object Get( string assetName, object reference, Type assetType )
            {
                AddedReferences.Add( assetName );
                ResultReferencesStatus.Add( assetName );
                return default;
            }

            public void ReleaseReference( string assetName, object reference )
            {
                RemovedReferences.Add( assetName );
                ResultReferencesStatus.Remove( assetName );
            }

            public T Get< T >( string assetName, object reference )
            {
                AddedReferences.Add( assetName );
                ResultReferencesStatus.Add( assetName );
                return default( T );
            }

            public bool Contains( string assetName )
            {
                return true;
            }
        }
    }

    public class TestComponent : MonoBehaviour
    {
    }
}