using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class ImpossibleToLoadAssetException : Exception
    {
        #region Constructors
        public ImpossibleToLoadAssetException( string message ) : base( message )
        {
        }
        #endregion
    }
}
