using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class MetaDataNotContainsReferenceObjectForAsset : Exception
    {
        #region Constructors
        public MetaDataNotContainsReferenceObjectForAsset( string message ) : base( message )
        {
        }
        #endregion
    }
}
