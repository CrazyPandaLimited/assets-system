#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class LocalFolderBundlesLoader : BaseLoader<LocalFolderBundleWorker, AssetBundle>, IBundlesLoader
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
    {
        #region Properties

        public AssetBundleManifest Manifest { get; protected set; }

        #endregion

        #region Private Fields
        
        private readonly string _localFolder;

        #endregion

        #region Constructors

        public LocalFolderBundlesLoader(string localFolder, ICoroutineManager coroutineManager, string supportsMask = "LocalFolderBundle",
            AssetBundleManifest manifest = null) : base(supportsMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _memoryCache = new BundlesMemoryCache(_resourceStorage);
            _localFolder = localFolder;
            SupportsMask = supportsMask;
            _coroutineManager = coroutineManager;
        }

        public LocalFolderBundlesLoader(string localFolder, ICache<AssetBundle> bundlesMemoryCacheOverride, ICoroutineManager coroutineManager,
            string supportsMask = "LocalFolderBundle",
            AssetBundleManifest manifest = null) : base(supportsMask, coroutineManager)
        {
            Manifest = manifest ?? new AssetBundleManifest();
            _memoryCache = bundlesMemoryCacheOverride;
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
            _memoryCache.Add(null, worker.Uri, loadedBundle);
            return loadedBundle as TResource;
        }

        #endregion
    }
}
#endif