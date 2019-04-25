using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class BundleManifestInfoDuplicationException:Exception
    {
        public BundleManifestInfoDuplicationException(string message) : base(message)
        {
        }
    }
}