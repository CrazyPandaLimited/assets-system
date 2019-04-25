#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class LocalFolderBundlesLoader : BaseLoader<LocalFolderBundleWorker>, IBundlesLoader
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
    {
        #region Properties

        public AssetBundleManifest Manifest { get; protected set; }

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
        private readonly string _localFolder;

        #endregion

        #region Constructors

        public LocalFolderBundlesLoader(string localFolder, ICoroutineManager coroutineManager, string supportsMask = "LocalFolderBundle",
            AssetBundleManifest manifest = null) : base(supportsMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _bundlesMemoryCache = new BundlesMemoryCache();
            _localFolder = localFolder;
            SupportsMask = supportsMask;
            _coroutineManager = coroutineManager;
        }

        public LocalFolderBundlesLoader(string localFolder, ICache<AssetBundle> bundlesMemoryCacheOverride, ICoroutineManager coroutineManager,
            string supportsMask = "LocalFolderBundle",
            AssetBundleManifest manifest = null) : base(supportsMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _bundlesMemoryCache = bundlesMemoryCacheOverride;
            _localFolder = localFolder;
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

            throw new InvalidOperationException("Cannot load WebRequest synchronously");
        }

        public override bool IsCached(string uri)
        {
            return _bundlesMemoryCache.Contains(uri);
        }

        public override List<object> ReleaseAllFromCache( bool destroy = true)
        {
            List<object> result = new List<object>();
            var resForRelease = _bundlesMemoryCache.ReleaseAllResources();
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }

            foreach (var bundle in resForRelease)
            {
                if (destroy)
                {
                    DestroyResource(bundle);
                    continue;
                }
                result.Add(bundle);
            }

            return result;
        }

        public override object ReleaseFromCache(object owner, string uri, bool destroy = true)
        {
            var releasedBundle = _bundlesMemoryCache.ReleaseResource(owner, uri);

            if (releasedBundle != null && destroy)
            {
                DestroyResource(releasedBundle);
                return null;
            }

            return releasedBundle;
        }

        public override object ForceReleaseFromCache(string uri, bool destroy = true)
        {
            var releasedBundle = _bundlesMemoryCache.ForceReleaseResource(uri);

            if (releasedBundle != null && destroy)
            {
                DestroyResource(releasedBundle);
                return null;
            }

            return releasedBundle;
        }

        public override List<object> ReleaseAllOwnerResourcesFromCache(object owner, bool destroy = true)
        {
            List<object> result = new List<object>();
            var ownerResources = _bundlesMemoryCache.GetOwnerResourcesNames(owner);
            var resForRelease = _bundlesMemoryCache.ReleaseResources(owner, ownerResources);
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }

            foreach (var bundle in resForRelease)
            {
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
            List<object> result = new List<object>();
            var resForRelease = _bundlesMemoryCache.ForceReleaseResources(_bundlesMemoryCache.GetUnusedResourceNames());
            if (resForRelease == null || resForRelease.Count == 0)
            {
                return result;
            }

            foreach (var bundle in resForRelease)
            {
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
            ((AssetBundle) resource).Unload(false);
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

        protected override LocalFolderBundleWorker CreateWorker<TResource>(string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);
            var bundleInfo = Manifest.BundleInfos[resourceName];
            return new LocalFolderBundleWorker(_localFolder, uri, bundleInfo, HandleLoadingComplete<TResource>,
                _coroutineManager);
        }

        protected override TResource GetResourceFromWorker<TResource>(LocalFolderBundleWorker worker)
        {
            var loadedBundle = worker.AssetBundle;
            _bundlesMemoryCache.Add(null, worker.Uri, loadedBundle);
            return loadedBundle as TResource;
        }

        #endregion
    }
}
#endif