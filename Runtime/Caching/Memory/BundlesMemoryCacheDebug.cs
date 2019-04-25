#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using System.Collections.Generic;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class BundlesMemoryCache : ICacheDebugger
    {
        public CacheObjectDebugInfo[] GetCachedObjectsDebugInfo()
        {
            CacheObjectDebugInfo[] result = new CacheObjectDebugInfo[_bundles.Count];
            int i = 0;
            foreach (var asset in _bundles)
            {
                result[i].key = asset.Key;
                result[i].resourceType = "AssetBundle";

                result[i].owners = new List<string>();
                i++;
            }

            return result;
        }
    }
}
#endif