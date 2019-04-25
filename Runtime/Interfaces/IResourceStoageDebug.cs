#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS

using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
    public interface IResourceStoageDebug
    {
        List<ILoaderDebugger> DebugAllLoaders { get; }
        IWorkersQueueDebug DebugWorkersQueue { get; }
    }
}
#endif