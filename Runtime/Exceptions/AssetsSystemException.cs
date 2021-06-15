using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class AssetsSystemException : Exception
    {
        public AssetsSystemException( string message ) : base( message )
        {
        }

        public AssetsSystemException( string message, Exception innerException ) : base( message, innerException )
        {
        }
    }
}
