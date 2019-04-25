#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class AssetInFileManifestMissingException : Exception
    {
        public string resourceName;
        public AssetInFileManifestMissingException(string name)
        {
            resourceName = name;
        }
    }
}
#endif