using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Caching
{
    public class BundlesCacheWithRefcountCacheController : AssetsWithRefcountCacheController
    {
        public AssetBundle Get( string assetName, object reference )
        {
            return Get< AssetBundle >( assetName, reference);
        }

        protected override void DestroyFromMemory( string assetName, object asset )
        {
            ( ( AssetBundle ) asset ).Unload( false );
        }
    }
}
