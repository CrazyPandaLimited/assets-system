using System;
using System.Collections;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class BundlesFromWebRequestLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< AssetBundle >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected readonly ICoroutineManager _coroutineManager;
        #endregion

        #region Private Fields
        private readonly WebRequestSettings _webRequestSettings;
        #endregion

        #region Properties
        public AssetBundleManifest Manifest { get; }
        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }
        public virtual string ServerUrl { get; set; }
        #endregion

        #region Constructors
        public BundlesFromWebRequestLoadProcessor( string serverUrl, AssetBundleManifest manifest, ICoroutineManager coroutineManager, WebRequestSettings webRequestSettings = null, IAntiCacheUrlResolver antiCacheUrlResolver = null )
        {
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( nameof(coroutineManager) );
            ServerUrl = serverUrl ?? throw new ArgumentNullException( nameof(serverUrl) );

            if( manifest == null )
            {
                Manifest = new AssetBundleManifest();
            }
            else
            {
                Manifest = manifest;
            }

            _webRequestSettings = webRequestSettings;
            AntiCacheUrlResolver = antiCacheUrlResolver;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                header.AddException( new Exception( "Sync processing not available" ) );
                _exceptionConnection.ProcessMessage( header,  body );
                return FlowMessageStatus.Accepted;
            }

            var bundleInfo = Manifest.BundleInfos[ body.Url ];

            _coroutineManager.StartCoroutine( this, LoadingProcess( header, body, bundleInfo ), ( o, exception ) => { ProcessException( header, body, exception ); } );

            return FlowMessageStatus.Accepted;
        }

        protected IEnumerator LoadingProcess( MessageHeader header, UrlLoadingRequest body, BundleInfo bundleInfo )
        {
            string uriWithAnticache = ServerUrl + "/" + bundleInfo.Name;
            if( AntiCacheUrlResolver != null )
            {
                uriWithAnticache = AntiCacheUrlResolver.ResolveURL( ServerUrl, bundleInfo.Name );
            }

            var _unityWebRequest = bundleInfo.CRC == null || bundleInfo.Hash == null ? UnityWebRequestAssetBundle.GetAssetBundle( uriWithAnticache ) : UnityWebRequestAssetBundle.GetAssetBundle( uriWithAnticache, Hash128.Parse( bundleInfo.Hash ), uint.Parse( bundleInfo.CRC ) );
            ConfigureWebRequest( _unityWebRequest, _webRequestSettings );

#if UNITY_2017_2_OR_NEWER
            var loadingAsyncOperation = _unityWebRequest.SendWebRequest();
#else
			var loadingAsyncOperation = request.Send();
#endif

            while( !loadingAsyncOperation.isDone )
            {
                body.ProgressTracker.ReportProgress( loadingAsyncOperation.progress );
                if( header.CancellationToken.IsCancellationRequested )
                {
                    _unityWebRequest.Dispose();
                    yield break;
                }

                yield return null;
            }

#if UNITY_2017_1_OR_NEWER
            if( _unityWebRequest.isNetworkError || _unityWebRequest.isHttpError )
#else
			if( request.isError )
#endif
            {
                header.AddException( new Exception( _unityWebRequest.error ) );
                _exceptionConnection.ProcessMessage( header,  body );
                yield break;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< AssetBundle >( body, ( ( DownloadHandlerAssetBundle ) _unityWebRequest.downloadHandler ).assetBundle ) );
            _unityWebRequest.Dispose();
        }

        protected void ConfigureWebRequest( UnityWebRequest webRequest, WebRequestSettings webRequestSettings )
        {
            if( webRequestSettings == null )
            {
                return;
            }

            foreach( var header in webRequestSettings.Headers )
            {
                webRequest.SetRequestHeader( header.Key, header.Value );
            }

            if( webRequestSettings.Method != WebRequestMethod.NotSet )
            {
                webRequest.method = webRequestSettings.Method.ToString().ToUpper();
            }

            webRequest.timeout = Mathf.RoundToInt( webRequestSettings.Timeout );
            webRequest.redirectLimit = webRequestSettings.RedirectsLimit;
            webRequest.chunkedTransfer = webRequestSettings.ChunkTransfer;
        }

        protected override void InternalDispose()
        {
            _coroutineManager.StopAllCoroutinesForTarget( this );
        }
        #endregion
    }
}
