using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class CheckAssetInCacheProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        #region Protected Fields
        protected ICache _cache;

        protected Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > > _cacheCheckResultOutConnections;
        #endregion

        #region Constructors
        public CheckAssetInCacheProcessor( ICache cache )
        {
            _cache = cache ?? throw new ArgumentNullException( $"{nameof(cache)} == null" );
            _cacheCheckResultOutConnections = new Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > >( 2 );
        }
        #endregion

        #region Public Members
        public void RegisterExistInCacheOutConnection( NodeOutputConnection< UrlLoadingRequest > connection )
        {
            RegisterConnection( connection );
            _cacheCheckResultOutConnections.Add( true, connection );
        }

        public void RegisterNotExistInCacheOutConnection( NodeOutputConnection< UrlLoadingRequest > connection )
        {
            RegisterConnection( connection );
            _cacheCheckResultOutConnections.Add( false, connection );
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
            _cacheCheckResultOutConnections[ _cache.Contains(assetName ) ].ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
