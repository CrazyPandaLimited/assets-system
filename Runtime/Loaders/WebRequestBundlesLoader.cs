#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class WebRequestBundlesLoader : BaseLoader<WebRequestBundleWorker>, IBundlesLoader
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
    {
        #region Properties

        public AssetBundleManifest Manifest { get; protected set; }
        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }

        #endregion

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        public List<ICacheDebugger> DebugCaches
        {
            get
            {
                if (_bundlesMemoryCache is ICacheDebugger) return new List<ICacheDebugger>(1) {(ICacheDebugger) _bundlesMemoryCache};

                return new List<ICacheDebugger>(0);
            }
        }
#endif

        #region Private Fields

        private readonly ICache<AssetBundle> _bundlesMemoryCache;
        private readonly string _serverUrl;
        private readonly WebRequestSettings _webRequestSettings;

        #endregion

        #region Constructors

        public WebRequestBundlesLoader(string serverUrl, ICoroutineManager coroutineManager, string supportMask = "WebRequestBundle",
            WebRequestSettings webRequestSettings = null,
            AssetBundleManifest manifest = null, IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _bundlesMemoryCache = new BundlesMemoryCache();
            AntiCacheUrlResolver = antiCacheUrlResolver;
            _serverUrl = serverUrl;
            _webRequestSettings = webRequestSettings;
        }

        public WebRequestBundlesLoader(string serverUrl, ICache<AssetBundle> bundlesMemoryCacheOverride, ICoroutineManager coroutineManager,
            string supportMask = "WebRequestBundle", WebRequestSettings webRequestSettings = null,
            AssetBundleManifest manifest = null, IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            AntiCacheUrlResolver = antiCacheUrlResolver;
            _bundlesMemoryCache = bundlesMemoryCacheOverride;
            _serverUrl = serverUrl;
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

        public override bool IsCached(string uri)
        {
            return _bundlesMemoryCache.Contains(uri);
        }

        public override List<object> ReleaseAllFromCache(bool destroy = true)
        {
            List<object> result =new List<object>();
            
            var resForRelease = _bundlesMemoryCache.ReleaseAllResources();
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }
            
            foreach (var bundle in resForRelease)
            {
                if (bundle == null)
                {
                    continue;
                }
                if (destroy)
                {
                    DestroyResource(bundle);
                    continue;
                }
                result.Add(bundle);
            }
            return result;
        }

        public override object ReleaseFromCache(object owner, string uri,bool destroy = true)
        {
            var bundle =  _bundlesMemoryCache.ReleaseResource(owner, uri);
            if (destroy)
            {
                if (bundle == null)
                {
                    return null;
                }
                DestroyResource(bundle);
                return null;
            }

            return bundle;
        }

        public override object ForceReleaseFromCache(string uri,bool destroy = true)
        {
            var bundle = _bundlesMemoryCache.ForceReleaseResource(uri);
            if (destroy)
            {
                if (bundle == null)
                {
                    return null;
                }
                DestroyResource(bundle);
                return null;
            }

            return bundle;
        }

        public override List<object> ReleaseAllOwnerResourcesFromCache(object owner,bool destroy = true)
        {
            List<object> result =new List<object>();
            var ownerResources = _bundlesMemoryCache.GetOwnerResourcesNames(owner);
            var resForRelease = _bundlesMemoryCache.ReleaseResources(owner, ownerResources);
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }
            
            foreach (var bundle in resForRelease)
            {
                if (bundle == null)
                {
                    continue;
                }
                if (destroy)
                {   
                    DestroyResource(bundle);
                    continue;
                }
                result.Add(bundle);
            }
            return result;
        }

        public override List<object> RemoveUnusedFromCache(bool destroy = true)
        {
            List<object> result =new List<object>();
            var resForRelease = _bundlesMemoryCache.ForceReleaseResources(_bundlesMemoryCache.GetUnusedResourceNames());
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }
            foreach (var bundle in resForRelease)
            {
                if (bundle == null)
                {
                    continue;
                }
                if (destroy)
                {
                    DestroyResource(bundle);
                    continue;
                }
                result.Add(bundle);
            }
            return result;
        }

        public override void DestroyResource(object resource)
        {
            ((AssetBundle)resource).Unload(false);
        }

        #endregion

        #region Protected Members

        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {   
            return _bundlesMemoryCache.Get(owner, uri) as TResource;
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
            var resourceName = UrlHelper.GetResourceName(uri);
            var bundleInfo = Manifest.BundleInfos[resourceName];
            return new WebRequestBundleWorker(_serverUrl, uri, bundleInfo, HandleLoadingComplete<TResource>,_coroutineManager, AntiCacheUrlResolver, _webRequestSettings);
        }

        protected override TResource GetResourceFromWorker<TResource>(WebRequestBundleWorker worker)
        {
            var loadedBundle = worker.AssetBundle;
            _bundlesMemoryCache.Add(null, worker.Uri, loadedBundle);
            return loadedBundle as TResource;
        }

        #endregion
    }
}
#endif