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
            if (!_memoryCache.Contains(body.Url))
            {
                _memoryCache.Add(body.Url, body.Asset);
            }
            else if (_memoryCache.Get(body.Url) != (object)body.Asset)
            {
                header.AddException( new TryOfOverridingCachedObjectException("Can't override cached object"));
                _exceptionConnection.ProcessMessage( header, new UrlLoadingRequest( body ) );
                return FlowMessageStatus.Accepted;
            }
            
            _defaultConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
