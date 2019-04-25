#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using  System.Collections.Generic;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{

    public partial class ResourceStorage : IResourceStoageDebug
    {
        public List<ILoaderDebugger> DebugAllLoaders
        {
            get { return _loaders.ConvertAll(input => (ILoaderDebugger) input); }
        }

        public IWorkersQueueDebug DebugWorkersQueue
        {
            get { return (IWorkersQueueDebug) _workersQueue; }
        }
    }
}
#endif