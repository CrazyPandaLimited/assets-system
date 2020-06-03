using System;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddAssetToCacheNode< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< AssetLoadingRequest< T >, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Private Fields
        private ICache _memoryCache;
        #endregion

        #region Constructors
        public AddAssetToCacheNode( ICache memoryCache )
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException( $"{nameof(memoryCache)} == null" );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
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
                _exceptionConnection.ProcessMessage( header, new UrlLoadingRequest( body ) );
                return FlowMessageStatus.Accepted;
            }
            
            _defaultConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
