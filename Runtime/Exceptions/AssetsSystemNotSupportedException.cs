using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetsSystemNotSupportedException : Exception
    {
        #region Constructors
        public AssetsSystemNotSupportedException()
        {
        }

        public AssetsSystemNotSupportedException( string message ) : base( message )
        {
        }
        #endregion
    }
}
