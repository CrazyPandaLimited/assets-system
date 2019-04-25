#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
    public partial class ResourceMemoryCache : ICacheDebugger
    {
        public CacheObjectDebugInfo[] GetCachedObjectsDebugInfo()
        {
            CacheObjectDebugInfo[] result = new CacheObjectDebugInfo[_assets.Count];
            int i = 0;
            foreach (var asset in _assets)
            {
                result[i].key = asset.Key;
                result[i].resourceType = asset.Value.Resource.GetType().ToString();
                
                result[i].owners = new List<string>(asset.Value.Owners.Count);
                foreach (var ownerRef in asset.Value.Owners)
                {
                    if (ownerRef == null || ownerRef.Target == null)
                    {
                        continue;
                    }
                    result[i].owners.Add(ownerRef.Target.ToString());
                }
                i++;
            }

            return result;
        }
    }
#endif
}
#endif