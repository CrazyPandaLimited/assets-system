using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Tests
{
    static class ResourceStorageTestUtils
    {
        public static string ConstructTestBundlesUrl(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return "http://unitycore.dev.crazypanda.ru/s/files-for-testing/bundles";
            }

            return string.Format("http://unitycore.dev.crazypanda.ru/s/files-for-testing/bundles/{0}", filename);
        }

        public static string ConstructTestUrl(string filename)
        {
            return string.Format("http://unitycore.dev.crazypanda.ru/s/files-for-testing/{0}", filename);
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