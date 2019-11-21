using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class ByteArrayDataCreator : IAssetDataCreator
    {
        #region Public Members
        public bool Supports( Type requestedAssetType )
        {
            return requestedAssetType == typeof( byte[ ] );
        }

        public TAssetType Create< TAssetType >( byte[ ] data ) where TAssetType : class
        {
            return data as TAssetType;
        }

        public object Create( byte[ ] data, Type type )
        {
            return data;
        }
        #endregion
    }
}
