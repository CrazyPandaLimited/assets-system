using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class CheckAssetInCacheWithRefcountProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        #region Protected Fields
        protected ICacheControllerWithAssetReferences _cache;
        protected NodeOutputConnection< UrlLoadingRequest > _existInCacheOutConnection;
        protected NodeOutputConnection< UrlLoadingRequest > _notExistInCacheOutConnection;
        #endregion

        #region Constructors
        public CheckAssetInCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cache )
        {
            _cache = cache ?? throw new ArgumentNullException( $"{nameof(cache)} == null" );
        }
        #endregion

        #region Public Members
        public void RegisterExistCacheOutConnection( IInputNode< UrlLoadingRequest > node )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( node );
            RegisterConnection( connection );
            _existInCacheOutConnection = connection;
        }

        public void RegisterNotExistCacheOutConnection( IInputNode< UrlLoadingRequest > node )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( node );
            RegisterConnection( connection );
            _notExistInCacheOutConnection = connection;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                var subAssetName = header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );
                assetName = $"{body.Url}_SubAsset:{subAssetName}";
            }
            
            if( _cache.Contains( assetName ) )
            {
                _existInCacheOutConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            _notExistInCacheOutConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
