using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Caching
{
    public class BundlesCacheWithRefcountCacheController : AssetsWithRefcountCacheController
    {
        #region Public Members        
        public AssetBundle Get( string assetName, object reference )
        {
            return Get< AssetBundle >( assetName, reference);
        }
        #endregion

        #region Protected Members
        protected override void DestroyFromMemory( string assetName, object asset )
        {
            ( ( AssetBundle ) asset ).Unload( false );
        }
        #endregion
    }
}
