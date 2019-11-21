using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class FileCachingException : Exception
    {
        #region Constructors
        public FileCachingException( string message ) : base( message )
        {
        }

        public FileCachingException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
