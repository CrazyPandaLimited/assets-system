using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Tests
{
    static class ResourceStorageTestUtils
    {
        public static string ConstructTestBundlesUrl(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return "https://crazypandalimited.github.io/assets-system/files-for-testing/bundles";
            }

            return string.Format("https://crazypandalimited.github.io/assets-system/files-for-testing/bundles/{0}", filename);
        }

        public static string ConstructTestUrl(string filename)
        {
            return string.Format("https://crazypandalimited.github.io/assets-system/files-for-testing/{0}", filename);
        }

        public static bool IsBundleLoaded(string bundleName)
        {
            foreach (var ab in AssetBundle.GetAllLoadedAssetBundles())
            {
                Debug.Log(ab.name);
                if (ab.name == bundleName) return true;
            }

            return false;
        }
    }
}