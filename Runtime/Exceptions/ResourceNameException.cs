#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ResourceNameException:Exception
    {
        public ResourceNameException(string message) : base(message)
        {
        }
    }
}
#endif