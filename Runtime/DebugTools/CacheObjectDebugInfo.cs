#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
    public struct  CacheObjectDebugInfo
    {
        public string key;
        public string resourceType;
        public List<string> owners;
    }
#endif
}
#endif