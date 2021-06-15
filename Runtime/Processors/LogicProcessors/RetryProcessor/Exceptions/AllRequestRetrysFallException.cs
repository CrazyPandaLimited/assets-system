using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AllRequestRetrysFallException : Exception
    {
        public AllRequestRetrysFallException()
        {
        }

        public AllRequestRetrysFallException( string message ) : base( message )
        {
        }
    }
}
