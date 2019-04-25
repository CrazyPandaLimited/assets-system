#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class FakeBundlesLoader : BaseLoader<LocalFolderBundleWorker>
    {
        #region Constructors

        public FakeBundlesLoader(string supportsMask) : base(supportsMask, null)
        {
        }

        #endregion

        #region Private Fields

        #endregion

        #region Public Members

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            return false;
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            return default(T);
        }

        public override bool IsCached(string uri)
        {
            return true;
        }

        public override List<object> ReleaseAllFromCache(bool destroy = true)
        {
            return new List<object>();
        }

        public override object ReleaseFromCache(object owner, string uri,bool destroy = true)
        {
            return null;
        }

        public override object ForceReleaseFromCache(string uri,bool destroy = true)
        {
            return null;
        }

        public override List<object> ReleaseAllOwnerResourcesFromCache(object owner,bool destroy = true)
        {
            return new List<object>();
        }

        public override List<object> RemoveUnusedFromCache(bool destroy = true)
        {
            return new List<object>();
        }

        public override void DestroyResource(object resource)
        {
            
        }

        #endregion

        #region Protected Members

        protected override TResource GetCachedResource<TResource>(object owner, string uri)
        {
            return default(TResource);
        }

        protected override void ValidateInputData<TResource>(string uri)
        {
        }

        protected override LocalFolderBundleWorker CreateWorker<TResource>(string uri)
        {
            return null;
        }

        protected override TResource GetResourceFromWorker<TResource>(LocalFolderBundleWorker worker)
        {
            return null;
        }

        #endregion
    }
}
#endif