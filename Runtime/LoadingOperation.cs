#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class LoadingOperation<T> : CustomYieldInstruction, IResourceSystemLoadingOperation, ILoadingOperation<T>
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , IDebugLoadingOperation
#endif
        where T : class
    {
        #region Private Fields

        private Action<LoadingOperation<T>> _onCancel;
        private Action<ResourceLoadedEventArgs<T>> _onLoad;
        private Action<ResourceLoadingProgressEventArgs> _onStep;

        #endregion

        #region Properties

        public string Uri { get; private set; }
        public object Owner { get; private set; }
        public T Resource { get; private set; }
        public bool IsCompleted { get; private set; }

        public bool IsCanceled { get; private set; }
        public float Progress { get; private set; }
        public Exception Error { get; private set; }

        public Action<ResourceLoadedEventArgs<T>> OnResourceLoaded
        {
            get { return _onLoad; }
            set { _onLoad = value; }
        }
        
        public Action<ResourceLoadingProgressEventArgs> OnLoadStep
        {
            get { return _onStep; }
            set { _onStep = value; }
        }
        

        public override bool keepWaiting
        {
            get { return !IsCompleted; }
        }

        #endregion

        #region Constructors

        public LoadingOperation(object owner, string uri, Action<LoadingOperation<T>> onCancel)
        {
            Owner = owner;
            Uri = uri;
            _onCancel = onCancel;
        }

        #endregion

        #region Public Members

        public void LoadingProgressChange(ResourceLoadingProgressEventArgs args)
        {
            Progress = args.Progress;
            if (_onStep != null)
            {
                _onStep(args);
            }
        }

        public void LoadingCanceled()
        {
            IsCanceled = true;
            LoadingComplete(ResourceLoadedEventArgs<T>.Canceled(Uri));
        }

        public void CancelLoading()
        {
            if (!IsCompleted)
            {
                IsCanceled = true;
                _onCancel(this);
            }
        }

        public override string ToString()
        {
            return Uri;
        }

        public static LoadingOperation<T> GetCompletedLoadingOperation(object owner, string uri, T loadedResource)
        {
            var result = new LoadingOperation<T>(owner, uri, null);
            result.Resource = loadedResource;
            result.IsCompleted = true;
            return result;
        }

        #endregion

        #region Private Members

        internal void LoadingComplete(ResourceLoadedEventArgs<T> args)
        {
            IsCompleted = true;
            Error = args.Error;
            Resource = args.Resource;

            if (_onLoad != null)
            {
                _onLoad(args);
            }
        }

        #endregion
    }
}
#endif