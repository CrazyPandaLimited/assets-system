namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetInBundleInfoDuplicateException : AssetsSystemException
    {
        public string AssetInBundleInfoKey { get; }

        public AssetInBundleInfoDuplicateException( string assetInBundleInfoKey )
            : base( $"Attempt to add duplicate AssetInBundleInfo with key {assetInBundleInfoKey}" )
        {
            AssetInBundleInfoKey = assetInBundleInfoKey;
        }
    }
}
