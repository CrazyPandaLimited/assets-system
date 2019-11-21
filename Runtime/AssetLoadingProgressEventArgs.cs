using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetLoadingProgressEventArgs : EventArgs
    {
        #region Properties
        public string Key { get; private set; }
        public float Progress { get; private set; }
        #endregion

        #region Constructors
        public AssetLoadingProgressEventArgs( string key, float progress )
        {
            Key = key;
            Progress = progress;
        }
        #endregion
    }
}
