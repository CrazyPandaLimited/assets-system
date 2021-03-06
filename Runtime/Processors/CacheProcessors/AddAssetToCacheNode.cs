﻿using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddAssetToCacheNode< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< AssetLoadingRequest< T >, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        private ICache _memoryCache;

        public AddAssetToCacheNode( ICache memoryCache )
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException( $"{nameof(memoryCache)} == null" );
        }

        protected override void InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }

            if (!_memoryCache.Contains(assetName))
            {
                _memoryCache.Add(assetName, body.Asset);
            }
            else if (_memoryCache.Get(assetName) != (object)body.Asset)
            {
                header.AddException( new CachedObjectOverrideException( this, header, body ) );
                SendException( header, new UrlLoadingRequest( body ) );
                return;
            }
            
            SendOutput( header, body );
        }
    }
}
