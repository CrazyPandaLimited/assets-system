using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class SyncLoadException : Exception
    {
        #region Constructors
        public SyncLoadException( string message ) : base( message )
        {
        }

        public SyncLoadException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
