#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFromBundleLoaderEditorLocal : AbstractMemoryCachedLoader<UnityResourceFromBundleWorkerEditorLocal>
    {
        #region Constructors

        public UnityResourceFromBundleLoaderEditorLocal(ICoroutineManager coroutineManager, string supportMask = "UnityResourceFromLocalFolderBundle") : base(supportMask,
            coroutineManager,
            new ResourceMemoryCache())
        {
        }


        public UnityResourceFromBundleLoaderEditorLocal(ICache<object> memoryCacheOverride, CoroutineManager coroutineManager,
            string supportMask = "UnityResourceFromLocalFolderBundle") : base(supportMask, coroutineManager,
            memoryCacheOverride)
        {
        }

        #endregion

        #region Public Members

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            return true;
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            if (IsCached(uri))
            {
                return GetCachedResource<T>(owner, uri);
            }
            var resourceName = UrlHelper.GetResourceName(uri);

            T asset = null;
#if UNITY_EDITOR
            if (resourceName.EndsWith(".prefab"))
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(resourceName);
                if (typeof(T) == typeof(GameObject))
                    asset = prefab as T;
                else
                    asset = prefab.GetComponent<T>();
            }
            else
            {
                asset = AssetDatabase.LoadAssetAtPath<T>(resourceName);
            }
#endif
            if (asset == null) throw new ResourceSystemException(string.Format("Asset {0} not loaded from bundle", resourceName));

            _memoryCache.Add(owner, uri, asset);
            return asset;
        }

        public override void DestroyResource(object resource)
        {
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

        protected override UnityResourceFromBundleWorkerEditorLocal CreateWorker<TResource>(string uri)
        {
            var worker =new UnityResourceFromBundleWorkerEditorLocal(uri, HandleLoadingComplete<TResource>,_coroutineManager);
            return worker;
        }

        protected override TResource GetResourceFromWorker<TResource>(UnityResourceFromBundleWorkerEditorLocal worker)
        {
            var resource = worker.LoadedUnityObject as TResource;
            foreach (var completedLoaderLoadingOperation in worker.LoadingOperations)
            {
                _memoryCache.Add(completedLoaderLoadingOperation.Owner, worker.Uri, resource);
            }

            return resource;
        }

        #endregion
    }
}
#endif