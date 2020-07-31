using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class CheckAssetInCacheWithRefcountProcessor : AbstractRequestInputProcessor< UrlLoadingRequest >
    {
        private BaseOutput< UrlLoadingRequest > _existInCacheOutput = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );
        private BaseOutput< UrlLoadingRequest > _notExistInCacheOutput = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );

        protected ICacheControllerWithAssetReferences _cache;

        public IOutputNode< UrlLoadingRequest > ExistInCacheOutput => _existInCacheOutput;
        public IOutputNode< UrlLoadingRequest > NotExistInCacheOutput => _notExistInCacheOutput;

        public CheckAssetInCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cache )
        {
            _cache = cache ?? throw new ArgumentNullException( nameof( cache ) );
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }

            if( _cache.Contains( assetName ) )
                _existInCacheOutput.ProcessMessage( header, body );
            else
                _notExistInCacheOutput.ProcessMessage( header, body );
        }
    }
}
