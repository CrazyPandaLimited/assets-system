#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class WebRequestBundlesLoader : BaseLoader<WebRequestBundleWorker, AssetBundle>, IBundlesLoader
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
    {
        #region Properties

        public AssetBundleManifest Manifest { get; protected set; }
        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }

		#endregion

		#region Private Fields

		public virtual string ServerUrl { get; set; }
		private readonly WebRequestSettings _webRequestSettings;

        #endregion

        #region Constructors

        public WebRequestBundlesLoader(string serverUrl, ICoroutineManager coroutineManager, string supportMask = "WebRequestBundle",
            WebRequestSettings webRequestSettings = null,
            AssetBundleManifest manifest = null, IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _memoryCache = new BundlesMemoryCache(_resourceStorage);
            AntiCacheUrlResolver = antiCacheUrlResolver;
			ServerUrl = serverUrl;
            _webRequestSettings = webRequestSettings;
        }

        public WebRequestBundlesLoader(string serverUrl, ICache<AssetBundle> bundlesMemoryCacheOverride, ICoroutineManager coroutineManager,
            string supportMask = "WebRequestBundle", WebRequestSettings webRequestSettings = null,
            AssetBundleManifest manifest = null, IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            AntiCacheUrlResolver = antiCacheUrlResolver;
            _memoryCache = bundlesMemoryCacheOverride;
			ServerUrl = serverUrl;
            _webRequestSettings = webRequestSettings;
        }

        #endregion

        #region Public Members

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            return IsCached(uri);
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            if (IsCached(uri))
            {
                return GetCachedResource<T>(owner, uri);
            }

            throw new ResourceSystemException("Cannot load WebRequest synchronously");
        }

        public override void DestroyResource(string uri, object resource)
        {
            var loader = _resourceStorage.GetResourceLoader<UnityResourceFromBundleLoader>();
            if (!loader.HasWorkersDependentOnAssetBundle(uri))
            {
                ((AssetBundle)resource).Unload(false);
            }
        }

        #endregion

        #region Protected Members

        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {   
            return _memoryCache.Get(owner, uri) as TResource;
        }

        protected override void ValidateInputData<TResource>(string uri)
        {
            var resourcePath = UrlHelper.GetResourceName(uri);

            if (!Manifest.BundleInfos.ContainsKey(resourcePath))
            {
                throw new ResourceSystemException("Manifest not contains bundle with name:" + resourcePath);
            }
        }

        protected override WebRequestBundleWorker CreateWorker<TResource>(string uri)
        {
            Debug.Log("uri="+uri);
            var resourceName = UrlHelper.GetResourceName(uri);
            var bundleInfo = Manifest.BundleInfos[resourceName];
            return new WebRequestBundleWorker(ServerUrl, uri, bundleInfo, HandleLoadingComplete<TResource>,_coroutineManager, AntiCacheUrlResolver, _webRequestSettings);
        }

        protected override TResource GetResourceFromWorker<TResource>(WebRequestBundleWorker worker)
        {
            var loadedBundle = worker.AssetBundle;
            _memoryCache.Add(null, worker.Uri, loadedBundle);
            return loadedBundle as TResource;
        }

        #endregion
    }
}
#endif