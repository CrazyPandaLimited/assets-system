﻿using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class DefaultAntiCacheUrlResolver : IAntiCacheUrlResolver
    {
        private Dictionary< string, string > _perResourceAntiCache = new Dictionary< string, string >();
        private string _defaultAnticache;

        public void UpdateDefault( string anticache )
        {
            _defaultAnticache = anticache;
        }

        public void AddOrUpdateKey( string key, string anticache )
        {
            if( _perResourceAntiCache.ContainsKey( key ) )
            {
                _perResourceAntiCache[ key ] = anticache;
                return;
            }

            _perResourceAntiCache.Add( key, anticache );
        }

        public void AddOrUpdateKeys( Dictionary< string, string > perResourceAnticaches )
        {
            foreach( var perResourceAnticach in perResourceAnticaches )
            {
                AddOrUpdateKey( perResourceAnticach.Key, perResourceAnticach.Value );
            }
        }

        public string ResolveURL( string uri, string anticacheAssetKey = "" )
        {
            if( string.IsNullOrEmpty( anticacheAssetKey ) )
            {
                return uri + _defaultAnticache;
            }

            if( _perResourceAntiCache.ContainsKey( anticacheAssetKey ) )
            {
                return uri + _perResourceAntiCache[ anticacheAssetKey ];
            }

            throw new AnticacheNotFoundException( uri, anticacheAssetKey );
        }
    }
}
