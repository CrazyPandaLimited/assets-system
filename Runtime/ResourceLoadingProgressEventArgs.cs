#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ResourceLoadingProgressEventArgs : EventArgs
    {
        #region Properties
        public string Key { get; private set; }
        public float Progress { get; private set; }
        #endregion

        #region Constructors
        public ResourceLoadingProgressEventArgs( string key, float progress )
        {
            Key = key;
            Progress = progress;
        }
        #endregion
    }
}
#endif