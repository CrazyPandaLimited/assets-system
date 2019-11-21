using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Caching
{
    public class AssetsFromBundlesWithRefcountCacheController : AssetsWithRefcountCacheController
    {
        #region Private Fields
        private AssetBundleManifest _manifest;
        private BundlesCacheWithRefcountCacheController _bundlesCache;
        #endregion

        #region Constructors
        public AssetsFromBundlesWithRefcountCacheController( AssetBundleManifest manifest, BundlesCacheWithRefcountCacheController bundlesCache, int startCapacity = 200 ) : base( startCapacity )
        {
            _manifest = manifest ?? throw new ArgumentNullException( nameof(manifest) );
            _bundlesCache = bundlesCache ?? throw new ArgumentNullException( nameof(bundlesCache) );
        }
        #endregion

        #region Protected Members
        protected override void DestroyFromMemory( string assetName, object asset )
        {
            base.DestroyFromMemory( assetName, asset );
            var bundleInfo = _manifest.GetBundleByAssetName( assetName );
            _bundlesCache.Remove( bundleInfo.Name );
        }
        #endregion
    }
}
