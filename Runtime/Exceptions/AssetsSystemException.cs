using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class AssetsSystemException : Exception
    {
        #region Constructors
        public AssetsSystemException( string message ) : base( message )
        {
        }

        public AssetsSystemException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
