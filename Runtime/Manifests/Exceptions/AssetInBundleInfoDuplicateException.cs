namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetInBundleInfoDuplicateException : AssetsSystemException
    {
        #region Properties
        public string AssetInBundleInfoKey { get; }
        #endregion

        #region Constructors
        public AssetInBundleInfoDuplicateException( string assetInBundleInfoKey )
            : base( $"Attempt to add duplicate AssetInBundleInfo with key {assetInBundleInfoKey}" )
        {
            AssetInBundleInfoKey = assetInBundleInfoKey;
        }
        #endregion
    }
}
