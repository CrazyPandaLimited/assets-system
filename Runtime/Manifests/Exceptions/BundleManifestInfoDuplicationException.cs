using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class BundleManifestInfoDuplicationException : Exception
    {
        #region Constructors
        public BundleManifestInfoDuplicationException( string message ) : base( message )
        {
        }
        #endregion
    }
}
