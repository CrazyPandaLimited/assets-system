using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class UnityAssetFolderLoaderException : AssetSystemException
    {
        #region Constructors
        public UnityAssetFolderLoaderException( string message ) : base( message )
        {
        }

        public UnityAssetFolderLoaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
