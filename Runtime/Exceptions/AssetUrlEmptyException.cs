using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetUrlEmptyException : Exception
    {
        #region Constructors
        public AssetUrlEmptyException( string message ) : base( message )
        {
        }
        #endregion
    }
}
