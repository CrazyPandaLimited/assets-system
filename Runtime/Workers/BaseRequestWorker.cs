#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public abstract class BaseRequestWorker : IResourceWorker
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , IResourceWorkerDebug
#endif
    {
        #region Protected Fields

        protected readonly List<IResourceSystemLoadingOperation> _loadingOperations;

        #endregion

        #region Private Fields

        private ICoroutineManager _coroutineManager;
        protected IAntiCacheUrlResolver _antiCacheUrlResolver;

        #endregion

        #region Properties

        public List<IResourceSystemLoadingOperation> LoadingOperations
        {
            get { return _loadingOperations; }
        }

        public Exception Error { get; protected set; }

        public string Uri { get; private set; }

        public abstract bool IsWaitDependentResource { get; }

        public bool IsLoadingCanceled { get; private set; }

        #endregion

        #region Constructors

        protected BaseRequestWorker(string uri, ICoroutineManager coroutineManager, IAntiCacheUrlResolver antiCacheUrlResolver = null)
        {
            if (coroutineManager == null)
            {
                throw new ArgumentNullException("coroutineManager");
            }

            if (string.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("Uri is null or empty", "uri");
            }

            _coroutineManager = coroutineManager;
            _antiCacheUrlResolver = antiCacheUrlResolver;
            _loadingOperations = new List<IResourceSystemLoadingOperation>();
            Uri = uri;
        }

        #endregion

        #region Public Members

        public void RegisterLoadingOperation(IResourceSystemLoadingOperation loadingOperation)
        {
            _loadingOperations.Add(loadingOperation);
        }

        public void UnregisterLoadingOperation(IResourceSystemLoadingOperation loadingOperation)
        {
            InnerCancelLoading(new List<IResourceSystemLoadingOperation> {loadingOperation});
        }

        public void RemoveLoadingOperation(object owner)
        {
            var toDeleteList = new List<IResourceSystemLoadingOperation>();
            foreach (var loadingOperation in _loadingOperations)
            {
                if (loadingOperation.Owner == owner)
                {
                    toDeleteList.Add(loadingOperation);
                }
            }

            InnerCancelLoading(toDeleteList);
        }

        public IResourceSystemLoadingOperation GetLoadingOperationByOwner(object owner)
        {
            foreach (var loadingOperation in _loadingOperations)
            {
                if (loadingOperation.Owner == owner)
                {
                    return loadingOperation;
                }
            }

            return null;
        }

        public void StartLoading()
        {
            _coroutineManager.StartCoroutine(this, LoadProcess(), HandlerLoadProcessError);
        }

        private void HandlerLoadProcessError(object owner, Exception exception)
        {
            Error = exception;
            FireComplete();
        }


        public void CancelLoading()
        {
            InnerCancelLoading(new List<IResourceSystemLoadingOperation>(_loadingOperations));
        }

        public virtual void Dispose()
        {
            _coroutineManager = null;
            _loadingOperations.Clear();
            Uri = null;
        }

        #endregion

        #region Protected Members

        protected IEnumerator UpdateLoadingOperations(AsyncOperation asyncOperation)
        {
            while (!asyncOperation.isDone)
            {
                foreach (var loadingOperation in _loadingOperations)
                {
                    loadingOperation.LoadingProgressChange(new ResourceLoadingProgressEventArgs(Uri, asyncOperation.progress));
                }

                yield return null;
            }

            foreach (var loadingOperation in _loadingOperations)
            {
                loadingOperation.LoadingProgressChange(new ResourceLoadingProgressEventArgs(Uri, asyncOperation.progress));
            }
        }

        protected abstract void FireComplete();
        protected abstract IEnumerator LoadProcess();

        protected abstract void InnerCancelRequest();

        #endregion

        #region Private Members

        private void InnerCancelLoading(List<IResourceSystemLoadingOperation> stoppedLoadingOperations)
        {
            foreach (var loadingOperation in stoppedLoadingOperations)
            {
                loadingOperation.LoadingCanceled();
                _loadingOperations.Remove(loadingOperation);
            }

            if (_loadingOperations.Count == 0)
            {
                InnerCancelRequest();
                IsLoadingCanceled = true;
                _coroutineManager.StopAllCoroutinesForTarget(this);
                FireComplete();
            }
        }

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        List<IDebugLoadingOperation> IResourceWorkerDebug.LoadingOperations
        {
            get { return _loadingOperations.ConvertAll(input => (IDebugLoadingOperation) input); }
        }
#endif

        #endregion
    }
}
#endif