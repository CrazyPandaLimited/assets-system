using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class BuilderException : Exception
    {
        #region Constructors
        public BuilderException( string message ) : base( message )
        {
        }

        public BuilderException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
