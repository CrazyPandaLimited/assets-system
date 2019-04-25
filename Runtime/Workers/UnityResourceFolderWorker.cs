#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFolderWorker : BaseRequestWorker
    {
        #region Private Fields

        private Action<UnityResourceFolderWorker> _onLoadingComplete;
        private ResourceRequest _loadingAsyncOperation;

        #endregion

        #region Properties

        public Object LoadedUnityObject { get; private set; }

        public override bool IsWaitDependentResource
        {
            get { return false; }
        }

        #endregion

        #region Constructors

        public UnityResourceFolderWorker(string uri, Action<UnityResourceFolderWorker> onLoadingComplete,
            ICoroutineManager coroutineManager) : base(uri, coroutineManager)
        {
            _onLoadingComplete = onLoadingComplete;
        }

        #endregion

        #region Public Members

        public override void Dispose()
        {
            base.Dispose();
            LoadedUnityObject = null;
            _onLoadingComplete = null;
        }

        #endregion

        #region Protected Members

        protected override void FireComplete()
        {
            _onLoadingComplete(this);
        }

        protected override IEnumerator LoadProcess()
        {
            var resourceName = UrlHelper.GetResourceName(Uri);
            _loadingAsyncOperation = Resources.LoadAsync(resourceName);

            yield return UpdateLoadingOperations(_loadingAsyncOperation);

            LoadedUnityObject = _loadingAsyncOperation.asset;

            if (_loadingAsyncOperation.asset == null)
            {
                Error = new Exception("Asset not in unity resource folder path:" + Uri);
                FireComplete();
            }
            else
            {
                LoadedUnityObject = _loadingAsyncOperation.asset;
                FireComplete();
            }
        }

        protected override void InnerCancelRequest()
        {
            // impossible to cancel this ResourceRequest
        }

        #endregion
    }
}
#endif