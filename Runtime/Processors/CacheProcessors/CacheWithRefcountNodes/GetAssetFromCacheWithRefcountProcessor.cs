using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class GetAssetFromCacheWithRefcountProcessor< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        protected ICacheControllerWithAssetReferences _cache;

        public GetAssetFromCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cache )
        {
            _cache = cache ?? throw new ArgumentNullException( $"{nameof(cache)} == null" );
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }
            
            if( !header.MetaData.IsMetaExist( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY ) )
            {
                header.AddException( new MetaDataNotContainsReferenceObjectForAsset( this, header, body ) );
                SendException( header,body );
                return;
            }

            var asset = _cache.Get( assetName, header.MetaData.GetOwnerReference(), body.AssetType );
            SendOutput( header, new AssetLoadingRequest< T >( body, ( T ) asset ) );
        }
    }
}
