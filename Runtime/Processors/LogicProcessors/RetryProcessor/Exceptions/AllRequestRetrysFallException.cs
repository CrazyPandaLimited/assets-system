using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AllRequestRetrysFallException : Exception
    {
        #region Constructors
        public AllRequestRetrysFallException()
        {
        }

        public AllRequestRetrysFallException( string message ) : base( message )
        {
        }
        #endregion
    }
}
