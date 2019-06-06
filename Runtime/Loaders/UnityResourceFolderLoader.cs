#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.Utils;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFolderLoader : AbstractMemoryCachedLoader<UnityResourceFolderWorker, object>
    {
        #region Constructors

        public UnityResourceFolderLoader(ICoroutineManager coroutineManager, string supportsMask = "UnityResourceFolder") : base(supportsMask, coroutineManager,
            new ResourceMemoryCache())
        {
        }

        public UnityResourceFolderLoader(ICache<object> memoryCacheOverride, ICoroutineManager coroutineManager, string supportsMask = "UnityResourceFolder") : base(
            supportsMask, coroutineManager, memoryCacheOverride)
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
            if (_memoryCache.Contains(uri))
            {
                return _memoryCache.Get(owner, uri) as T;
            }


            var resourceName = UrlHelper.GetResourceName(uri);
            var resource = Resources.Load<T>(resourceName);

            if (resource == null)
            {
                throw new
                    ResourceSystemException(string.Format(@"You try to load '{0}' of type {1}. This resource not found in project.", uri, typeof(T).CSharpName()));
            }

            _memoryCache.Add(owner, uri, resource);

            return resource;
        }

        public override void DestroyResource(string uri, object resource)
        {
#if !UNITY_EDITOR
            var unityObject = resource as Object;
            if (unityObject != null) Resources.UnloadAsset(unityObject);
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

        protected override UnityResourceFolderWorker CreateWorker<TResource>(string uri)
        {
            return new UnityResourceFolderWorker(uri, HandleLoadingComplete<TResource>, _coroutineManager);
        }

        protected override TResource GetResourceFromWorker<TResource>(UnityResourceFolderWorker worker)
        {
            var resource = worker.LoadedUnityObject as TResource;
            foreach (var loadingOperation in worker.LoadingOperations)
            {
                _memoryCache.Add(loadingOperation.Owner, worker.Uri, resource);
            }

            return resource;
        }

        #endregion
    }
}
#endif