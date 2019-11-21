using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetInFileManifestMissingException : Exception
    {
        #region Public Fields
        public string assetName;
        #endregion

        #region Constructors
        public AssetInFileManifestMissingException( string name )
        {
            assetName = name;
        }
        #endregion
    }
}
