#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public abstract class BaseLoader<TWorker> : IResourceLoader where TWorker : IResourceWorker
    {
        #region Protected Fields

        protected ICoroutineManager _coroutineManager;
        protected ResourceStorage _resourceStorage;
        protected Action<Exception> _onResourceLoadError;
        protected WorkersQueue _workersQueue;

        #endregion

        #region Public Members

        protected BaseLoader(string supportsMask, ICoroutineManager coroutineManager)
        {
            SupportsMask = supportsMask;
            _coroutineManager = coroutineManager;
        }

        public void OnRegisteredToResourceStorage(ResourceStorage resourceStorege, WorkersQueue workersQueue, Action<Exception> onResourceLoadError)
        {
            _resourceStorage = resourceStorege;
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

        public abstract bool IsCached(string uri);

        public abstract List<object> ReleaseAllFromCache(bool destroy = true);
        public abstract object ReleaseFromCache(object owner, string uri,bool destroy = true);

        public abstract object ForceReleaseFromCache(string uri,bool destroy = true);

        public abstract List<object> ReleaseAllOwnerResourcesFromCache(object owner,bool destroy = true);
        public abstract List<object> RemoveUnusedFromCache(bool destroy = true);
        public abstract void DestroyResource(object resource);

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