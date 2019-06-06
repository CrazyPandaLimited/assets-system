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

        private ResourceStorage _resourceStorage;
        private readonly Dictionary<string, AssetBundle> _bundles;

        #endregion

        #region Constructors

        public BundlesMemoryCache(ResourceStorage _resourceStorage, int startCapacity = 200)
        {
            _bundles = new Dictionary<string, AssetBundle>(startCapacity);
            SetResourceStorage(_resourceStorage);
        }

        public void SetResourceStorage(ResourceStorage _resourceStorage)
        {
            this._resourceStorage = _resourceStorage;
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

        public List<string> GetOwnerResourcesNames(object owner)
        {
            return new List<string>(0);
        }

        public Dictionary<string, object> ReleaseAllResources()
        {
            return ReleaseResources(_bundles.Keys.ToList());
        }

        public Dictionary<string, object> ReleaseResource(object owner, string uri)
        {
            return ReleaseResources(new List<string>() { uri });
        }

        public Dictionary<string, object> ReleaseResources(object owner, List<string> uris)
        {
            return ReleaseResources(uris);
        }

        public Dictionary<string, object> ForceReleaseResource(string uri)
        {
            return ReleaseResources(new List<string>() { uri });
        }

        public Dictionary<string, object> ForceReleaseResources(List<string> uris)
        {
            return ReleaseResources(uris);
        }

        public Dictionary<string, object> ReleaseAllOwnerResources(object owner)
        {
            return new Dictionary<string, object>();
        }

        public Dictionary<string, object> ReleaseUnusedResources()
        {
            return new Dictionary<string, object>();
        }

        private Dictionary<string, object> ReleaseResources(List<string> keys)
        {
            Dictionary<string, object> releasedBundles = new Dictionary<string, object>();
            AssetBundle tmp = null;
            foreach (var bundleName in keys)
            {
                tmp = ReleaseResource(bundleName);
                if (tmp != null)
                {
                    if (!releasedBundles.ContainsKey(bundleName))
                    {
                        releasedBundles.Add(bundleName, tmp);
                    }
                    else
                    {
                        Debug.LogError("Removing resource with the same key = " + bundleName);
                    }
                }
            }
            return releasedBundles;
        }

        private AssetBundle ReleaseResource(string key)
        {
            var validBundleName = ValidateKey(key);
            if (!Contains(validBundleName))
            {
                return null;
            }

            var loader = _resourceStorage.GetResourceLoader<UnityResourceFromBundleLoader>();
            if (!loader.HasWorkersDependentOnAssetBundle(validBundleName))
            {
                var bundle = _bundles[validBundleName];
                _bundles.Remove(validBundleName);
                return bundle;
            }

            return null;           
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