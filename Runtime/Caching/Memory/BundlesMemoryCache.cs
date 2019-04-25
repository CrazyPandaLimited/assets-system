#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class BundlesMemoryCache : ICache<AssetBundle>
    {
        #region Private Fields

        private readonly Dictionary<string, AssetBundle> _bundles;

        #endregion

        #region Constructors

        public BundlesMemoryCache(int startCapacity = 200)
        {
            _bundles = new Dictionary<string, AssetBundle>(startCapacity);
        }

        #endregion

        #region Public Members

        public bool Contains(string key)
        {
            return _bundles.ContainsKey(ValidateKey(key));
        }

        public List<string> GetAllResorceNames()
        {
            var result = new List<string>();
            foreach (var assetBundle in _bundles)
            {
                result.Add(assetBundle.Key);
            }

            return result;
        }

        public void Add(object owner, string key, AssetBundle bundle)
        {
            key = ValidateKey(key);

            if (_bundles.ContainsKey(key))
            {
                return;
            }

            _bundles.Add(key, bundle);
        }

        public AssetBundle Get(object owner, string key)
        {
            key = ValidateKey(key);
            AssetBundle bundle = null;
            if (_bundles.TryGetValue(key, out bundle))
            {
                return bundle;
            }

            throw new ResourceMemoryCacheException(string.Format("Missing bundle in cache. Bundle name: {0}", key));
        }

        public IEnumerator AddAsync(object owner, string key, AssetBundle resource)
        {
            throw new ResourceSystemNotSupportedException();
        }

        public ICacheGettingOperation<AssetBundle> GetAsync(object owner, string key)
        {
            throw new ResourceSystemNotSupportedException();
        }

        public List<string> GetUnusedResourceNames()
        {
            return new List<string>();
        }

        public List<AssetBundle> ReleaseUnusedResources()
        {
            return new List<AssetBundle>();
        }

        public List<string> GetOwnerResourcesNames(object owner)
        {
            return new List<string>(0);
        }

        public AssetBundle ReleaseResource(object owner, string key)
        {
            var validBundleName = ValidateKey(key);
            if (!Contains(validBundleName))
            {
                return null;
            }

            var bundle = _bundles[validBundleName];
            _bundles.Remove(validBundleName);
            return bundle;
        }

        public List<AssetBundle> ReleaseResources(object owner, List<string> keys)
        {
            return ForceReleaseResources(keys);
        }

        public List<AssetBundle> ForceReleaseResources(List<string> keys)
        {
            List<AssetBundle> releasedBundles = new List<AssetBundle>(0);
            AssetBundle tmp = null;
            foreach (var bundleName in keys)
            {
                tmp = ForceReleaseResource(bundleName);
                if (tmp != null)
                {
                    releasedBundles.Add(tmp);
                }
            }
            return releasedBundles;
        }

        public AssetBundle ForceReleaseResource(string key)
        {
            return ReleaseResource(null, key);
        }

        public List<AssetBundle> ReleaseAllResources()
        {
            var allBundles = _bundles.Keys.ToList();
            return ForceReleaseResources(allBundles);
        }

        #endregion

        #region Private Members

        private string ValidateKey(string key)
        {
            if (key == null)
            {
                return String.Empty;
            }

            return key; // key.ToLower();
        }

        #endregion
    }
}
#endif