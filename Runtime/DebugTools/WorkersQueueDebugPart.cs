#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using System.Collections.Generic;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class WorkersQueue : IWorkersQueueDebug
    {
        public int DebugMaxSimultaneousResouceWorkers
        {
            get { return _maxSimultaneousResouceWorkers; }
        }

        public List<IResourceWorkerDebug> DebugWorkersInWaitingState
        {
            get { return _workersInWaitingState.ConvertAll(input => (IResourceWorkerDebug) input); }
        }

        public List<IResourceWorkerDebug> DebugWorkersInProcessState
        {
            get { return _workersInProcessState.ConvertAll(input => (IResourceWorkerDebug) input); }
        }
    }
}
#endif