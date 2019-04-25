#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using CrazyPanda.UnityCore.CoroutineSystem;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class VersionedDiskCachedResourceWorker : WebRequestWorker
    {
        public AssetFileInfo AssetInfo { protected set; get; }
        private ICache<byte[]> _fileCache;
        private Action<VersionedDiskCachedResourceWorker> _onLoadingComplete;
        private bool _isLoadFromFileCache;
            
        public VersionedDiskCachedResourceWorker(string uri, AssetFileInfo assetInfo, ICache<byte[]> fileCache, Action<VersionedDiskCachedResourceWorker> onLoadingComplete,
            ICoroutineManager coroutineManager) :
            base(uri, null, coroutineManager, null)
        {
            AssetInfo = assetInfo;
            _fileCache = fileCache;
            _onLoadingComplete = onLoadingComplete;
            _isLoadFromFileCache = true;
        }

        public VersionedDiskCachedResourceWorker(AssetFileInfo assetInfo, string uri, Action<VersionedDiskCachedResourceWorker> onLoadingComplete,
            ICoroutineManager coroutineManager, IAntiCacheUrlResolver antiCacheUrlResolver, WebRequestSettings webRequestSettings = null) :
            base(uri, null, coroutineManager, antiCacheUrlResolver, webRequestSettings)
        {
            AssetInfo = assetInfo;
            _onLoadingComplete = onLoadingComplete;
            _isLoadFromFileCache = false;
        }

        protected override void FireComplete()
        {
            _onLoadingComplete(this);
        }

        protected override IEnumerator LoadProcess()
        {
            yield return _isLoadFromFileCache ? LoadProcessFromFileCache() : base.LoadProcess();
        }

        protected IEnumerator LoadProcessFromFileCache()
        {
            if (_fileCache == null)
            {
                Error = new Exception("Caching object is null");
                FireComplete();
                yield break;
            }

            var loadingProcess = _fileCache.GetAsync(null,Uri);

            yield return loadingProcess.StartProcess();

            LoadedData = loadingProcess.Result;
            FireComplete();
        }

        public override void Dispose()
        {
            base.Dispose();
            _fileCache = null;
            AssetInfo = null;
            _onLoadingComplete = null;
        }
    }
}
#endif