#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFromBundleWorker : BaseRequestWorker
    {
        #region Private Fields

        private ILoadingOperation<AssetBundle> _loadingOperationForMainBundle;

        private List<ILoadingOperation<AssetBundle>> _loadingOperationForDependentBundles =
            new List<ILoadingOperation<AssetBundle>>();

        private Action<UnityResourceFromBundleWorker> _onLoadingComplete;
        private AssetBundleRequest _loadingAsyncOperation;

        #endregion

        #region Properties

        public Object LoadedUnityObject { get; private set; }

        public override bool IsWaitDependentResource
        {
            get
            {
                if (!_loadingOperationForMainBundle.IsCompleted)
                {
                    return true;
                }

                foreach (var loadingOperation in _loadingOperationForDependentBundles)
                {
                    if (!loadingOperation.IsCompleted)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        #endregion

        #region Constructors

        public UnityResourceFromBundleWorker(string uri, Action<UnityResourceFromBundleWorker> onLoadingComplete,
            ICoroutineManager coroutineManager) : base(uri, coroutineManager)
        {
            _onLoadingComplete = onLoadingComplete;
        }

        #endregion

        #region Public Members

        public bool IsDependentOnAssetBundle(string bundleUri)
        {
            if (string.IsNullOrEmpty(bundleUri)) return false;
            foreach (var lo in _loadingOperationForDependentBundles)
            {
                if (lo.Uri == bundleUri) return true;
            }
            return _loadingOperationForMainBundle.Uri == bundleUri;
        }

        public void RegisterMainDependency<T>(ILoadingOperation<T> dependendentLoader) where T : class
        {
            _loadingOperationForMainBundle = (ILoadingOperation<AssetBundle>) dependendentLoader;
        }

        public void RegisterSecondDependency<T>(ILoadingOperation<T> dependendentLoader) where T : class
        {
            _loadingOperationForDependentBundles.Add((ILoadingOperation<AssetBundle>) dependendentLoader);
        }

        public override void Dispose()
        {
            base.Dispose();
            LoadedUnityObject = null;
        }

        #endregion

        #region Protected Members

        protected override void FireComplete()
        {
            _onLoadingComplete(this);
        }

        protected override IEnumerator LoadProcess()
        {
            if (_loadingOperationForMainBundle == null)
            {
                Error = new Exception("Main bundle loading operation is NULL resource:" + Uri);
                FireComplete();
                yield break;
            }


            if (_loadingOperationForMainBundle.Error != null)
            {
                Error = _loadingOperationForMainBundle.Error;
                FireComplete();
                yield break;
            }

            foreach (var operationForDependentBundle in _loadingOperationForDependentBundles)
            {
                if (operationForDependentBundle.Error != null)
                {
                    Error = operationForDependentBundle.Error;
                    FireComplete();
                    yield break;
                }
            }

            var resourceName = UrlHelper.GetResourceName(Uri);
            Debug.Log("Try get resource from bundle " + resourceName);
            if (!_loadingOperationForMainBundle.Resource.Contains(resourceName))
            {
                Error = new Exception("Bundle" + _loadingOperationForMainBundle.Uri + " not contains resource " + Uri);
                FireComplete();
                yield break;
            }
            
            
            
            _loadingAsyncOperation = _loadingOperationForMainBundle.Resource.LoadAssetAsync(resourceName);


            yield return UpdateLoadingOperations(_loadingAsyncOperation);

            if (_loadingAsyncOperation.asset != null)

            {
                LoadedUnityObject = _loadingAsyncOperation.asset;
                FireComplete();
                yield break;
            }
            

            Error = new Exception("Asset not in unity resource folder path: " + Uri);
                FireComplete();
            
        }

        protected override void InnerCancelRequest()
        {
            _loadingOperationForMainBundle.CancelLoading();
            foreach (var loadingOperationForDependentBundle in _loadingOperationForDependentBundles)
            {
                loadingOperationForDependentBundle.CancelLoading();
            }

            //TODO: make cancel loading
        }

        #endregion
    }
}
#endif