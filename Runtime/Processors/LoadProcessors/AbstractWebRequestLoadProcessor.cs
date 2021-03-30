using System;
using System.Collections.Generic;
using System.Net;
using JetBrains.Annotations;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractWebRequestLoadProcessor< T > : AbstractRequestProcessor< UnityWebRequestAsyncOperation, UrlLoadingRequest, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Private Fields
        private readonly WebRequestSettings _webRequestSettings;
        #endregion

        #region Constructors
        protected AbstractWebRequestLoadProcessor([CanBeNull] WebRequestSettings webRequestSettings ) => _webRequestSettings = webRequestSettings;
        #endregion

        #region Protected Members
        protected abstract UnityWebRequest GetRequestData(MessageHeader header, UrlLoadingRequest body );
        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                header.AddException( new SyncLoadNotSupportedException( this, header, body ) );
                SendException( header, body );
                return;
            }

            BuildAndSendWebRequest( GetRequestData(header, body ), header, body );
        }

        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );

        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data ) => data.Body.ProgressTracker.ReportProgress( FinalProgress );

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data ) => RequestFinishedWithoutErrors( data.RequestLoadingOperation.webRequest, data.Header, data.Body );
        
        protected override void OnOperationCancelled( RequestProcessorData data ) => data.RequestLoadingOperation.webRequest.Dispose();
        
        #endregion

        #region Private Members
        private void BuildAndSendWebRequest( UnityWebRequest webRequest, MessageHeader header, UrlLoadingRequest body )
        {
            webRequest.downloadHandler = webRequest.downloadHandler ?? new DownloadHandlerBuffer();
            ConfigureWebRequest( webRequest, _webRequestSettings );

#if UNITY_2017_2_OR_NEWER
            var webRequestInProgressTask = webRequest.SendWebRequest();
#else
            var webRequestInProgressTask = webRequest.Send();
#endif

            ConfigureLoadingProcess( new RequestProcessorData( webRequestInProgressTask, header, body ) );
        }
        
        private void ConfigureWebRequest( UnityWebRequest webRequest, WebRequestSettings webRequestSettings )
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
        }

        private bool RequestFinishedWithoutErrors( UnityWebRequest webRequest, MessageHeader header, UrlLoadingRequest body )
        {
            if( !webRequest.isDone )
            {
                return false;
            }

#if UNITY_2017_1_OR_NEWER
            if( webRequest.isNetworkError || webRequest.isHttpError )
#else
			if( webRequest.isError )
#endif
            {
                header.AddException( new WebRequestException( webRequest, this, header, body ) );
                SendException( header, body );
                return false;
            }
            return true;
        }
        #endregion
    }
}
