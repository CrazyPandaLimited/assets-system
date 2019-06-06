#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public abstract class BaseLoader<TWorker, TCache> : IResourceLoader where TWorker : IResourceWorker
    {
        #region Protected Fields

        protected ICoroutineManager _coroutineManager;
        protected ResourceStorage _resourceStorage;
        protected Action<Exception> _onResourceLoadError;
        protected WorkersQueue _workersQueue;
        protected ICache<TCache> _memoryCache;

        #endregion

        #region Public Members

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        public virtual List<ICacheDebugger> DebugCaches
        {
            get
            {
                if (_memoryCache is ICacheDebugger) return new List<ICacheDebugger>(1) { (ICacheDebugger)_memoryCache };
                return new List<ICacheDebugger>(0);
            }
        }
#endif

        protected BaseLoader(string supportsMask, ICoroutineManager coroutineManager)
        {
            SupportsMask = supportsMask;
            _coroutineManager = coroutineManager;
        }

        public void OnRegisteredToResourceStorage(ResourceStorage resourceStorage, WorkersQueue workersQueue, Action<Exception> onResourceLoadError)
        {
            _resourceStorage = resourceStorage;
            if (_memoryCache is BundlesMemoryCache)
            {
                (_memoryCache as BundlesMemoryCache).SetResourceStorage(resourceStorage);
            }
            _workersQueue = workersQueue;
            _onResourceLoadError = onResourceLoadError;
        }

        public string SupportsMask { get; protected set; }
        
        public abstract bool CanLoadResourceImmediately<TResource>(object owner, string uri) where TResource : Object; 
        public abstract TResource LoadResourceImmediately<TResource>(object owner, string uri) where TResource : Object;

        public ILoadingOperation<TResource> CreateLoadingWorkerAsync<TResource>(object owner, string uri) where TResource : class
        {
            ValidateInputData<TResource>(uri);

            if (IsCached(uri))
            {
                var resource = GetCachedResource<TResource>(owner, uri);
                return LoadingOperation<TResource>.GetCompletedLoadingOperation(owner, uri, resource);
            }

            var worker = _workersQueue.GetExistResourceWorker<TWorker>(uri);
            if (worker == null)
            {
                worker = CreateWorker<TResource>(uri);
                _workersQueue.AddToQueue(worker);
            }

            var existLoadingOperation = worker.GetLoadingOperationByOwner(owner);
            if (existLoadingOperation != null)
            {
                return existLoadingOperation as ILoadingOperation<TResource>;
            }

            var loadingOperation = new LoadingOperation<TResource>(owner, uri, loadOp => { HandleLoadingCancel(worker, loadOp); });
            worker.RegisterLoadingOperation(loadingOperation);
            return loadingOperation;
        }

        public virtual bool IsCached(string uri)
        {
            return _memoryCache.Contains(uri);
        }        

        public virtual Dictionary<string, object> ReleaseAllFromCache(bool destroy = true)
        {
            Dictionary<string, object> releasedRes = _memoryCache.ReleaseAllResources();
            return DestroyReleasedResourcesIfNeed(releasedRes, destroy);
        }

        public virtual Dictionary<string, object> ReleaseFromCache(object owner, string uri, bool destroy = true)
        {
            Dictionary<string, object> releasedRes = _memoryCache.ReleaseResource(owner, uri);
            return DestroyReleasedResourcesIfNeed(releasedRes, destroy);
        }

        public virtual Dictionary<string, object> ForceReleaseFromCache(string uri,bool destroy = true)                    
        {
            Dictionary<string, object> releasedRes = _memoryCache.ForceReleaseResource(uri);
            return DestroyReleasedResourcesIfNeed(releasedRes, destroy);
        }

        public virtual Dictionary<string, object> ReleaseAllOwnerResourcesFromCache(object owner, bool destroy = true)
        {
            Dictionary<string, object> releasedRes = _memoryCache.ReleaseAllOwnerResources(owner);
            return DestroyReleasedResourcesIfNeed(releasedRes, destroy);
        }

        public virtual Dictionary<string, object> ReleaseUnusedFromCache(bool destroy = true)
        {
            Dictionary<string, object> releasedRes = _memoryCache.ReleaseUnusedResources();
            return DestroyReleasedResourcesIfNeed(releasedRes, destroy);
        }

        protected Dictionary<string, object> DestroyReleasedResourcesIfNeed(Dictionary<string, object> releasedRes, bool destroy)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            foreach (var kvp in releasedRes)
            {
                if (kvp.Value == null)
                {
                    continue;
                }
                if (destroy)
                {
                    CustomDestroyLogic(kvp.Key, kvp.Value);                    
                    result.Add(kvp.Key, null);
                }
                else
                {
                    result.Add(kvp.Key, kvp.Value);
                }
            }
            return result;
        }

        protected virtual void CustomDestroyLogic(string uri, object resource)
        {
            DestroyResource(uri, resource);
        }

        public abstract void DestroyResource(string uri, object resource);


        #endregion

        #region Protected Members

        protected abstract TResource GetCachedResource<TResource>(object owner, string uri) where TResource : class;
        protected abstract void ValidateInputData<TResource>(string uri) where TResource : class;
        protected abstract TWorker CreateWorker<TResource>(string uri) where TResource : class;

        protected void HandleLoadingComplete<TResource>(TWorker worker) where TResource : class
        {
            if (worker.Error != null && _onResourceLoadError != null)
            {
                _onResourceLoadError(worker.Error);
            }
            
            TResource resource = null;
            if (worker.Error == null && !worker.IsLoadingCanceled)
            {
                resource = GetResourceFromWorker<TResource>(worker);
            }

            foreach (var loadingOperation in worker.LoadingOperations)
            {
                var convertedLoadingOperation = (LoadingOperation<TResource>) loadingOperation;
                convertedLoadingOperation.LoadingComplete(new ResourceLoadedEventArgs<TResource>(worker.Uri, resource, worker.Error, worker.IsLoadingCanceled));
            }

            _workersQueue.LoadingComplete(worker);
            worker.Dispose();
        }

        protected abstract TResource GetResourceFromWorker<TResource>(TWorker worker) where TResource : class;

        #endregion

        #region Private Members

        private void HandleLoadingCancel<TResource>(TWorker worker, LoadingOperation<TResource> loadingOperation)
            where TResource : class
        {
            worker.UnregisterLoadingOperation(loadingOperation);
        }

        #endregion
    }
}
#endif