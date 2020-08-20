using System;
using System.Text;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class StringDataCreator : IAssetDataCreator
    {
        #region Public Members
        public bool Supports( Type requestedAssetType )
        {
            return requestedAssetType == typeof( String );
        }

        public TAssetType Create< TAssetType >( byte[ ] data ) where TAssetType : class
        {
            return Encoding.UTF8.GetString( data ) as TAssetType;
        }

        public object Create( byte[ ] data, Type type )
        {
            return Create< string >( data );
        }
        #endregion
    }
}
