#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
    public interface ICacheDebugger
    {
        CacheObjectDebugInfo[] GetCachedObjectsDebugInfo();

    }
}
#endif
