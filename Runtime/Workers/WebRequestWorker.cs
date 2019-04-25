#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class WebRequestWorker : BaseRequestWorker
    {
        #region Private Fields

        private Action<WebRequestWorker> _onLoadingComplete;
        private UnityWebRequest _webRequest;
        private WebRequestSettings _webRequestSettings;

        #endregion

        #region Properties

        public byte[] LoadedData { get; protected set; }

        public override bool IsWaitDependentResource
        {
            get { return false; }
        }

        #endregion

        #region Constructors

        public WebRequestWorker(string uri, Action<WebRequestWorker> onLoadingComplete, ICoroutineManager coroutineManager, IAntiCacheUrlResolver antiCacheUrlResolver,
            WebRequestSettings webRequestSettings = null) : base(uri, coroutineManager, antiCacheUrlResolver)
        {
            _webRequestSettings = webRequestSettings;
            _onLoadingComplete = onLoadingComplete;
        }

        #endregion

        #region Public Members

        public override void Dispose()
        {
            base.Dispose();
            LoadedData = null;
        }

        #endregion

        #region Protected Members

        protected override void FireComplete()
        {
            _onLoadingComplete(this);
        }

        protected override IEnumerator LoadProcess()
        {
            string uriWithAnticache = Uri;
            if (_antiCacheUrlResolver != null)
            {
                uriWithAnticache = _antiCacheUrlResolver.ResolveURL(Uri);
            }
            _webRequest = UnityWebRequest.Get(uriWithAnticache);
            ConfigureWebRequest(_webRequest, _webRequestSettings);

#if UNITY_2017_2_OR_NEWER
            var loadingAsyncOperation = _webRequest.SendWebRequest();
#else
			var loadingAsyncOperation = _webRequest.Send();
#endif

            yield return UpdateLoadingOperations(loadingAsyncOperation);

            LoadedData = _webRequest.downloadHandler.data;

#if UNITY_2017_1_OR_NEWER
            if (_webRequest.isNetworkError || _webRequest.isHttpError)
#else
			if( _webRequest.isError )
#endif
            {
                Error = new Exception(_webRequest.error);
                FireComplete();
            }
            else
            {
                FireComplete();
            }

            _webRequest.Dispose();
        }

        protected override void InnerCancelRequest()
        {
            if (_webRequest == null)
            {
                return;
            }

            _webRequest.Abort();
        }

        #endregion

        #region Private Members

        private void ConfigureWebRequest(UnityWebRequest webRequest, WebRequestSettings webRequestSettings)
        {
            if (webRequestSettings == null)
            {
                return;
            }

            foreach (var header in webRequestSettings.Headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }

            if (webRequestSettings.Method != WebRequestMethod.NotSet)
            {
                webRequest.method = webRequestSettings.Method.ToString().ToUpper();
            }

            webRequest.timeout = Mathf.RoundToInt(webRequestSettings.Timeout);
            webRequest.redirectLimit = webRequestSettings.RedirectsLimit;
            webRequest.chunkedTransfer = webRequestSettings.ChunkTransfer;
        }

        #endregion
    }
}
#endif