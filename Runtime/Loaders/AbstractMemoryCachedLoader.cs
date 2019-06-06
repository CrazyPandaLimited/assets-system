#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

#endif

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public abstract class AbstractMemoryCachedLoader<TWorker, TCache> : BaseLoader<TWorker, TCache>
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
        , ILoaderDebugger
#endif
        where TWorker : IResourceWorker
    {

        protected AbstractMemoryCachedLoader(string supportsMask, ICoroutineManager coroutineManager, ICache<TCache> memoryCache) : base(supportsMask, coroutineManager)
        {
            _memoryCache = memoryCache;
        }
    }
}
#endif