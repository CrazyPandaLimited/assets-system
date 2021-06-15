using System.Collections.Generic;
using CrazyPanda.UnityCore.Collections;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueue : IRequestsQueue
    {
        protected int _maxWorkingRequests;
        public OrderedNotifySet< RequestQueueEntry > _waitingRequests { get; private set; }
        public List< RequestQueueEntry > _workingRequests { get; private set; }

        public RequestsQueue( int maxWorkingRequests )
        {
            _waitingRequests = new OrderedNotifySet< RequestQueueEntry >();
            _workingRequests = new List< RequestQueueEntry >();
            _maxWorkingRequests = maxWorkingRequests;
        }

        public void Add( RequestQueueEntry entry )
        {
            _waitingRequests.AddLast( entry );
            entry.Header.CancellationToken.Register( () => { TryStartNextRequest(); } );

            TryStartNextRequest();
        }


        public void RequestReachedQueuedEndPoint( MessageHeader header )
        {
            _workingRequests.RemoveAt( _workingRequests.FindIndex( c => c.Header.Id == header.Id ) );
            TryStartNextRequest();
        }

        protected void TryStartNextRequest()
        {
            CheckCanceledRequests();
            if( _workingRequests.Count == _maxWorkingRequests || _waitingRequests.Count == 0 )
            {
                return;
            }

            var newRequestToProcess = _waitingRequests.First;

            foreach( var requestQueueEntry in _waitingRequests )
            {
                if( requestQueueEntry.Priority < newRequestToProcess.Priority )
                {
                    newRequestToProcess = requestQueueEntry;
                }
            }
            _waitingRequests.Remove( newRequestToProcess );
            _workingRequests.Add( newRequestToProcess );
            newRequestToProcess.ContinueProcessingHandler();
            TryStartNextRequest();
        }

        private void CheckCanceledRequests()
        {
            List< RequestQueueEntry > requestsToDel = new List< RequestQueueEntry >();
            foreach( var waitingRequest in _waitingRequests )
            {
                if( waitingRequest.Header.CancellationToken.IsCancellationRequested )
                {
                    requestsToDel.Add( waitingRequest );
                }
            }

            foreach( var request in requestsToDel )
            {
                _waitingRequests.Remove( request );
            }

            requestsToDel.Clear();

            foreach( var waitingRequest in _workingRequests )
            {
                if( waitingRequest.Header.CancellationToken.IsCancellationRequested )
                {
                    requestsToDel.Add( waitingRequest );
                }
            }

            foreach( var request in requestsToDel )
            {
                _workingRequests.Remove( request );
            }
        }
    }
}
