namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class BundleInfoDuplicateException : AssetsSystemException
    {
        public string BundleInfoKey { get; }

        public BundleInfoDuplicateException( string bundleInfoKey )
            : base( $"Attempt to add duplicate BundleInfo with key {bundleInfoKey}" )
        {
            BundleInfoKey = bundleInfoKey;
        }
    }
}
