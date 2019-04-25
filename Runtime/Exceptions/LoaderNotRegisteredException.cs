#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class LoaderNotRegisteredException:Exception
    {
        public LoaderNotRegisteredException(string message) : base(message)
        {
        }
    }
}
#endif