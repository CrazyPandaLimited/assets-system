using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class CombinedRequest
    {
        #region Public Fields
        public MessageHeader CombinedHeader;
        public UrlLoadingRequest CombinedBody;
        #endregion

        #region Protected Fields
        public CancellationToken CancellationToken => _cancellationTokenSource.Token;
        protected CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Properties
        public Dictionary< MessageHeader, UrlLoadingRequest > SourceRequests { get; private set; }
        #endregion

        #region Constructors
        public CombinedRequest( MessageHeader combinedHeader, UrlLoadingRequest combinedBody, CancellationTokenSource cancellationTokenSource )
        {
            SourceRequests = new Dictionary< MessageHeader, UrlLoadingRequest >();
            CombinedHeader = combinedHeader;
            CombinedBody = combinedBody;
            _cancellationTokenSource = cancellationTokenSource;
            CombinedBody.ProgressTracker.OnProgressChanged += ProgressTrackerOnProgressChanged;
        }
        #endregion

        #region Public Members
        public void AddRequest( MessageHeader header, UrlLoadingRequest body )
        {
            SourceRequests.Add( header, body );
            header.CancellationToken.Register( () => OnCancelRequested( header ) );
        }
        #endregion

        #region Protected Members
        protected void OnCancelRequested( MessageHeader header )
        {
            SourceRequests.Remove(header);
            if ( SourceRequests.Count == 0 )
            {
                _cancellationTokenSource.Cancel();
            }
        }
        #endregion

        #region Private Members
        private void ProgressTrackerOnProgressChanged( object sender, ProgressChangedEventArgs< float > e )
        {
            foreach( var request in SourceRequests )
            {
                request.Value.ProgressTracker.ReportProgress( e.progress );
            }
        }
        #endregion
    }
}
