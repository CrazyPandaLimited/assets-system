using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsUncombinerProcessor< T > : AbstractRequestInputOutputProcessor< AssetLoadingRequest< T >, AssetLoadingRequest< T > >
    {
        private Dictionary< string, CombinedRequest > _combinedRequests;
        private List< string > _keysToDelete = new List< string >();

        public RequestsUncombinerProcessor( Dictionary< string, CombinedRequest > combinedRequests )
        {
            _combinedRequests = combinedRequests;
        }

        protected override void InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( !header.MetaData.HasFlag( body.Url ) )
            {
                SendOutput( header, body );
                return;
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

                SendOutput( request.Key, new AssetLoadingRequest< T >( request.Value, body.Asset ) );
            }
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
    }

    public class RequestsUncombinerProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        private Dictionary< string, CombinedRequest > _combinedRequests;
        private List< string > _keysToDelete = new List< string >();

        public RequestsUncombinerProcessor( Dictionary< string, CombinedRequest > combinedRequests )
        {
            _combinedRequests = combinedRequests;
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( !header.MetaData.HasFlag( body.Url ) )
            {
                SendOutput( header, body );
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

                SendOutput( request.Key, new UrlLoadingRequest( body ) );
            }
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
    }
}
