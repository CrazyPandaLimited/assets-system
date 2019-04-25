#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class ResourceMemoryCache : ICache<object>
    {
        #region Private Fields

        private readonly Dictionary<string, ResourceReferenceInfo> _assets;

        #endregion

        #region Constructors

        public ResourceMemoryCache(int startCapacity = 200)
        {
            _assets = new Dictionary<string, ResourceReferenceInfo>(startCapacity);
        }

        #endregion

        #region Public Members

        public bool Contains(string key)
        {
            return _assets.ContainsKey(ValidateKey(key));
        }

        public void Add(object owner, string key, object resource)
        {
            key = ValidateKey(key);
            ResourceReferenceInfo resourceReference;
            if (_assets.TryGetValue(key, out resourceReference))
            {
                if (!resourceReference.ContainsOwner(owner))
                {
                    resourceReference.AddOwner(owner);
                }
            }
            else
            {
                resourceReference = new ResourceReferenceInfo(resource);
                resourceReference.AddOwner(owner);
                _assets.Add(key, resourceReference);
            }
        }

        public object Get(object owner, string key)
        {
            key = ValidateKey(key);
            ResourceReferenceInfo resourceInfo = null;
            if (_assets.TryGetValue(key, out resourceInfo))
            {
                if (!resourceInfo.ContainsOwner(owner))
                {
                    resourceInfo.AddOwner(owner);
                }

                return resourceInfo.Resource;
            }

            throw new ResourceMemoryCacheException(string.Format("Missing resource in cach. Resource name: {0}", key));
        }

        public IEnumerator AddAsync(object owner, string key, object resource)
        {
            throw new ResourceSystemNotSupportedException();
        }

        public ICacheGettingOperation<object> GetAsync(object owner, string key)
        {
            throw new ResourceSystemNotSupportedException();
        }

        public List<string> GetUnusedResourceNames()
        {
            var removedAssetsNames = new List<string>();
            
            foreach (var resourceReferenceInfo in _assets)
            {
                var removedOwners = new List<WeakReference>();
                foreach (var weakReference in resourceReferenceInfo.Value.Owners)
                {
                    if (weakReference.Target == null || weakReference.Target.ToString() == "null")
                    {
                        removedOwners.Add(weakReference);
                    }
                }

                foreach (var weakReference in removedOwners)
                {
                    resourceReferenceInfo.Value.Owners.Remove(weakReference);
                }

                if (resourceReferenceInfo.Value.Owners.Count == 0)
                {
                    removedAssetsNames.Add(resourceReferenceInfo.Key);
                }
            }

            return removedAssetsNames;
        }

        public List<string> GetOwnerResourcesNames(object owner)
        {
            List<string> keysToRemove = new List<string>(0);
            foreach (var resourceReferenceInfo in _assets)
            {
                if (resourceReferenceInfo.Value.ContainsOwner(owner))
                {
                    keysToRemove.Add(resourceReferenceInfo.Key);
                }
            }
            return keysToRemove;
        }

        public object ReleaseResource(object owner, string key)
        {
            var validName = ValidateKey(key);
            if (!Contains(validName))
            {
                return null;
            }

            var assetReferenceInfo = _assets[validName];
            var removedOwners = GetRemovedWeekRefereces(assetReferenceInfo, owner);

            for (var i = 0; i < removedOwners.Count; i++)
            {
                assetReferenceInfo.Owners.Remove(removedOwners[i]);
            }

            if (assetReferenceInfo.Owners.Count == 0)
            {
                _assets.Remove(validName);
                return assetReferenceInfo.Resource;
            }

            return null;
        }

        public List<object> ReleaseResources(object owner, List<string> keys)
        {
            List<object> resourcesToRelease = new List<object>(0);
            object tmp = null;
            foreach (var currentAssetName in keys)
            {
                tmp = ReleaseResource(owner, currentAssetName);
                if (tmp != null)
                {
                    resourcesToRelease.Add(tmp);
                }
            }
            return resourcesToRelease;
        }

        public List<object> ForceReleaseResources(List<string> keys)
        {
            List<object> resources = new List<object>();
            object resourceTmp = null;
            foreach (var currentAssetName in keys)
            {
                resourceTmp = ForceReleaseResource(currentAssetName);
                if (resourceTmp != null)
                {
                    resources.Add(resourceTmp);
                }
            }
            return resources;
        }

        public object ForceReleaseResource(string key)
        {
            
            var validName = ValidateKey(key);
            if (!Contains(validName))
            {
                return null;
            }

            var assetReferenceInfo = _assets[validName];
            _assets.Remove(validName);
            assetReferenceInfo.Owners.Clear();
            return assetReferenceInfo.Resource;
        }

        public List<string> GetAllResorceNames()
        {
            var result = new List<string>(_assets.Count);
            foreach (var asset in _assets)
            {
                result.Add(asset.Key);
            }

            return result;
        }

        public List<object> ReleaseAllResources()
        {
            List<object> resources = new List<object>();
            foreach (var asset in _assets)
            {
                asset.Value.Owners.Clear();
                resources.Add(asset.Value.Resource);
            }

            _assets.Clear();
            return resources;
        }

        private List<WeakReference> GetRemovedWeekRefereces(ResourceReferenceInfo resourceInfo, object owner)
        {
            var removedOwners = new List<WeakReference>();
            foreach (var weakReference in resourceInfo.Owners)
            {
                if (weakReference.Target == owner || weakReference.Target == null || weakReference.Target.ToString() == "null")
                {
                    removedOwners.Add(weakReference);
                }
            }

            return removedOwners;
        }

        #endregion

        #region Private Members

        private string ValidateKey(string key)
        {
            return key;
            //return key.ToLower();
        }

        #endregion
    }
}
#endif