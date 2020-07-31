using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class GetAssetFromCacheProcessor< T > : AbstractRequestInputOutputProcessor< UrlLoadingRequest, AssetLoadingRequest< T > >
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
        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }
            SendOutput( header, new AssetLoadingRequest< T >( body, ( T ) _cache.Get( assetName ) ) );
        }
        #endregion
    }
}
