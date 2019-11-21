using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsUncombinerProcessor< T > : AbstractRequestInputOutputProcessorWithDefaultOutput< AssetLoadingRequest< T >, AssetLoadingRequest< T > >
    {
        #region Private Fields
        private Dictionary< string, CombinedRequest > _combinedRequests;
        private List< string > _keysToDelete = new List< string >();
        #endregion

        #region Constructors
        public RequestsUncombinerProcessor( Dictionary< string, CombinedRequest > combinedRequests )
        {
            _combinedRequests = combinedRequests;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( !header.MetaData.HasFlag( RequestsCombinerProcessor.IS_COMBINED_REQUEST_METADATA_FLAG ) )
            {
                _defaultConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            ClearCancelledRequests();

            var combinedRequest = _combinedRequests[ body.Url ];
            _combinedRequests.Remove( body.Url );

            foreach( var request in combinedRequest.SourceRequests )
            {
                if( header.Exceptions != null )
                {
                    request.Key.AddException( header.Exceptions );
                }

                _defaultConnection.ProcessMessage( request.Key, new AssetLoadingRequest< T >( request.Value, body.Asset ) );
            }

            return FlowMessageStatus.Accepted;
        }

        private void ClearCancelledRequests()
        {
            _keysToDelete.Clear();

            foreach( var kvp in _combinedRequests )
            {
                if( kvp.Value.CombinedHeader.CancellationToken.IsCancellationRequested )
                {
                    _keysToDelete.Add( kvp.Key );
                }
            }

            foreach( var key in _keysToDelete )
            {
                _combinedRequests.Remove( key );
            }
        }
        #endregion
    }

    public class RequestsUncombinerProcessor : AbstractRequestInputOutputProcessorWithDefaultOutput< UrlLoadingRequest, UrlLoadingRequest >
    {
        #region Private Fields
        private Dictionary< string, CombinedRequest > _combinedRequests;
        private List< string > _keysToDelete = new List< string >();
        #endregion

        #region Constructors
        public RequestsUncombinerProcessor( Dictionary< string, CombinedRequest > combinedRequests )
        {
            _combinedRequests = combinedRequests;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( !header.MetaData.HasFlag( RequestsCombinerProcessor.IS_COMBINED_REQUEST_METADATA_FLAG ) )
            {
                _defaultConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            ClearCancelledRequests();

            var combinedRequest = _combinedRequests[ body.Url ];
            _combinedRequests.Remove( body.Url );

            foreach( var request in combinedRequest.SourceRequests )
            {
                if( header.Exceptions != null )
                {
                    request.Key.AddException( header.Exceptions );
                }

                _defaultConnection.ProcessMessage( request.Key, new UrlLoadingRequest( body ) );
            }

            return FlowMessageStatus.Accepted;
        }

        private void ClearCancelledRequests()
        {
            _keysToDelete.Clear();

            foreach( var kvp in _combinedRequests )
            {
                if( kvp.Value.CombinedHeader.CancellationToken.IsCancellationRequested )
                {
                    _keysToDelete.Add( kvp.Key );
                }
            }

            foreach( var key in _keysToDelete )
            {
                _combinedRequests.Remove( key );
            }
        }
        #endregion
    }
}
