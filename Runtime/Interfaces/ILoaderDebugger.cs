#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
    public interface ILoaderDebugger
    {
        List<ICacheDebugger> DebugCaches { get; }
        string SupportsMask { get; }
    }
#endif
}
#endif