using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class RequestsQueueProcessorTests
    {
        private RequestsQueue _requestsQueue;

        private MessageHeader _messageHeader1;
        private MessageHeader _messageHeader2;
        private MessageHeader _messageHeader3;
        private UrlLoadingRequest _messageBody;

        [SetUp]
        public void Setup()
        {
            _messageHeader1 =  new MessageHeader(new MetaData(), CancellationToken.None);
            _messageHeader2 =  new MessageHeader(new MetaData(), CancellationToken.None);
            _messageHeader3 =  new MessageHeader(new MetaData(), CancellationToken.None);
            _messageBody = new UrlLoadingRequest("1", typeof(UnityEngine.Object), new ProgressTracker<float>());
            RecreateProcessors(1);
        }

        private void RecreateProcessors(int maxWorkingRequests)
        {
            _requestsQueue = new RequestsQueue(maxWorkingRequests);
        }

        [Test]
        public void OneRequestSucessPassQueue()
        {
            bool continueCalled = false;
            var queueEntry = new RequestQueueEntry( _messageHeader1, _messageBody, 0, () => continueCalled = true );
            _requestsQueue.Add( queueEntry );
            Assert.True( continueCalled );
        }

        [Test]
        public void ThreeRequestsSucessPassOneByOneQueue()
        {
            RecreateProcessors( 1 );
            var passOrder = new List< int >();
            var queueEntry1 = new RequestQueueEntry( _messageHeader1, _messageBody, 0, () => passOrder.Add( 1 ) );
            var queueEntry2 = new RequestQueueEntry( _messageHeader2, _messageBody, 0, () => passOrder.Add( 2 ) );
            var queueEntry3 = new RequestQueueEntry( _messageHeader3, _messageBody, 0, () => passOrder.Add( 3 ) );
            _requestsQueue.Add( queueEntry1 );
            _requestsQueue.Add( queueEntry2 );
            _requestsQueue.Add( queueEntry3 );
            
            Assert.AreEqual( 1, passOrder.Count );
            Assert.AreEqual( 1, passOrder[0] );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader1 );
            
            Assert.AreEqual( 2, passOrder.Count );
            Assert.AreEqual( 2, passOrder[1] );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader2 );
            
            Assert.AreEqual( 3, passOrder.Count );
            Assert.AreEqual( 3, passOrder[2] );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader3 );
            
            Assert.AreEqual( 0, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 0, _requestsQueue._workingRequests.Count );
        }

        [Test]
        public void ThreeRequestsSucessPassTogetherQueue()
        {
            RecreateProcessors( 3 );
            var passOrder = new List< int >();
            var queueEntry1 = new RequestQueueEntry( _messageHeader1, _messageBody, 0, () => passOrder.Add( 1 ) );
            var queueEntry2 = new RequestQueueEntry( _messageHeader2, _messageBody, 0, () => passOrder.Add( 2 ) );
            var queueEntry3 = new RequestQueueEntry( _messageHeader3, _messageBody, 0, () => passOrder.Add( 3 ) );
            _requestsQueue.Add( queueEntry1 );
            _requestsQueue.Add( queueEntry2 );
            _requestsQueue.Add( queueEntry3 );
            
            Assert.AreEqual( 3, passOrder.Count );
            Assert.AreEqual( 1, passOrder[0] );
            Assert.AreEqual( 2, passOrder[1] );
            Assert.AreEqual( 3, passOrder[2] );
            Assert.AreEqual( 0, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 3, _requestsQueue._workingRequests.Count );
        }
        
        
        [Test]
        public void CancelRequestInWaitingQueue()
        {
            CancellationTokenSource cancelTocken = new CancellationTokenSource();
            _messageHeader1 = new MessageHeader(new MetaData(), cancelTocken.Token);
            RecreateProcessors(0);
            var queueEntry = new RequestQueueEntry( _messageHeader1, _messageBody, 0, () => { } );
            
            _requestsQueue.Add( queueEntry );
            
            Assert.AreEqual( 1, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 0, _requestsQueue._workingRequests.Count );
        
            cancelTocken.Cancel();
        
            Assert.AreEqual( 0, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 0, _requestsQueue._workingRequests.Count );
        }
        
        
        [Test]
        public void ThreeRequestsWithDifferentPrioritySucessPassQueue()
        {
            RecreateProcessors( 1 );
            var passOrder = new List< int >();
            var _messageHeader4 = new MessageHeader(new MetaData(), CancellationToken.None);
            
            var queueEntry1 = new RequestQueueEntry( _messageHeader1, _messageBody, 0, () => passOrder.Add( 1 ) );
            var queueEntry2 = new RequestQueueEntry( _messageHeader2, _messageBody, 1, () => passOrder.Add( 2 ) );
            var queueEntry3 = new RequestQueueEntry( _messageHeader3, _messageBody, 0, () => passOrder.Add( 3 ) );
            var queueEntry4 = new RequestQueueEntry( _messageHeader4, _messageBody, -1, () => passOrder.Add( 4 ) );
            _requestsQueue.Add( queueEntry1 );
            _requestsQueue.Add( queueEntry2 );
            _requestsQueue.Add( queueEntry3 );
            _requestsQueue.Add( queueEntry4 );
            
            Assert.AreEqual( 1, passOrder.Count );
            Assert.AreEqual( 1, passOrder[0] );
            Assert.AreEqual( 3, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 1, _requestsQueue._workingRequests.Count );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader1 );
            
            Assert.AreEqual( 2, passOrder.Count );
            Assert.AreEqual( 4, passOrder[1] );
            Assert.AreEqual( 2, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 1, _requestsQueue._workingRequests.Count );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader4 );
            
            Assert.AreEqual( 3, passOrder.Count );
            Assert.AreEqual( 3, passOrder[2] );
            Assert.AreEqual( 1, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 1, _requestsQueue._workingRequests.Count );
            
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader3 );
            
            Assert.AreEqual( 4, passOrder.Count );
            Assert.AreEqual( 2, passOrder[3] );
            Assert.AreEqual( 0, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 1, _requestsQueue._workingRequests.Count );
            
            _requestsQueue.RequestReachedQueuedEndPoint( _messageHeader2 );
            
            Assert.AreEqual( 0, _requestsQueue._waitingRequests.Count );
            Assert.AreEqual( 0, _requestsQueue._workingRequests.Count );
        }
    }
}