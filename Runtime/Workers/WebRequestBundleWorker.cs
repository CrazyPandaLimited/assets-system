#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.IO;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class WebRequestBundleWorker : BaseRequestWorker
    {
        #region Private Fields

        private BundleInfo _bundleInfo;

        private string _serverUrl;
        private Action<WebRequestBundleWorker> _onLoadingComplete;
        private UnityWebRequest _unityWebRequest;
        private WebRequestSettings _webRequestSettings;

        #endregion

        #region Properties

        public AssetBundle AssetBundle { get; private set; }

        public override bool IsWaitDependentResource
        {
            get { return false; }
        }

        #endregion

        #region Constructors

        public WebRequestBundleWorker(string serverUrl, string uri, BundleInfo bundleInfo,
            Action<WebRequestBundleWorker> onLoadingComplete, ICoroutineManager coroutineManager, IAntiCacheUrlResolver antiCacheUrlResolver,
            WebRequestSettings webRequestSettings = null) : base(uri, coroutineManager, antiCacheUrlResolver)
        {
            _serverUrl = serverUrl;
            _bundleInfo = bundleInfo;
            _onLoadingComplete = onLoadingComplete;
            _webRequestSettings = webRequestSettings;
        }

        #endregion

        #region Public Members

        public override void Dispose()
        {
            base.Dispose();
            _bundleInfo = null;
            AssetBundle = null;
            _onLoadingComplete = null;
        }

        #endregion

        #region Protected Members

        protected override void FireComplete()
        {
            _onLoadingComplete(this);
        }

        protected override IEnumerator LoadProcess()
        {
            string uriWithAnticache = _serverUrl +"/"+_bundleInfo.Name;
            if (_antiCacheUrlResolver != null)
            {
                uriWithAnticache = _antiCacheUrlResolver.ResolveURL(_serverUrl, _bundleInfo.Name);
            }
            _unityWebRequest = _bundleInfo.CRC == null || _bundleInfo.Hash == null
                ? UnityWebRequestAssetBundle.GetAssetBundle(uriWithAnticache)
                : UnityWebRequestAssetBundle.GetAssetBundle(uriWithAnticache, Hash128.Parse(_bundleInfo.Hash), uint.Parse(_bundleInfo.CRC));
            ConfigureWebRequest(_unityWebRequest, _webRequestSettings);

#if UNITY_2017_2_OR_NEWER
            var loadingAsyncOperation = _unityWebRequest.SendWebRequest();
#else
			var loadingAsyncOperation = request.Send();
#endif

            yield return UpdateLoadingOperations(loadingAsyncOperation);

#if UNITY_2017_1_OR_NEWER
            if (_unityWebRequest.isNetworkError || _unityWebRequest.isHttpError)
#else
			if( request.isError )
#endif
            {
                Error = new Exception(_unityWebRequest.error);
                FireComplete();
            }
            else
            {
                AssetBundle = ((DownloadHandlerAssetBundle) _unityWebRequest.downloadHandler).assetBundle;
                FireComplete();
            }

            _unityWebRequest.Dispose();
        }

        protected override void InnerCancelRequest()
        {
            if (_unityWebRequest == null)
            {
                return;
            }

            _unityWebRequest.Abort();
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