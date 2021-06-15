namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetInFileManifestMissingException : AssetsSystemException
    {
        public string AssetName { get; }

        public AssetInFileManifestMissingException( string name )
            : base( $"Asset '{name}' is missing in manifest file" )
        {
            AssetName = name;
        }
    }
}
