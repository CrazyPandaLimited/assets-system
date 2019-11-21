using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class WebRequestLoaderException : AssetSystemException
    {
        #region Constructors
        public WebRequestLoaderException( string message ) : base( message )
        {
        }

        public WebRequestLoaderException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
