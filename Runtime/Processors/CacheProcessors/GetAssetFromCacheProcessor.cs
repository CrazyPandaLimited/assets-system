using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class GetAssetFromCacheProcessor< T > : AbstractRequestInputOutputProcessorWithDefaultOutput< UrlLoadingRequest, AssetLoadingRequest< T > >
    {
        #region Protected Fields
        protected ICache _cache;
        #endregion

        #region Constructors
        public GetAssetFromCacheProcessor( ICache cache )
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
            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< T >( body, ( T ) _cache.Get( assetName ) ) );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
