using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.PandaTasks;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class CombinedRequest
    {
        public MessageHeader CombinedHeader;
        public UrlLoadingRequest CombinedBody;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        protected CancellationTokenSource _cancellationTokenSource;

        public Dictionary< MessageHeader, UrlLoadingRequest > SourceRequests { get; private set; }

        public CombinedRequest( MessageHeader combinedHeader, UrlLoadingRequest combinedBody, CancellationTokenSource cancellationTokenSource )
        {
            SourceRequests = new Dictionary< MessageHeader, UrlLoadingRequest >();
            CombinedHeader = combinedHeader;
            CombinedBody = combinedBody;
            _cancellationTokenSource = cancellationTokenSource;
            CombinedBody.ProgressTracker.OnProgressChanged += ProgressTrackerOnProgressChanged;
        }

        public void AddRequest( MessageHeader header, UrlLoadingRequest body )
        {
            SourceRequests.Add( header, body );

            if( header.CancellationToken.CanBeCanceled )
            {
                header.CancellationToken.Register( () => OnCancelRequested( header ) );
            }
        }

        protected void OnCancelRequested( MessageHeader header )
        {
            SourceRequests.Remove(header);
            if ( SourceRequests.Count == 0 )
            {
                _cancellationTokenSource.Cancel();
            }
        }

        private void ProgressTrackerOnProgressChanged( float progress )
        {
            foreach( var request in SourceRequests )
            {
                request.Value.ProgressTracker.ReportProgress( progress );
            }
        }
    }
}
