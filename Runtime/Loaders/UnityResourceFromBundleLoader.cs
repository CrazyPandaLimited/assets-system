#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFromBundleLoader : AbstractMemoryCachedLoader<UnityResourceFromBundleWorker, object>
    {
        #region Constructors

        public UnityResourceFromBundleLoader(ICoroutineManager coroutineManager, string supportMask = "ResourceFromBundle") : base(supportMask, coroutineManager,
            new ResourceMemoryCache())
        {
        }

        public UnityResourceFromBundleLoader(ICache<object> memoryCacheOverride, ICoroutineManager coroutineManager, string supportMask = "ResourceFromBundle") : base(
            supportMask, coroutineManager, memoryCacheOverride)
        {
        }

        #endregion

        #region Public Members

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            if (IsCached(uri))
            {
                return true;
            }

            var resourceName = UrlHelper.GetResourceName(uri);
            AssetBundleManifest manifest = null;
            var bundlesLoader = GetExistBundlesLoader(out manifest);

            var assetInfo = manifest.AssetInfos[resourceName];
            var bundle = manifest.GetBundleByAssetName(resourceName);

            if (!bundlesLoader.IsCached(UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, bundle.Name)))
            {
                return false;
            }

            foreach (var dependentBundle in assetInfo.Dependencies)
            {
                if (!bundlesLoader.IsCached(UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, dependentBundle)))
                {
                    return false;
                }
            }

            return true;
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            if (IsCached(uri))
            {
                return GetCachedResource<T>(owner, uri);
            }

            var resourceName = UrlHelper.GetResourceName(uri);
            AssetBundleManifest manifest = null;
            var bundlesLoader = GetExistBundlesLoader(out manifest);

            var assetInfo = manifest.AssetInfos[resourceName];
            var bundle = manifest.GetBundleByAssetName(resourceName);


            if (!bundlesLoader.IsCached(UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, bundle.Name)))
            {
                throw new ResourceSystemException(string.Format("Bundle {0} not loaded in cache", bundle.Name));
            }

            foreach (var dependentBundle in assetInfo.Dependencies)
            {
                if (!bundlesLoader.IsCached(UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, dependentBundle)))
                {
                    throw new ResourceSystemException(string.Format("Dependent Bundle {0} for Bundle {1} not loaded in cache", dependentBundle, bundle.Name));
                }
            }

            var cachedBundle = bundlesLoader.LoadResourceImmediately<AssetBundle>(this, UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, bundle.Name));

            T asset = null;

            if (resourceName.EndsWith(".prefab"))
            {
                var prefab = cachedBundle.LoadAsset<GameObject>(resourceName);
                if (typeof(T) == typeof(GameObject))
                    asset = prefab as T;
                else
                    asset = prefab.GetComponent<T>();
            }
            else
            {
                asset = cachedBundle.LoadAsset<T>(resourceName);
            }

            if (asset == null) throw new ResourceSystemException(string.Format("Asset {0} not loaded from bundle", resourceName));

            _memoryCache.Add(owner, uri, asset);
            return asset;
        }

        private IResourceLoader GetExistBundlesLoader(out AssetBundleManifest manifest)
        {
            IResourceLoader existLoader = null;
            if (_resourceStorage.IsLoaderRegistered<WebRequestBundlesLoader>())
            {
                existLoader = _resourceStorage.GetResourceLoader<WebRequestBundlesLoader>();
                manifest = ((WebRequestBundlesLoader) existLoader).Manifest;
                return existLoader;
            }

            if (_resourceStorage.IsLoaderRegistered<LocalFolderBundlesLoader>())
            {
                existLoader = _resourceStorage.GetResourceLoader<LocalFolderBundlesLoader>();
                manifest = ((LocalFolderBundlesLoader) existLoader).Manifest;
                return existLoader;
            }

            throw new ResourceSystemException("Not found correct bundles loader!!!");
        }

        public override void DestroyResource(string uri, object resource)
        {
#if !UNITY_EDITOR
            var unityObject = resource as UnityEngine.Object;

            if (unityObject != null)
            {
                UnityEngine.Object.Destroy(unityObject);
            }
#endif
        }

        #endregion

        #region Protected Members

        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {
            return _memoryCache.Get(owner, uri) as TResource;
        }

        protected override void ValidateInputData<TResource>(string uri)
        {
        }

        protected override UnityResourceFromBundleWorker CreateWorker<TResource>(string uri)
        {
            var resourceName = UrlHelper.GetResourceName(uri);
            var worker = new UnityResourceFromBundleWorker(uri, HandleLoadingComplete<TResource>, _coroutineManager);

            var bundlesLoader = _resourceStorage.GetResourceLoader<IBundlesLoader>();
            var assetInBundleInfo = bundlesLoader.Manifest.AssetInfos[resourceName];

            var bundle = bundlesLoader.Manifest.GetBundleByAssetName(resourceName);

            worker.RegisterMainDependency(_resourceStorage.LoadResource<AssetBundle>(worker, UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, bundle.Name)));

            foreach (var dependentBundle in assetInBundleInfo.Dependencies)
            {
                if (!bundlesLoader.IsCached(dependentBundle))
                {
                    worker.RegisterSecondDependency(_resourceStorage.LoadResource<AssetBundle>(worker,
                        UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, dependentBundle)));
                }
            }

            return worker;
        }

        public bool HasWorkersDependentOnAssetBundle(string bundleUri)
        {
            foreach (var w in _workersQueue.GetAllWorkersInWaitingSate())
            {
                var worker = w as UnityResourceFromBundleWorker;
                if (worker != null && worker.IsDependentOnAssetBundle(bundleUri)) return true;
            }

            foreach (var w in _workersQueue.GetAllWorkersInProcessSate())
            {
                var worker = w as UnityResourceFromBundleWorker;
                if (worker != null && worker.IsDependentOnAssetBundle(bundleUri)) return true;
            }
            return false;
        }

        protected override TResource GetResourceFromWorker<TResource>(UnityResourceFromBundleWorker worker)
        {
            var resource = worker.LoadedUnityObject as TResource;
            foreach (var completedLoaderLoadingOperation in worker.LoadingOperations)
            {
                _memoryCache.Add(completedLoaderLoadingOperation.Owner, worker.Uri, resource);
            }

            return resource;
        }

        protected override void CustomDestroyLogic(string resourceName, object resource)
        {
            base.CustomDestroyLogic(resourceName, resource);

            var bundlesLoader = _resourceStorage.GetResourceLoader<IBundlesLoader>();
            var correctResourceName = UrlHelper.GetResourceName(resourceName);
            var bundle = bundlesLoader.Manifest.GetBundleByAssetName(correctResourceName);

            _resourceStorage.ForceReleaseFromCache(UrlHelper.GetUriWithPrefix(bundlesLoader.SupportsMask, bundle.Name), true);
        }

        #endregion
    }
}
#endif