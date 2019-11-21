using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ConnectionAlreadyExistException : Exception
    {
        #region Constructors
        public ConnectionAlreadyExistException( string message ) : base( message )
        {
        }

        public ConnectionAlreadyExistException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
