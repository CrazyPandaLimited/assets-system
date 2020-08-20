using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetFilesManifest : IManifest
    {
        #region Public Fields
        public Dictionary< string, AssetFileInfo > _assetsInfos = new Dictionary< string, AssetFileInfo >();
        #endregion

        #region Public Members
        public void AddManifestPart( AssetFilesManifest manifestPart )
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

        public AssetFileInfo GetAssetByName( string name )
        {
            if( !_assetsInfos.ContainsKey( name ) )
            {
                throw new AssetInFileManifestMissingException( name );
            }

            return _assetsInfos[ name ];
        }
        #endregion
    }
}
