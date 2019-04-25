using System.Collections.Generic;

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class DefaultAntiCacheUrlResolver : IAntiCacheUrlResolver
    {
        private Dictionary<string, string> _perResourceAntiCache;
        private string _defaultAnticache;
        
        public void UpdateDefault(string anticache)
        {
            _defaultAnticache = anticache;
        }

        public void AddOrUpdateKey(string key, string anticache)
        {
            if (_perResourceAntiCache.ContainsKey(key))
            {
                _perResourceAntiCache[key] = anticache;
                return;
            }

            _perResourceAntiCache.Add(key, anticache);
        }

        public void AddOrUpdateKeys(Dictionary<string, string> perResourceAnticaches)
        {
            foreach (var perResourceAnticach in perResourceAnticaches)
            {
                AddOrUpdateKey(perResourceAnticach.Key, perResourceAnticach.Value);
            }
        }

        public string ResolveURL(string uri, string anticacheResourceKey = "")
        {
            if (string.IsNullOrEmpty(anticacheResourceKey))
            {
                return uri + _defaultAnticache;
            }

            if (_perResourceAntiCache.ContainsKey(anticacheResourceKey))
            {
                return uri + _perResourceAntiCache[anticacheResourceKey];
            }
            throw new AnticacheNotFoundException("URI:" + uri + " ResorceKey:" + anticacheResourceKey);
        }
    }
}
#endif