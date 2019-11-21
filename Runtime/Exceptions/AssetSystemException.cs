using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetSystemException : Exception
    {
        #region Constructors
        public AssetSystemException( string message ) : base( message )
        {
        }

        public AssetSystemException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
