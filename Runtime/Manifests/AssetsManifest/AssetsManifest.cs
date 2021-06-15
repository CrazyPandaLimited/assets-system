using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetsManifest< AssetInfoType > : IManifest where AssetInfoType : class
    {
        public Dictionary< string, AssetInfoType > _assetsInfos = new Dictionary< string, AssetInfoType >();

        public void AddManifestPart( AssetsManifest< AssetInfoType > manifestPart )
        {
            foreach( var part in manifestPart._assetsInfos )
            {
                _assetsInfos.Add( part.Key, part.Value );
            }
        }

        public bool ContainsAsset( string name )
        {
            return _assetsInfos.ContainsKey( name );
        }

        public AssetInfoType GetAssetByName( string name )
        {
            if( !_assetsInfos.ContainsKey( name ) )
            {
                throw new AssetInFileManifestMissingException( name );
            }

            return _assetsInfos[ name ];
        }
    }
}
