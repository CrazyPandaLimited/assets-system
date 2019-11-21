#if CRAZYPANDA_UNITYCORE_ASSETSSYSTEM_JSON
using System;

using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class JsonDataCreator : IAssetDataCreator
    {
#region Private Fields
        private NewtonsoftJsonSerializer _serializer;
#endregion

#region Constructors
        public JsonDataCreator()
        {
            _serializer = new NewtonsoftJsonSerializer();
        }
#endregion

#region Public Members
        public bool Supports( Type requestedAssetType )
        {
            return requestedAssetType == typeof( IJsonAsset );
        }

        public TAssetType Create< TAssetType >( byte[ ] data ) where TAssetType : class
        {
            return _serializer.Deserialize< TAssetType >( data );
        }

        public object Create( byte[ ] data, Type type )
        {
            return _serializer.Deserialize( data, type );
        }
#endregion
    }
}
#endif