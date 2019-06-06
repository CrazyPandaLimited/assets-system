#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class VersionedDiskCachedResourceLoader : AbstractMemoryCachedLoader<VersionedDiskCachedResourceWorker, object>
    {
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        public override List<ICacheDebugger> DebugCaches
        {
            get
            {
                var result = base.DebugCaches;
                if (_fileCaching is ICacheDebugger) result.Add((ICacheDebugger) _fileCaching);

                return result;
            }
        }
#endif

        #region Private Fields

        private readonly List<IResourceDataCreator> _resourceDataCreators = new List<IResourceDataCreator>();

        protected ICache<byte[]> _fileCaching;
        protected string _serverUrl;
        public AssetsManifest<AssetFileInfo> Manifest { get; protected set; }
        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }

        #endregion

        #region Constructors

        public VersionedDiskCachedResourceLoader(string diskCacheDir, string serverUrl, ICoroutineManager coroutineManager,
            IAntiCacheUrlResolver antiCacheUrlResolver = null,
            string supportMask = "VersionedResource") : base(supportMask, coroutineManager, new ResourceMemoryCache())
        {
            Manifest = new AssetsManifest<AssetFileInfo>();
            _fileCaching = new FileCaching(diskCacheDir);
            AntiCacheUrlResolver = antiCacheUrlResolver;
            _serverUrl = serverUrl;
        }

        public VersionedDiskCachedResourceLoader(string serverUrl, ICache<byte[]> fileCacheOverride, ICache<object> memoryCacheOverride,
            ICoroutineManager coroutineManager, IAntiCacheUrlResolver antiCacheUrlResolver = null, string supportMask = "VersionedResource") : base(supportMask, coroutineManager, memoryCacheOverride)
        {
            Manifest = new AssetsManifest<AssetFileInfo>();
            AntiCacheUrlResolver = antiCacheUrlResolver;
            _fileCaching = fileCacheOverride;
            _serverUrl = serverUrl;
        }

        #endregion

        #region Public Members

        public void RegisterResourceCreator(IResourceDataCreator resourceDataCreator)
        {
            _resourceDataCreators.Add(resourceDataCreator);
        }

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);
            var resourceInfo = Manifest.GetAssetByName(resourceName);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());

            return IsCached(resourceFullUri) || _fileCaching.Contains(resourceFullUri);
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);
            var resourceInfo = Manifest.GetAssetByName(resourceName);

            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());

            if (IsCached(resourceFullUri))
            {
                return GetCachedResource<T>(owner, resourceFullUri);
            }

            if (_fileCaching.Contains(resourceFullUri))
            {
                var loadedData = _fileCaching.Get(owner, resourceFullUri);
                var asset = GetResourceFromCreator<T>(loadedData);
                _memoryCache.Add(owner, resourceFullUri, asset);
                return asset;
            }

            throw new ResourceSystemException(string.Format("Asset {0} not cached in memory or disk", resourceName));
        }

        public override bool IsCached(string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);

            if (!Manifest.HasResource(resourceName))
            {
                return false;
            }

            var resourceInfo = Manifest.GetAssetByName(resourceName);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());
            return _memoryCache.Contains(resourceFullUri);
        }

        public override Dictionary<string, object> ReleaseFromCache(object owner, string uri,bool destroy = true)
        {
            var resourceName = UrlHelper.GetResourceName(uri);

            if (!Manifest.HasResource(resourceName))
            {
                return null;
            }

            var resourceInfo = Manifest.GetAssetByName(resourceName);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());
            var resForRelease = _memoryCache.ReleaseResource(owner, resourceFullUri);
            return DestroyReleasedResourcesIfNeed(resForRelease, destroy);
        }

        public override void DestroyResource(string uri, object resource)
        {
            foreach (var resourceDataCreator in _resourceDataCreators)
            {
                if (resourceDataCreator.Supports(resource.GetType()))
                {
                    resourceDataCreator.Destroy(resource);
                    break;
                }
            }
        }

        private void RemoveOldResourcesFromDiskCache()
        {
            foreach (var assetInfo in Manifest._assetsInfos)
            {
                RemoveOldResourceFromDiskCache(assetInfo.Value);
            }
        }

        private void RemoveOldResourceFromDiskCache(string resourceName)
        {
            var resourceNameconv = UrlHelper.GetResourceName(resourceName);

            if (!Manifest.HasResource(resourceNameconv))
            {
                return;
            }

            var resourceInfo = Manifest.GetAssetByName(resourceNameconv);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());
            RemoveOldResourceFromDiskCache(Manifest.GetAssetByName(resourceFullUri));
        }

        private void RemoveOldResourceFromDiskCache(AssetFileInfo resourceInfo)
        {
            if (resourceInfo.version > 0)
            {
                var oldVersionREsources = resourceInfo.GetOldVersionedResourceName().ToList();

                for (int i = 0; i < oldVersionREsources.Count; i++)
                {
                    oldVersionREsources[i] = UrlHelper.GetUriWithPrefix(SupportsMask, oldVersionREsources[i]);                    
                }

                _memoryCache.ReleaseResources(null, oldVersionREsources);
                _fileCaching.ReleaseResources(null, oldVersionREsources);
            }
        }

        public void RemoveAllResourcesFromDiskCache()
        {
            _fileCaching.ReleaseAllResources();
        }

        #endregion

        #region Protected Members

        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);

            if (!Manifest.HasResource(resourceName))
            {
                return null;
            }

            var resourceInfo = Manifest.GetAssetByName(resourceName);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, resourceInfo.GetVersionedResourceName());
            return _memoryCache.Get(owner, resourceFullUri) as TResource;
        }

        protected override void ValidateInputData<TResource>(string uri)
        {
        }

        protected override VersionedDiskCachedResourceWorker CreateWorker<TResource>(string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);
            var assetInfo = Manifest.GetAssetByName(resourceName);
            var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, assetInfo.GetVersionedResourceName());

            VersionedDiskCachedResourceWorker worker = null;
            if (_fileCaching.Contains(resourceFullUri))
            {
                worker = new VersionedDiskCachedResourceWorker(resourceFullUri, assetInfo, _fileCaching, HandleLoadingComplete<TResource>, _coroutineManager);
            }
            else
            {
                worker = new VersionedDiskCachedResourceWorker(assetInfo, _serverUrl + assetInfo.GetVersionedResourceName(), HandleLoadingComplete<TResource>,
                    _coroutineManager, AntiCacheUrlResolver);
            }

            return worker;
        }

        protected override TResource GetResourceFromWorker<TResource>(VersionedDiskCachedResourceWorker worker)
        {
            var resource = GetResourceFromCreator<TResource>(worker.LoadedData);

            foreach (var loadingOperation in worker.LoadingOperations)
            {
                var resourceFullUri = UrlHelper.GetUriWithPrefix(SupportsMask, worker.AssetInfo.GetVersionedResourceName());
                _memoryCache.Add(loadingOperation.Owner, resourceFullUri, resource);
            }


            _fileCaching.Add(null, UrlHelper.GetUriWithPrefix(SupportsMask, worker.AssetInfo.GetVersionedResourceName()), worker.LoadedData);
            RemoveOldResourceFromDiskCache(worker.AssetInfo);
            return resource;
        }

        protected TResource GetResourceFromCreator<TResource>(byte[] data) where TResource : class
        {
            foreach (var resourceDataCreator in _resourceDataCreators)
            {
                if (resourceDataCreator.Supports(typeof(TResource)))
                {
                    return resourceDataCreator.Create<TResource>(data);
                }
            }

            return default(TResource);
        }

        #endregion
    }
}
#endif