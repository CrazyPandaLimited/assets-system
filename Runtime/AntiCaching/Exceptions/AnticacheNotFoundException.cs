namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AnticacheNotFoundException : AssetsSystemException
    {
        public string Uri { get; }
        public string AnticacheAssetKey { get; }

        public AnticacheNotFoundException( string uri, string anticacheAssetKey )
            : base( $"URI: {uri} ResorceKey: {anticacheAssetKey}" )
        {
            Uri = uri;
            AnticacheAssetKey = anticacheAssetKey;
        }
    }
}
