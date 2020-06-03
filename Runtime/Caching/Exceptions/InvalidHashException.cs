namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class InvalidHashException : AssetsSystemException
    {
        #region Properties
        public string AssetKey { get; }
        public string Path { get; }
        public string ExpectedHash { get; }
        public string ActualHash { get; }
        #endregion

        #region Constructors
        public InvalidHashException( string assetKey, string path, string expectedHash, string actualHash )
            : base( $"Cached key {assetKey} with path {path} hash {expectedHash} is not equals hash from disk {actualHash}. Please remove or override file!" )
        {
            AssetKey = assetKey;
            Path = path;
            ExpectedHash = expectedHash;
            ActualHash = actualHash;
        }
        #endregion
    }
}
