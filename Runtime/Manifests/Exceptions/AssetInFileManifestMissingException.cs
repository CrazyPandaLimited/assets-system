namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetInFileManifestMissingException : AssetsSystemException
    {
        #region Properties
        public string AssetName { get; }
        #endregion

        #region Constructors
        public AssetInFileManifestMissingException( string name )
            : base( $"Asset '{name}' is missing in manifest file" )
        {
            AssetName = name;
        }
        #endregion
    }
}
