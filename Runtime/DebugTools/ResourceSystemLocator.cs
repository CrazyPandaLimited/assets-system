#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{

    public class ResourceSystemLocator
    {
        public static List<IResourceStoageDebug> ResourceStorageInstances
        {
            get
            {
                #if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS_TEST_MODE
                return DebugToolsDemoData.FakeData;
                #endif
                
                return _resourceStorageInstances;
            }
        }

        private static List<IResourceStoageDebug> _resourceStorageInstances;
        
        public static event Action ResourceStorageInstancesChanged;
        public static void RegisterInstance(IResourceStoageDebug resourceStorage)
        {
            if (_resourceStorageInstances == null)
            {
                _resourceStorageInstances = new List<IResourceStoageDebug>(1);
            }

            if (_resourceStorageInstances.Contains(resourceStorage))
            {
                return;
            }
            
            _resourceStorageInstances.Add(resourceStorage);
            if (ResourceStorageInstancesChanged != null)
            {
                ResourceStorageInstancesChanged();
            }
        }

        public static void ReleaseInstance(IResourceStoageDebug resourceStorage)
        {
            if (_resourceStorageInstances == null || !_resourceStorageInstances.Contains(resourceStorage))
            {
                return;
            }
            _resourceStorageInstances.Remove(resourceStorage);
            if (ResourceStorageInstancesChanged != null)
            {
                ResourceStorageInstancesChanged();
            }
        }

        public static void ForceReleaseAll()
        {
            if (_resourceStorageInstances == null)
            {
                return;
            }
            
            _resourceStorageInstances.Clear();
            
            if (ResourceStorageInstancesChanged != null)
            {
                ResourceStorageInstancesChanged();
            }
        }
    }
}
#endif
