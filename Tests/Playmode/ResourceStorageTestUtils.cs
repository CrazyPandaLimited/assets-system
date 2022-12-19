using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Tests
{
    static class ResourceStorageTestUtils
    {
        private const string TestUrl = "https://unitycore.dev.crazypanda.ru/s/remote-asset-bundles";
        
        public static string ConstructTestBundlesUrl(string filename = null)
        {
            if (string.IsNullOrEmpty(filename))
            {
                return $"{TestUrl}/{GetPlatformPrefix()}";
            }

            return $"{TestUrl}/{GetPlatformPrefix()}/{filename}";
        }

        public static string ConstructTestUrl(string filename)
        {
            return $"{TestUrl}/{filename}";
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

        private static string GetPlatformPrefix()
        {
#if UNITY_STANDALONE_WIN
            return "StandaloneWindows";
#elif UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_WEBGL        
            return "WebGL";
#endif
            return "Editor";
        }        
    }
}