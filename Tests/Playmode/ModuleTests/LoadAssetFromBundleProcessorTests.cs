using CrazyPanda.UnityCore.PandaTasks.Progress;
using System.Linq;
using System.Threading;
using UnityEngine.TestTools;
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    [TestFixture]
    public class LoadAssetFromBundleProcessorTests
    {
        private const float _defaultWaitTimeout = 5.0f;
        private const string bundleName = "experiment_aliceslots.bundle";

        private const string bundleName2 = "bundletest_0.bundle";
        private const string bundleName3 = "bundletest_1.bundle";

        private const string assetInTestBundle0 = "Assets/AssetsInBundle/TestPrefab1.prefab";
        private const string assetInTestBundle1 = "Assets/AssetsInBundle/TestPrefab2.prefab";

        private const string assetName = "assets/assetbundlesexperiments/stuff/aliceslots/images/gem_alice.png";
        private const string prefabAssetName = "assets/assetbundlesexperiments/stuff/aliceslots/prefabs/prefabalice.prefab";

        private AssetBundleManifest _manifest;
        private AssetFromBundleLoadProcessor _assetLoaderProcessor;
        private AssetBundleManifestCheckerProcessor _assetBundleLoaderProcessor;

        [SetUp]
        public void InternalSetup()
        {
            AssetBundle.UnloadAllAssetBundles( true );

            var nodesBuilder = new DefaultBuilder( 5 );
            
            _manifest = nodesBuilder.AssetBundleManifest;
            _manifest.BundleInfos.Add( bundleName, new BundleInfo( new GameAssetType( "Image" ), bundleName ) { AssetInfos = new List< string > { assetName, prefabAssetName } } );

            _manifest.AssetInfos.Add( assetName, new AssetInBundleInfo( assetName ) );
            _manifest.AssetInfos.Add( prefabAssetName, new AssetInBundleInfo( prefabAssetName ) );
            _manifest.RecalculateCache();

            var bundleLoader = new BundlesFromLocalFolderLoadProcessor( $"{Application.dataPath}/UnityCoreSystems/ResourcesSystem/Tests/Bundle", _manifest );

            nodesBuilder.AssetsStorage.LinkTo( bundleLoader.DefaultInput );
            bundleLoader.DefaultOutput.LinkTo( nodesBuilder.GetNewAssetLoadingRequestEndpoint< AssetBundle >() );

            _assetBundleLoaderProcessor = nodesBuilder.BuildLoadAssetFromBundleTree();
            
            _assetLoaderProcessor = nodesBuilder.GetExistingNodes<AssetFromBundleLoadProcessor>().First();
        }


        [ UnityTest ]
        public IEnumerator SuccesLoadAssetAsync()
        {
            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _assetLoaderProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = args.Body as AssetLoadingRequest< UnityEngine.Object >;
            };

            _assetBundleLoaderProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( _defaultWaitTimeout );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                yield return null;
            }

            Assert.Null( _assetLoaderProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.IsInstanceOf( typeof( Texture2D ), sendedBody.Asset );
        }
        
        [ Test ]
        public void SuccesLoadAssetSync()
        {
            var requestBody = new UrlLoadingRequest( assetName, typeof( Texture2D ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _assetLoaderProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = args.Body as AssetLoadingRequest< UnityEngine.Object >;
            };

            _assetBundleLoaderProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            Assert.Null( _assetLoaderProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.IsInstanceOf( typeof( Texture2D ), sendedBody.Asset );
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

            _assetLoaderProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = args.Body as AssetLoadingRequest< UnityEngine.Object >;
            };

            _assetBundleLoaderProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( _defaultWaitTimeout );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                yield return null;
            }

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.IsInstanceOf( typeof( Texture2D ), sendedBody.Asset );
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

            _assetLoaderProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = args.Body as AssetLoadingRequest< UnityEngine.Object >;
            };

            _assetBundleLoaderProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName.Replace( ".bundle", "" ) ) );
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName2.Replace( ".bundle", "" ) ) );
            Assert.NotNull( loadedBundles.First( c => c.name == bundleName3.Replace( ".bundle", "" ) ) );


            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.IsInstanceOf( typeof( Texture2D ), sendedBody.Asset );
        }
    }
}