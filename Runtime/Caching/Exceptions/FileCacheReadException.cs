using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class FileCacheReadException : AssetsSystemException
    {
        public string AssetKey { get; }
        public string Path { get; }

        public FileCacheReadException( string assetKey, string path, Exception innerException )
            : base( $"Exception while reading a key {assetKey} with path {path}", innerException )
        {
            AssetKey = assetKey;
            Path = path;
        }
    }
}
