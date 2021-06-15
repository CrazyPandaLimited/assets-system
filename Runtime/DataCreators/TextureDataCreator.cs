using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class TextureDataCreator : IAssetDataCreator
    {
        public bool Supports( Type requestedAssetType )
        {
            return requestedAssetType == typeof( Texture ) || requestedAssetType == typeof( Texture2D );
        }

        public TResourceType Create< TResourceType >( byte[ ] data ) where TResourceType : class
        {
            var result = new Texture2D( 2, 2 );
            result.LoadImage( data );
            return result as TResourceType;
        }

        public object Create( byte[ ] data, Type type )
        {
            return Create< Texture2D >( data );
        }
    }
}
