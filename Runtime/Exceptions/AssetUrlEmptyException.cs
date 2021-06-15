using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetUrlEmptyException : Exception
    {
        public AssetUrlEmptyException( string message ) : base( message )
        {
        }
    }
}
