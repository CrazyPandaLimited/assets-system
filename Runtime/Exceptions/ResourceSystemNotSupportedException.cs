#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ResourceSystemNotSupportedException:Exception
    {
        public ResourceSystemNotSupportedException()
        {
        }

        public ResourceSystemNotSupportedException(string message) : base(message)
        {
        }
    }
}
#endif