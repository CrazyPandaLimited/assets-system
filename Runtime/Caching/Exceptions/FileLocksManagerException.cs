using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class FileLocksManagerException : Exception
    {
        #region Constructors
        public FileLocksManagerException( string message ) : base( message )
        {
        }
        #endregion
    }
}
