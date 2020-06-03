using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class GetAssetFromCacheWithRefcountProcessor< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected ICacheControllerWithAssetReferences _cache;
        #endregion

        #region Constructors
        public GetAssetFromCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cache )
        {
            _cache = cache ?? throw new ArgumentNullException( $"{nameof(cache)} == null" );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }
            
            if( !header.MetaData.IsMetaExist( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY ) )
            {
                header.AddException( new MetaDataNotContainsReferenceObjectForAsset( this, header, body ) );
                _exceptionConnection.ProcessMessage( header, body);
                return FlowMessageStatus.Accepted;
            }

            var asset = _cache.Get( assetName, header.MetaData.GetOwnerReference(), body.AssetType );
            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< T >( body, ( T ) asset ) );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
