using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class UnityAssetFromBundleLoaderException : AssetSystemException
    {
        #region Constructors
        public UnityAssetFromBundleLoaderException( string message ) : base( message )
        {
        }

        public UnityAssetFromBundleLoaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
