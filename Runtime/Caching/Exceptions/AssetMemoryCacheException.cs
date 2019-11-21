using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetMemoryCacheException : Exception
    {
        #region Constructors
        public AssetMemoryCacheException( string message ) : base( message )
        {
        }

        public AssetMemoryCacheException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
