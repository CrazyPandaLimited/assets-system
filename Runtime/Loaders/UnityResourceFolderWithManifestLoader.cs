#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.Utils;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class UnityResourceFolderWithManifestLoader : UnityResourceFolderLoader
    {
        public AssetsManifest<AssetInfo> Manifest;

        #region Constructors

        public UnityResourceFolderWithManifestLoader(ICoroutineManager coroutineManager, string supportsMask = "UnityResourceFolder") : base(coroutineManager,
            supportsMask)
        {
            Manifest = new AssetsManifest<AssetInfo>();
        }

        public UnityResourceFolderWithManifestLoader(ICache<object> memoryCacheOverride, ICoroutineManager coroutineManager,
            string supportsMask = "UnityResourceFolder") : base(
            memoryCacheOverride, coroutineManager, supportsMask)
        {
            Manifest = new AssetsManifest<AssetInfo>();
        }

        #endregion

        #region Public Members

        public override bool CanLoadResourceImmediately<TResource>(object owner, string uri)
        {
            return Manifest.HasResource(uri);
        }

        public override T LoadResourceImmediately<T>(object owner, string uri)
        {
            if (!Manifest.HasResource(uri))
            {
                throw new
                    ResourceSystemException(string.Format(@"You try to load '{0}' of type {1}. This resource not found in manifest.", uri, typeof(T).CSharpName()));
            }

            return base.LoadResourceImmediately<T>(owner, uri);
        }
        
        protected override UnityResourceFolderWorker CreateWorker<TResource>(string uri)
        {
            if (!Manifest.HasResource(uri))
            {
                throw new
                    ResourceSystemException(string.Format(@"You try to load '{0}' of type {1}. This resource not found in manifest.", uri, typeof(TResource).CSharpName()));
            }

            return base.CreateWorker<TResource>(uri);
        }

        #endregion
    }
}
#endif