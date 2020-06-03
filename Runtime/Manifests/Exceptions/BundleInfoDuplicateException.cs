namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class BundleInfoDuplicateException : AssetsSystemException
    {
        #region Properties
        public string BundleInfoKey { get; }
        #endregion

        #region Constructors
        public BundleInfoDuplicateException( string bundleInfoKey )
            : base( $"Attempt to add duplicate BundleInfo with key {bundleInfoKey}" )
        {
            BundleInfoKey = bundleInfoKey;
        }
        #endregion
    }
}
