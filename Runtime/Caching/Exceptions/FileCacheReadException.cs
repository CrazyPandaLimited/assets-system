using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class FileCacheReadException : AssetsSystemException
    {
        #region Properties
        public string AssetKey { get; }
        public string Path { get; }
        #endregion

        #region Constructors
        public FileCacheReadException( string assetKey, string path, Exception innerException )
            : base( $"Exception while reading a key {assetKey} with path {path}", innerException )
        {
            AssetKey = assetKey;
            Path = path;
        }
        #endregion
    }
}
