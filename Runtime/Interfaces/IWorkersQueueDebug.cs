#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
    public interface IWorkersQueueDebug
    {
        int DebugMaxSimultaneousResouceWorkers { get; }
        List<IResourceWorkerDebug> DebugWorkersInWaitingState { get; }
        List<IResourceWorkerDebug> DebugWorkersInProcessState { get; }
    }
#endif
}
#endif