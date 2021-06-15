namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetNotFoundInCacheException : AssetsSystemException
    {
        public string AssetKey { get; }

        public AssetNotFoundInCacheException( string assetKey )
            : base( $"Asset with key '{assetKey}' not found in cache" )
        {
            AssetKey = assetKey;
        }
    }
}
