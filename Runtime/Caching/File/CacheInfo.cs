using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    [ Serializable ]
    public class CacheInfo
    {
        #region Public Fields
        public List< CachedFileInfo > Files = new List< CachedFileInfo >();
        #endregion

        #region Public Members
        public void AddOrUpdate( string key, string md5 )
        {
            Remove( key );
            Files.Add( new CachedFileInfo( key, md5 ) );
        }

        public bool Contains( string key )
        {
            return Files.Any( f => f.Key == key );
        }

        public void Remove( string key )
        {
            Files.RemoveAll( f => f.Key == key );
        }

        public CachedFileInfo Get( string key )
        {
            return Files.FirstOrDefault( f => f.Key == key );
        }
        #endregion
    }
}
