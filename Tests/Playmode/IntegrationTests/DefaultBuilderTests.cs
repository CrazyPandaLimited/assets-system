using CrazyPanda.UnityCore.AssetsSystem.Tests;
using System.Text;
using Newtonsoft.Json;
using System;
using UnityEngine.TestTools;
using Object = System.Object;
using System.Collections;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks;
using NUnit.Framework;
using UnityEngine;
using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.AssetsSystem.IntegrationTests
{
    [NUnit.Framework.Category("IntegrationTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class DefaultBuilderTests
    {
        private DefaultBuilder _builder;

        [ SetUp ]
        public void Setup()
        {
            AssetBundle.UnloadAllAssetBundles( true );
            _builder = new DefaultBuilder(  3 );
        }
        
        [ Test ]
        public void BuildAllTree()
        {
            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, UnityEngine.Object >();
            var loadAssetFromBundleTree = _builder.BuildLoadAssetFromBundleTree();
            var loadBundlesFromWebRequestTree = _builder.BuildLoadBundlesFromWebRequestTree( "" );
            var loadFromResourceFolderWithManifestTree = _builder.BuildLoadFromResourceFolderWithManifestTree();
            var loadFromResourceFolderTree = _builder.BuildLoadFromResourceFolderTree();

            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( loadAssetFromBundleTree );
            loadAssetFromBundleTree.NotExistOutput.LinkTo( loadBundlesFromWebRequestTree );
            loadBundlesFromWebRequestTree.NotExistOutput.LinkTo( loadFromResourceFolderWithManifestTree );
            loadFromResourceFolderWithManifestTree.NotExistOutput.LinkTo( loadFromResourceFolderTree );
        }


        [ UnityTest ]
        public IEnumerator SuccessLoadFromResourceFolder()
        {
            string url = "Cube";
            Object owner = new object();
            var loadFromResourceFolderTree = _builder.BuildLoadFromResourceFolderTree();
            _builder.AddExceptionsHandlingForAllNodes();

            _builder.AssetsStorage.LinkTo( loadFromResourceFolderTree.DefaultInput );
            var promise = _builder.AssetsStorage.LoadAssetAsync< GameObject >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );

            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.NotNull( promise.Result );
            Assert.True( _builder.OtherAssetsCache.Contains( url ) );
        }

        [ UnityTest ]
        public IEnumerator FailLoadFromResourceFolder()
        {
            string url = "CubeFail";
            Object owner = new object();
            var loadFromResourceFolderTree = _builder.BuildLoadFromResourceFolderTree();
            _builder.AddExceptionsHandlingForAllNodes();

            _builder.AssetsStorage.LinkTo( loadFromResourceFolderTree.DefaultInput );
            var promise = _builder.AssetsStorage.LoadAssetAsync< GameObject >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Rejected, promise.Status );
            Assert.Catch( () => { Debug.Log( promise.Result ); } );

            Assert.IsFalse( _builder.OtherAssetsCache.Contains( url ) );
        }

        [ UnityTest ]
        public IEnumerator FailLoadFromResourceFolderThenSuccess()
        {
            string url = "CubeFail";
            Object owner = new object();
            var loadFromResourceFolderTree = _builder.BuildLoadFromResourceFolderTree();
            _builder.AddExceptionsHandlingForAllNodes();

            _builder.AssetsStorage.LinkTo( loadFromResourceFolderTree.DefaultInput );
            var promise = _builder.AssetsStorage.LoadAssetAsync< GameObject >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Rejected, promise.Status );
            Assert.Catch( () => { Debug.Log( promise.Result ); } );

            Assert.IsFalse( _builder.OtherAssetsCache.Contains( url ) );

            url = "Cube";
            promise = _builder.AssetsStorage.LoadAssetAsync< GameObject >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.NotNull( promise.Result );
            Assert.True( _builder.OtherAssetsCache.Contains( url ) );
        }

        [ UnityTest ]
        public IEnumerator SuccessLoadAndUnloadFromResourceFolder()
        {
            string url = "Cube";
            Object owner = new object();
            var loadFromResourceFolderTree = _builder.BuildLoadFromResourceFolderTree();
            _builder.AddExceptionsHandlingForAllNodes();

            _builder.AssetsStorage.LinkTo( loadFromResourceFolderTree.DefaultInput );
            var promise = _builder.AssetsStorage.LoadAssetAsync< GameObject >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.NotNull( promise.Result );
            Assert.True( _builder.OtherAssetsCache.Contains( url ) );

            _builder.OtherAssetsCache.ReleaseAllAssetReferences( owner );
            _builder.OtherAssetsCache.RemoveUnusedAssets( destroy: false);

            Assert.False( _builder.OtherAssetsCache.Contains( url ) );
        }

        [ UnityTest ]
        public IEnumerator SuccessLoadAssetFromWebAndCheckGetFromCache()
        {
            var url = ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" );
            Object owner = new object();
            Object owner2 = new object();
            Object owner3 = new object();
            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, UnityEngine.Object >();

            var setExceptionNode = new AddExceptionToMessageProcessor< UrlLoadingRequest >( new Exception( $"Condition failed" ) );
            setExceptionNode.DefaultOutput.LinkTo( _builder.GetNewUrlRequestEndpoint() );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( setExceptionNode );

            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            _builder.AddExceptionsHandlingForAllNodes( setExceptionNode );

            var promise = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            var promise2 = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner2 ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise2.Status );
            Assert.NotNull( promise.Result );
            Assert.NotNull( promise2.Result );
            Assert.AreSame( promise.Result, promise2.Result );
            Assert.True( _builder.OtherAssetsCache.Contains( url ) );

            var cachedAsset = _builder.AssetsStorage.LoadAssetSync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner3 ) );
            Assert.NotNull( cachedAsset );
            Assert.AreSame( promise.Result, cachedAsset );
        }

        [ UnityTest ]
        public IEnumerator SuccessLoadAssetFromRemoteBundleAndCheckGetFromCache()
        {
            string BaseManifestName = "custom_manifest.json";
            string BundleName = "blue";
            string assetName = "Assets/blue.png";

            string manifestUrl = ResourceStorageTestUtils.ConstructTestBundlesUrl( BaseManifestName );

            Object owner = new object();

            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, System.Object >();
            var loadBundlesFromWebRequestTree = _builder.BuildLoadBundlesFromWebRequestTree( ResourceStorageTestUtils.ConstructTestBundlesUrl() );
            var loadAssetFromBundleTree = _builder.BuildLoadAssetFromBundleTree();
            var setExceptionNode = new AddExceptionToMessageProcessor< UrlLoadingRequest >( new Exception( $"Condition failed" ) );


            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( loadBundlesFromWebRequestTree );
            loadBundlesFromWebRequestTree.NotExistOutput.LinkTo( loadAssetFromBundleTree );

            loadAssetFromBundleTree.NotExistOutput.LinkTo( setExceptionNode );
            setExceptionNode.DefaultOutput.LinkTo( _builder.GetNewUrlRequestEndpoint() );

            _builder.AddExceptionsHandlingForAllNodes();

            //Load manifest
            var manifestPromise = _builder.AssetsStorage.LoadAssetAsync< string >( manifestUrl, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( manifestPromise );
            Assert.AreEqual( PandaTaskStatus.Resolved, manifestPromise.Status );
            Assert.NotNull( manifestPromise.Result );

            var jsonSerializer = new NewtonsoftJsonSerializer( new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8 );
            var manifest = jsonSerializer.Deserialize<AssetBundleManifest>( manifestPromise.Result );
            _builder.AssetBundleManifest.AddManifestPart( manifest );

            //load asset
            var assetPromise = _builder.AssetsStorage.LoadAssetAsync< Texture >( assetName, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            yield return WaitForPromiseEnging( assetPromise );
            Assert.AreEqual( PandaTaskStatus.Resolved, assetPromise.Status );
            Assert.NotNull( assetPromise.Result );

            Assert.True( _builder.BundlesCache.Contains( BundleName ) );
            Assert.True( _builder.AssetsFromBundlesCache.Contains( assetName ) );
            Assert.False( _builder.OtherAssetsCache.Contains( assetName ) );
            Assert.False( _builder.OtherAssetsCache.Contains( BundleName ) );

            //check get from cache
            var syncLoadAssetFromCachedBundle = _builder.AssetsStorage.LoadAssetSync< Texture >( assetName, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            Assert.NotNull( syncLoadAssetFromCachedBundle );
            Assert.True( _builder.AssetsFromBundlesCache.Contains( assetName ) );

            //check sync loading
            syncLoadAssetFromCachedBundle = null;
            _builder.AssetsFromBundlesCache.Remove( assetName, false, false );
            Assert.False( _builder.AssetsFromBundlesCache.Contains( assetName ) );
            syncLoadAssetFromCachedBundle = _builder.AssetsStorage.LoadAssetSync< Texture >( assetName, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            Assert.NotNull( syncLoadAssetFromCachedBundle );
            Assert.True( _builder.AssetsFromBundlesCache.Contains( assetName ) );
        }

        [ UnityTest ]
        public IEnumerator FailLoadAssetFromWebThenSuccess()
        {
            UnityEngine.Debug.unityLogger.logEnabled = false;
            
            var url = ResourceStorageTestUtils.ConstructTestUrl( "logo_test_notExist.jpg" );
            Object owner = new object();
            Object owner2 = new object();
            Object owner3 = new object();
            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, UnityEngine.Object >();

            var setExceptionNode = new AddExceptionToMessageProcessor< UrlLoadingRequest >( new Exception( $"Condition failed" ) );
            setExceptionNode.DefaultOutput.LinkTo( _builder.GetNewUrlRequestEndpoint() );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( setExceptionNode );

            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            _builder.AddExceptionsHandlingForAllNodes( setExceptionNode );

            var promise = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            var promise2 = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner2 ) );
            yield return WaitForPromiseEnging( promise );

            Assert.AreEqual( PandaTaskStatus.Rejected, promise.Status );
            Assert.AreEqual( PandaTaskStatus.Rejected, promise2.Status );
            Assert.False( _builder.OtherAssetsCache.Contains( url ) );

            url = ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" );
            promise = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ) );
            promise2 = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner2 ) );
            yield return WaitForPromiseEnging( promise );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.AreEqual( PandaTaskStatus.Resolved, promise2.Status );
            Assert.NotNull( promise.Result );
            Assert.NotNull( promise2.Result );
            Assert.AreSame( promise.Result, promise2.Result );
            Assert.True( _builder.OtherAssetsCache.Contains( url ) );

            var cachedAsset = _builder.AssetsStorage.LoadAssetSync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner3 ) );
            Assert.NotNull( cachedAsset );
            Assert.AreSame( promise.Result, cachedAsset );
        }

        [ Test ]
        public void SuccessCancelLoadAssetFromWeb()
        {
            var url = ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" );
            Object owner = new object();
            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, UnityEngine.Object >();

            var setExceptionNode = new AddExceptionToMessageProcessor< UrlLoadingRequest >( new Exception( $"Condition failed" ) );
            setExceptionNode.DefaultOutput.LinkTo( _builder.GetNewUrlRequestEndpoint() );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( setExceptionNode );

            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            _builder.AddExceptionsHandlingForAllNodes( setExceptionNode );

            CancellationTokenSource cancelTocken = new CancellationTokenSource();

            var promise = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ), cancelTocken.Token );

            cancelTocken.Cancel();

            Assert.AreEqual( PandaTaskStatus.Rejected, promise.Status );

            Assert.NotNull( promise.Error );
        }

        [ UnityTest ]
        public IEnumerator SuccessCancelLoadAssetFromWebAfterCompleteLoading()
        {
            var url = ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" );
            Object owner = new object();
            var unityAssetFromWebRequestTree = _builder.BuildLoadFromWebRequestTree< UrlLoadingRequest, UnityEngine.Object >();

            var setExceptionNode = new AddExceptionToMessageProcessor< UrlLoadingRequest >( new Exception( $"Condition failed" ) );
            setExceptionNode.DefaultOutput.LinkTo( _builder.GetNewUrlRequestEndpoint() );
            unityAssetFromWebRequestTree.NotPassedOutput.LinkTo( setExceptionNode );

            _builder.AssetsStorage.LinkTo( unityAssetFromWebRequestTree.DefaultInput );
            _builder.AddExceptionsHandlingForAllNodes( setExceptionNode );

            CancellationTokenSource cancelTocken = new CancellationTokenSource();

            var promise = _builder.AssetsStorage.LoadAssetAsync< Texture >( url, MetaDataExtended.CreateMetaDataWithOwner( owner ), cancelTocken.Token );
            yield return WaitForPromiseEnging( promise );

            Assert.AreEqual( PandaTaskStatus.Resolved, promise.Status );
            Assert.NotNull( promise.Result );

            cancelTocken.Cancel();
        }

        private IEnumerator WaitForPromiseEnging <TaskType>(IPandaTask<TaskType> promise)
        {
            var timeoutTime = DateTime.Now.AddSeconds( 5f );
            while( promise.Status == PandaTaskStatus.Pending && DateTime.Now < timeoutTime )
            {
                yield return null;
            }
        }
    }
}
