using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetFromBundleLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< Object >,UrlLoadingRequest >
    {
        #region Protected Fields
        protected IAssetsStorage _assetsStorage;
        protected ICacheControllerWithAssetReferences _bundlesCacheWithRefcounts;
        protected AssetBundleManifest _manifest;
        protected ICoroutineManager _coroutineManager;
        #endregion

        #region Constructors
        public AssetFromBundleLoadProcessor(IAssetsStorage assetsStorage, AssetBundleManifest manifest, ICacheControllerWithAssetReferences bundlesCacheWithRefcounts, ICoroutineManager coroutineManager )
        {
            _assetsStorage =  assetsStorage ?? throw new ArgumentNullException( nameof(assetsStorage) );
            _manifest = manifest ?? throw new ArgumentNullException( nameof(manifest) );
            _bundlesCacheWithRefcounts = bundlesCacheWithRefcounts ?? throw new ArgumentNullException( nameof(bundlesCacheWithRefcounts) );
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( nameof(coroutineManager) );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            bool isSyncRequest = header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
            var loadingTasks = new List< IPandaTask< AssetBundle > >();
            var progressTrackers = new List< ProgressTracker< float > >();
            var assetLoadingProgressTracker = new ProgressTracker< float >();
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
                    assetLoadingProgressTracker.ReportProgress( 1.0f );
                    OnAssetLoaded( header, body, mainBundleTask.Result.LoadAsset( body.Url, body.AssetType ), bundlesToLoad, bundlesHolder );
                }
                else
                {
                    _coroutineManager.StartCoroutine( this, LoadAssetAsync( header, body, mainBundleTask.Result, assetLoadingProgressTracker, bundlesToLoad, bundlesHolder ), ( o, exception ) =>
                    {
                        foreach( var loadedBundle in bundlesToLoad )
                        {
                            _bundlesCacheWithRefcounts.ReleaseReference( loadedBundle, bundlesHolder );
                        }

                        ProcessException( header, body, exception );
                    } );
                }
            } ).Fail( exception =>
            {
                header.AddException( exception );
                OnAssetLoaded( header, body, null, bundlesToLoad, bundlesHolder );
            } );

            return FlowMessageStatus.Accepted;
        }


        protected override void InternalDispose()
        {
            _coroutineManager.StopAllCoroutinesForTarget( this );
        }
        #endregion

        #region Private Members
        private IEnumerator LoadAssetAsync( MessageHeader header, UrlLoadingRequest body, AssetBundle mainBundle, IProgressTracker< float > progressTracker, List< string > bundlesNames, object bundlesHolder )
        {
            AssetBundleRequest assetrequest = mainBundle.LoadAssetAsync( body.Url, body.AssetType );

            while( !assetrequest.isDone )
            {
                progressTracker?.ReportProgress( assetrequest.progress );

                if( header.CancellationToken.IsCancellationRequested )
                {
                    foreach( var loadedBundle in bundlesNames )
                    {
                        _bundlesCacheWithRefcounts.ReleaseReference( loadedBundle, bundlesHolder );
                    }

                    yield break;
                }

                yield return null;
            }

            progressTracker?.ReportProgress( 1.0f );
            OnAssetLoaded( header, body, assetrequest.asset, bundlesNames, bundlesHolder );
        }

        private void OnAssetLoaded( MessageHeader header, UrlLoadingRequest body, Object asset, List< string > bundlesNames, object bundlesHolder )
        {
            foreach( var loadedBundle in bundlesNames )
            {
                _bundlesCacheWithRefcounts.ReleaseReference( loadedBundle, bundlesHolder );
            }

            if( asset == null )
            {
                header.AddException( new AssetSystemException( "Asset not loaded from bundle" ) );
                _exceptionConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
                return;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body.Url, body.AssetType, body.ProgressTracker, asset ) );
        }
        #endregion
    }
}
