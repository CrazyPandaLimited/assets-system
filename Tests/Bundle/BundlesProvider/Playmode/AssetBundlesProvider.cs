using System.Collections.Generic;
using CrazyPanda.UnityCore.DeliverySystem;
using UnityEngine;

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public sealed class AssetBundlesProvider : CrazyPanda.UnityCore.DeliverySystem.AssetBundlesProvider
    {
        public override IEnumerable< string > AssetBundles
        {
            get
            {
                var pathPrefix = $"Assets/UnityCoreSystems/ResourcesSystem/Tests/Bundle/{this.GetPlatformPrefix()}";
                
                yield return $"{pathPrefix}/bundletest_0.bundle";
                yield return $"{pathPrefix}/bundletest_1.bundle";
                yield return $"{pathPrefix}/experiment_aliceslots.bundle";
            }            
        }

#if !UNITY_EDITOR        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static async void CopyAssets()
        {
            var assetBundlesProvider = new AssetBundlesProvider();
            await assetBundlesProvider.CopyAssetBundlesToDataPath();
        }
#endif
    }
}