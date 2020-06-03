namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetNotFoundInCacheException : AssetsSystemException
    {
        #region Properties
        public string AssetKey { get; }
        #endregion

        #region Constructors
        public AssetNotFoundInCacheException( string assetKey )
            : base( $"Asset with key '{assetKey}' not found in cache" )
        {
            AssetKey = assetKey;
        }
        #endregion
    }
}
