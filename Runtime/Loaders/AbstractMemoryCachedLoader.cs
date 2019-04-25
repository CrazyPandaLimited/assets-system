#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

#endif

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public abstract class AbstractMemoryCachedLoader<TWorker> : BaseLoader<TWorker>
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
        where TWorker : IResourceWorker
    {
        protected ICache<object> _memoryCache;

        protected AbstractMemoryCachedLoader(string supportsMask, ICoroutineManager coroutineManager, ICache<object> memoryCache) : base(supportsMask, coroutineManager)
        {
            _memoryCache = memoryCache;
        }

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        public virtual List<ICacheDebugger> DebugCaches
        {
            get
            {
                if (_memoryCache is ICacheDebugger) return new List<ICacheDebugger>(1) {(ICacheDebugger) _memoryCache};
                return new List<ICacheDebugger>(0);
            }
        }
#endif

        public override bool IsCached(string uri)
        {
            return _memoryCache.Contains(uri);
        }

        public override List<object> ReleaseAllFromCache(bool destroy = true)
        {   
            if (!destroy)
            {
                return _memoryCache.ReleaseAllResources();
            }

            var resourcesNames = _memoryCache.GetAllResorceNames();
            var resourcesForDestroy = _memoryCache.ReleaseAllResources();

            for (int i = 0; i < resourcesNames.Count; i++)
            {
                if (resourcesForDestroy[i] == null)
                {
                    continue;
                }
                
                CustomDestroyLogic(resourcesNames[i], resourcesForDestroy[i]);
            }
            return new List<object>();
        }

        public override object ReleaseFromCache(object owner, string uri, bool destroy = true)
        {
            if (!destroy)
            {
                return _memoryCache.ReleaseResource(owner, uri);
            }

            var resourcesForDestroy = _memoryCache.ReleaseResource(owner, uri);

            if (resourcesForDestroy == null)
            {
                return null;
            }

            CustomDestroyLogic(uri, resourcesForDestroy);
            return null;
        }

        public override object ForceReleaseFromCache(string uri, bool destroy = true)
        {
            if (!destroy)
            {
                return _memoryCache.ForceReleaseResource(uri);
            }

            var resourcesForDestroy = _memoryCache.ForceReleaseResource(uri);

            if (resourcesForDestroy == null)
            {
                return null;
            }
            CustomDestroyLogic(uri, resourcesForDestroy);
            return null;
        }

        public override List<object> ReleaseAllOwnerResourcesFromCache(object owner, bool destroy = true)
        {
            var ownerResources = _memoryCache.GetOwnerResourcesNames(owner);
            if (!destroy)
            {
                return _memoryCache.ReleaseResources(owner,ownerResources);
            }

            object tmp = null;
            foreach (var ownerResource in ownerResources)
            {
                tmp = _memoryCache.ReleaseResource(owner, ownerResource);
                if (tmp == null)
                {
                    continue;
                }
                CustomDestroyLogic(ownerResource, tmp);
            }

            return new List<object>();
        }

        public override List<object> RemoveUnusedFromCache(bool destroy = true)
        {
            var unusedResourceNames = _memoryCache.GetUnusedResourceNames();
            if (!destroy)
            {
                return _memoryCache.ForceReleaseResources(unusedResourceNames);
            }

            object tmp = null;
            foreach (var unusedResourceName in unusedResourceNames)
            {
                tmp = _memoryCache.ForceReleaseResource(unusedResourceName);
                if (tmp == null)
                {
                    continue;
                }
                CustomDestroyLogic(unusedResourceName, tmp);
            }
            return new List<object>();
        }

        protected virtual void CustomDestroyLogic(string resourceName, object resource)
        {
            DestroyResource(resource);
        }
    }
}
#endif