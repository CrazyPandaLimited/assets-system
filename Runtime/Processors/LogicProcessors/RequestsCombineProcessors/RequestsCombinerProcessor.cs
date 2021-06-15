using System;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsCombinerProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        public const string IS_COMBINED_REQUEST_METADATA_FLAG = "requestCombined";

        private Dictionary< string, CombinedRequest > _combinedRequests;
        private List<string> _keysToDelete = new List<string>();

        public RequestsCombinerProcessor( Dictionary< string, CombinedRequest > combinedRequests )
        {
            _combinedRequests = combinedRequests;
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            //if sync request, don't combine it
            //if request with sub assets, don't combine it
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) ||
                header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ))
            {
                SendOutput( header, body );
                return;
            }

            ClearCancelledRequests();

            if( _combinedRequests.ContainsKey( body.Url ) )
            {
                _combinedRequests[ body.Url ].AddRequest( header, body );
                return;
            }

            var cancelTocken = new CancellationTokenSource();
            var progressTracker = new ProgressTracker< float >();
            var metaData = new MetaData( IS_COMBINED_REQUEST_METADATA_FLAG );
            metaData.SetFlag( IS_COMBINED_REQUEST_METADATA_FLAG );

            var combinedHeader = new MessageHeader( new MetaData( IS_COMBINED_REQUEST_METADATA_FLAG ), cancelTocken.Token );
            var combinedBody = new UrlLoadingRequest( body.Url, body.AssetType, progressTracker );


            var combinedRequest = new CombinedRequest( combinedHeader, combinedBody, cancelTocken );
            combinedRequest.AddRequest( header, body );
            _combinedRequests.Add( body.Url, combinedRequest );

            SendOutput( combinedHeader, combinedBody );
        }

        private void ClearCancelledRequests()
        {
            _keysToDelete.Clear();

            foreach (var kvp in _combinedRequests)
            {
                if (kvp.Value.CombinedHeader.CancellationToken.IsCancellationRequested)
                {
                    _keysToDelete.Add(kvp.Key);
                }
            }

            foreach (var key in _keysToDelete)
            {
                _combinedRequests.Remove(key);
            }
        }
    }
}
