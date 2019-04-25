#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ResourceOwnerException:Exception
    {
        public ResourceOwnerException(string message) : base(message)
        {
        }
    }
}
#endif