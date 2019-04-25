#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class WebRequestLoader : AbstractMemoryCachedLoader<WebRequestWorker>
    {
        private readonly List<IResourceDataCreator> _resourceDataCreators = new List<IResourceDataCreator>();
        private readonly WebRequestSettings _webRequestSettings;

        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }

        public WebRequestLoader(ICoroutineManager coroutineManager, string supportsMask = "https", WebRequestSettings webRequestSettings = null,
            IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportsMask,
            coroutineManager, new ResourceMemoryCache())
        {
            _webRequestSettings = webRequestSettings;
            AntiCacheUrlResolver = antiCacheUrlResolver;
        }

        public WebRequestLoader(ICache<object> memoryCacheOverride, ICoroutineManager coroutineManager, string supportsMask = "https",
            WebRequestSettings webRequestSettings = null, IAntiCacheUrlResolver antiCacheUrlResolver = null) : base(supportsMask, coroutineManager, memoryCacheOverride)
        {
            _webRequestSettings = webRequestSettings;
            AntiCacheUrlResolver = antiCacheUrlResolver;
        }

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            return IsCached(uri);
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            if (IsCached(uri))
            {
                return GetCachedResource<T>(owner, uri);
            }

            throw new InvalidOperationException("Cannot load WebRequest synchronously");
        }

        public override void DestroyResource(object resource)
        {
            foreach (var resourceDataCreator in _resourceDataCreators)
            {
                if (resourceDataCreator.Supports(resource.GetType()))
                {
                    resourceDataCreator.Destroy(resource);
                    break;
                }
            }
        }

        public void RegisterResourceCreator(IResourceDataCreator resourceDataCreator)
        {
            _resourceDataCreators.Add(resourceDataCreator);
        }


        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {
            return _memoryCache.Get(owner, uri) as TResource;
        }

        protected override WebRequestWorker CreateWorker<TResource>(string uri)
        {
            return new WebRequestWorker(uri, HandleLoadingComplete<TResource>, _coroutineManager, AntiCacheUrlResolver, _webRequestSettings);
        }

        protected override void ValidateInputData<TResource>(string uri)
        {
            foreach (var resourceDataCreator in _resourceDataCreators)
            {
                if (resourceDataCreator.Supports(typeof(TResource)))
                {
                    return;
                }
            }

            throw new DataCreatorResourceSystemException(
                string.Format(
                    "Bind creator for {0} - not found in ResourcesStorage.You need implement IResourceDataCreator for type {0} and registry in _resourceStorage. Use method RegisterResourceCreator<T>(). URI: {1}",
                    typeof(TResource), uri), typeof(TResource));
        }

        protected override TResource GetResourceFromWorker<TResource>(WebRequestWorker worker)
        {
            TResource resource;
            foreach (var resourceDataCreator in _resourceDataCreators)
            {
                if (resourceDataCreator.Supports(typeof(TResource)))
                {
                    resource = resourceDataCreator.Create<TResource>(worker.LoadedData);
                    foreach (var loadingOperation in worker.LoadingOperations)
                    {
                        _memoryCache.Add(loadingOperation.Owner, worker.Uri, resource);
                    }

                    return resource;
                }
            }

            return null;
        }
    }
}
#endif