namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AnticacheNotFoundException : AssetsSystemException
    {
        #region Properties
        public string Uri { get; }
        public string AnticacheAssetKey { get; }
        #endregion

        #region Constructors
        public AnticacheNotFoundException( string uri, string anticacheAssetKey )
            : base( $"URI: {uri} ResorceKey: {anticacheAssetKey}" )
        {
            Uri = uri;
            AnticacheAssetKey = anticacheAssetKey;
        }
        #endregion
    }
}
