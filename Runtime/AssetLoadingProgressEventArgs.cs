using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetLoadingProgressEventArgs : EventArgs
    {
        public string Key { get; private set; }
        public float Progress { get; private set; }

        public AssetLoadingProgressEventArgs( string key, float progress )
        {
            Key = key;
            Progress = progress;
        }
    }
}
