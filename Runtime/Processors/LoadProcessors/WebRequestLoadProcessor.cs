using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class WebRequestLoadProcessor< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected readonly ICoroutineManager _coroutineManager;
        protected readonly IAntiCacheUrlResolver _antiCacheUrlResolver;
        protected readonly List< IAssetDataCreator > _assetTypeDataCreators;
        protected readonly WebRequestSettings _webRequestSettings;
        #endregion

        #region Constructors
        public WebRequestLoadProcessor( ICoroutineManager coroutineManager, List< IAssetDataCreator > assetTypeDataCreators, WebRequestSettings webRequestSettings = null, IAntiCacheUrlResolver antiCacheUrlResolver = null )
        {
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( nameof(coroutineManager) );
            _assetTypeDataCreators = assetTypeDataCreators ?? throw new ArgumentNullException( nameof(assetTypeDataCreators) );
            _webRequestSettings = webRequestSettings;
            _antiCacheUrlResolver = antiCacheUrlResolver;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                header.AddException( new Exception( "Sync processing not available" ));
                _exceptionConnection.ProcessMessage( header,  body);
                return FlowMessageStatus.Accepted;
            }

            _coroutineManager.StartCoroutine( this, LoadingProcess( header, body ), ( o, exception ) => { ProcessException( header, body, exception ); } );

            return FlowMessageStatus.Accepted;
        }

        protected IEnumerator LoadingProcess( MessageHeader header, UrlLoadingRequest body )
        {
            string uriWithAnticache = body.Url;
            if( _antiCacheUrlResolver != null )
            {
                uriWithAnticache = _antiCacheUrlResolver.ResolveURL( uriWithAnticache );
            }

            var _webRequest = UnityWebRequest.Get( uriWithAnticache );
            ConfigureWebRequest( _webRequest, _webRequestSettings );

#if UNITY_2017_2_OR_NEWER
            var loadingAsyncOperation = _webRequest.SendWebRequest();
#else
			var loadingAsyncOperation = _webRequest.Send();
#endif

            while( !loadingAsyncOperation.isDone )
            {
                body.ProgressTracker.ReportProgress( loadingAsyncOperation.progress );
                if( header.CancellationToken.IsCancellationRequested )
                {
                    _webRequest.Dispose();
                    yield break;
                }

                yield return null;
            }

#if UNITY_2017_1_OR_NEWER
            if( _webRequest.isNetworkError || _webRequest.isHttpError )
#else
			if( _webRequest.isError )
#endif
            {
                header.AddException( new Exception( _webRequest.error ) );
                _exceptionConnection.ProcessMessage( header, body );
                yield break;
            }

            bool isCreatorFounded = false;
            Object asset = null;
            foreach( var assetDataCreator in _assetTypeDataCreators )
            {
                if( assetDataCreator.Supports( body.AssetType ) )
                {
                    isCreatorFounded = true;
                    asset = assetDataCreator.Create( _webRequest.downloadHandler.data, body.AssetType );
                }
            }

            _webRequest.Dispose();

            if( !isCreatorFounded )
            {
                header.AddException( new Exception( "Asset creator not found" ) );
                _exceptionConnection.ProcessMessage( header, body );
                yield break;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< T >( body, ( T ) asset ) );
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
