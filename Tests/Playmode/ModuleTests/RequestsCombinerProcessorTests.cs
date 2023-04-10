using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class RequestsCombinerProcessorTests
    {
        private RequestsCombinerProcessor _combinerProcessor;
        private RequestsUncombinerProcessor< object > _uncombinerProcessor;

        private Dictionary< string, CombinedRequest > _combinedRequests;

        private UrlLoadingRequest _requestFirstFile1;
        private UrlLoadingRequest _requestFirstFile2;

        private UrlLoadingRequest _requestSecondFile1;
        private UrlLoadingRequest _requestSecondFile2;

        [ SetUp ]
        public void Setup()
        {
            _combinedRequests = new Dictionary< string, CombinedRequest >();
            _combinerProcessor = new RequestsCombinerProcessor( _combinedRequests );
            _uncombinerProcessor = new RequestsUncombinerProcessor< object >( _combinedRequests );

            _requestFirstFile1 = new UrlLoadingRequest( "1", typeof( object ), null );
            _requestFirstFile2 = new UrlLoadingRequest( "1", typeof( object ), null );

            _requestSecondFile1 = new UrlLoadingRequest( "2", typeof( object ), null );
            _requestSecondFile2 = new UrlLoadingRequest( "2", typeof( object ), null );


            var outProcessor1 = Substitute.For< IInputNode< UrlLoadingRequest > >();
            _combinerProcessor.DefaultOutput.LinkTo( outProcessor1 );

            var outProcessor2 = Substitute.For< IInputNode< AssetLoadingRequest< object > > >();
            _uncombinerProcessor.DefaultOutput.LinkTo( outProcessor2 );
        }

        [ Test ]
        public void NotCombineSyncRequestTest()
        {
            MessageHeader sendedHeader = null;
            UrlLoadingRequest sendedBody = null;

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( UrlLoadingRequest ) args.Body;
            };

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), _requestFirstFile1 );

            Assert.AreSame( _requestFirstFile1, sendedBody );
        }

        [ Test ]
        public void CombineFirstAsyncRequestTest()
        {
            MetaData sourceMeta = new MetaData();
            MessageHeader sendedHeader = null;
            UrlLoadingRequest sendedBody = null;

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( UrlLoadingRequest ) args.Body;
            };

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( sourceMeta, CancellationToken.None ), _requestFirstFile1 );
            
            Assert.AreNotSame( _requestFirstFile1, sendedBody );
            Assert.IsTrue( sendedHeader.MetaData.IsMetaExist( RequestsCombinerProcessor.COMBINE_BASE_URL ) );
            Assert.IsFalse( sourceMeta.IsMetaExist( RequestsCombinerProcessor.COMBINE_BASE_URL ) );
        }

        [ Test ]
        public void SuccessCombineTwoRequestsTest()
        {
            List< MessageHeader > sendedHeader = new List< MessageHeader >();
            List< UrlLoadingRequest > sendedBody = new List< UrlLoadingRequest >();

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader.Add( args.Header );
                sendedBody.Add( ( UrlLoadingRequest ) args.Body );
            };

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _requestFirstFile1 );

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _requestFirstFile2 );


            Assert.AreEqual( 1, sendedBody.Count );
            Assert.AreEqual( 1, _combinedRequests.Count );
            Assert.AreNotSame( _requestFirstFile1, sendedBody[ 0 ] );
            Assert.AreNotSame( _requestFirstFile2, sendedBody[ 0 ] );
        }

        [ Test ]
        public void SuccessNotCombineTwoDifferentRequestsTest()
        {
            List< MessageHeader > sendedHeader = new List< MessageHeader >();
            List< UrlLoadingRequest > sendedBody = new List< UrlLoadingRequest >();

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader.Add( args.Header );
                sendedBody.Add( ( UrlLoadingRequest ) args.Body );
            };

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _requestFirstFile1 );

            _combinerProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _requestSecondFile1 );

            Assert.AreEqual( 2, sendedBody.Count );
            Assert.AreEqual( 2, _combinedRequests.Count );
            Assert.AreNotSame( _requestFirstFile1, sendedBody[ 0 ] );
            Assert.AreNotSame( _requestSecondFile1, sendedBody[ 0 ] );
            Assert.AreNotSame( _requestFirstFile1, sendedBody[ 1 ] );
            Assert.AreNotSame( _requestSecondFile1, sendedBody[ 1 ] );
        }



        [Test]
        public void SuccessCancelOneCombinedRequestTest()
        {            
            //ILoadingRequest outRequest = _combinerOutProcessor.ReceivedCalls().First().GetArguments()[0] as MessageHeader;

            List<MessageHeader> sendedHeader = new List<MessageHeader>();
            List<UrlLoadingRequest> sendedBody = new List<UrlLoadingRequest>();

            _combinerProcessor.DefaultOutput.MessageSent += (sender, args) =>
            {
                sendedHeader.Add(args.Header);
                sendedBody.Add((UrlLoadingRequest)args.Body);
            };

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var header = new MessageHeader(new MetaData(), tokenSource.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header, _requestFirstFile1);

            //_requestFirstFile1.RequestCancel();
            tokenSource.Cancel();

            Assert.True(sendedHeader[0] != header);//TODO: how to check???
            Assert.True(header.CancellationToken.IsCancellationRequested);

            Assert.AreEqual(1, sendedHeader.Count);

            Assert.AreEqual(1, _combinedRequests.Count);

            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestSecondFile1);

            Assert.AreEqual(1, _combinedRequests.Count);
        }

        [Test]
        public void SuccessCancelOneOfTwoCombinedRequestTest()
        {
            List<MessageHeader> sendedHeader = new List<MessageHeader>();
            List<UrlLoadingRequest> sendedBody = new List<UrlLoadingRequest>();

            _combinerProcessor.DefaultOutput.MessageSent += (sender, args) =>
            {
                sendedHeader.Add(args.Header);
                sendedBody.Add((UrlLoadingRequest)args.Body);
            };

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            var header = new MessageHeader(new MetaData(), tokenSource.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header, _requestFirstFile1);
            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestFirstFile2);            

            Assert.AreEqual(1, sendedHeader.Count);

            Assert.AreEqual(1, _combinedRequests.Count);
            Assert.AreEqual(2, _combinedRequests.Values.First().SourceRequests.Count);

            tokenSource.Cancel();

            Assert.True(header.CancellationToken.IsCancellationRequested);

            Assert.False(sendedHeader[0].CancellationToken.IsCancellationRequested);

            Assert.AreEqual(1, _combinedRequests.Count);
            Assert.AreEqual(1, _combinedRequests.Values.First().SourceRequests.Count);
        }

        [Test]
        public void SuccessCancelTwoCombinedRequestTest()
        {
            List<MessageHeader> sendedHeader = new List<MessageHeader>();
            List<UrlLoadingRequest> sendedBody = new List<UrlLoadingRequest>();

            _combinerProcessor.DefaultOutput.MessageSent += (sender, args) =>
            {
                sendedHeader.Add(args.Header);
                sendedBody.Add((UrlLoadingRequest)args.Body);
            };

            CancellationTokenSource tokenSource1 = new CancellationTokenSource();
            var header1 = new MessageHeader(new MetaData(), tokenSource1.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header1, _requestFirstFile1);

            CancellationTokenSource tokenSource2 = new CancellationTokenSource();
            var header2 = new MessageHeader(new MetaData(), tokenSource2.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header2, _requestFirstFile2);

            Assert.AreEqual(1, sendedHeader.Count);

            Assert.AreEqual(1, _combinedRequests.Count);
            Assert.AreEqual(2, _combinedRequests.Values.First().SourceRequests.Count);

            tokenSource1.Cancel();
            tokenSource2.Cancel();

            Assert.True(header1.CancellationToken.IsCancellationRequested);
            Assert.True(header2.CancellationToken.IsCancellationRequested);
            

            Assert.True(sendedHeader[0].CancellationToken.IsCancellationRequested);

            Assert.AreEqual(1, _combinedRequests.Count);
            Assert.AreEqual(0, _combinedRequests.Values.First().SourceRequests.Count);

            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestSecondFile1);
            Assert.AreEqual(1, _combinedRequests.Count);
        }


        [Test]
        public void SuccessCancelOnePairOfTwoPairedCombinedRequestsTest()
        {
            List<MessageHeader> sendedHeader = new List<MessageHeader>();
            List<UrlLoadingRequest> sendedBody = new List<UrlLoadingRequest>();

            _combinerProcessor.DefaultOutput.MessageSent += (sender, args) =>
            {
                sendedHeader.Add(args.Header);
                sendedBody.Add((UrlLoadingRequest)args.Body);
            };

            CancellationTokenSource tokenSource1 = new CancellationTokenSource();
            var header1 = new MessageHeader(new MetaData(), tokenSource1.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header1, _requestFirstFile1);

            CancellationTokenSource tokenSource2 = new CancellationTokenSource();
            var header2 = new MessageHeader(new MetaData(), tokenSource2.Token);
            _combinerProcessor.DefaultInput.ProcessMessage(header2, _requestFirstFile2);

            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestSecondFile1);
            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestSecondFile2);

            Assert.AreEqual(2, sendedHeader.Count);

            Assert.AreEqual(2, _combinedRequests.Count);
            Assert.AreEqual(2, _combinedRequests.Values.First().SourceRequests.Count);
            Assert.AreEqual(2, _combinedRequests.Values.ElementAt(1).SourceRequests.Count);

            tokenSource1.Cancel();
            tokenSource2.Cancel();

            Assert.True(header1.CancellationToken.IsCancellationRequested);
            Assert.True(header2.CancellationToken.IsCancellationRequested);


            Assert.True(sendedHeader[0].CancellationToken.IsCancellationRequested);
            Assert.False(sendedHeader[1].CancellationToken.IsCancellationRequested);

            Assert.AreEqual(2, _combinedRequests.Count);
            Assert.AreEqual(0, _combinedRequests.Values.First().SourceRequests.Count);
            Assert.AreEqual(2, _combinedRequests.Values.ElementAt(1).SourceRequests.Count);

            _combinerProcessor.DefaultInput.ProcessMessage(new MessageHeader(new MetaData(), CancellationToken.None), _requestSecondFile1);
            Assert.AreEqual(1, _combinedRequests.Count);

            //     _combinerProcessor.ProcessRequest< Object >( _requestFirstFile1 );
            //     _combinerProcessor.ProcessRequest< Object >( _requestFirstFile2 );
            //
            //     _combinerProcessor.ProcessRequest< Object >( _requestSecondFile1 );
            //     _combinerProcessor.ProcessRequest< Object >( _requestSecondFile2 );
            //
            //     var calls = _combinerOutProcessor.ReceivedCalls().ToList();
            //
            //     Assert.AreEqual( 2, calls.Count );
            //
            //     ILoadingRequest outRequest1 = calls[ 0 ].GetArguments()[ 0 ] as ILoadingRequest;
            //     ILoadingRequest outRequest2 = calls[ 1 ].GetArguments()[ 0 ] as ILoadingRequest;
            //
            //     _requestFirstFile1.RequestCancel();
            //     _requestFirstFile2.RequestCancel();
            //
            //     Assert.AreEqual( PandaTaskStatus.Pending, outRequest1.Status );
            //     Assert.AreEqual( PandaTaskStatus.Pending, outRequest2.Status );
            //     Assert.AreEqual( PandaTaskStatus.Rejected, _requestFirstFile1.Status );
            //     Assert.AreEqual( PandaTaskStatus.Rejected, _requestFirstFile2.Status );
            //     Assert.AreEqual( PandaTaskStatus.Pending, _requestSecondFile1.Status );
            //     Assert.AreEqual( PandaTaskStatus.Pending, _requestSecondFile2.Status );
            //
            //     Assert.IsTrue( outRequest1.IsCancelRequested );
            //     Assert.IsFalse( outRequest2.IsCancelRequested );
            //
            //     Assert.NotNull( _taskSourceFirstFileAsync1.ResultTask.Error );
            //     Assert.NotNull( _taskSourceFirstFileAsync2.ResultTask.Error );
            //
            //     Assert.Null( _taskSourceSecondFileAsync1.ResultTask.Error );
            //     Assert.Null( _taskSourceSecondFileAsync2.ResultTask.Error );
            //
            //     try
            //     {
            //         var a = _taskSourceFirstFileAsync1.ResultTask.Result;
            //     }
            //     catch( Exception e )
            //     {
            //         Assert.AreEqual( typeof( OperationCanceledException ), ( ( AggregateException ) e ).GetBaseException().GetType() );
            //     }
            //
            //     try
            //     {
            //         var a = _taskSourceFirstFileAsync2.ResultTask.Result;
            //     }
            //     catch( Exception e )
            //     {
            //         Assert.AreEqual( typeof( OperationCanceledException ), ( ( AggregateException ) e ).GetBaseException().GetType() );
            //     }
        }

        [Test ]
        public void SuccessUncombineRequest()
        {
            object sourceAssset = new object();

            var sourceHeader1 = new MessageHeader( new MetaData(), CancellationToken.None );
            var sourceHeader2 = new MessageHeader( new MetaData(), CancellationToken.None );
            
            List< MessageHeader > sendedHeader = new List< MessageHeader >();
            List< UrlLoadingRequest > sendedBody = new List< UrlLoadingRequest >();

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader.Add( args.Header );
                sendedBody.Add( ( UrlLoadingRequest ) args.Body );
            };

            _combinerProcessor.DefaultInput.ProcessMessage( sourceHeader1, _requestFirstFile1 );

            _combinerProcessor.DefaultInput.ProcessMessage( sourceHeader2, _requestFirstFile2 );

            Assert.AreEqual( 1, sendedHeader.Count );
            Assert.AreEqual( 1, sendedBody.Count );


            List< MessageHeader > uncombinedHeader = new List< MessageHeader >();
            List< AssetLoadingRequest< object > > uncombinedBody = new List< AssetLoadingRequest< object > >();

            _uncombinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                uncombinedHeader.Add( args.Header );
                uncombinedBody.Add( ( AssetLoadingRequest< object > ) args.Body );
            };

            _uncombinerProcessor.DefaultInput.ProcessMessage( sendedHeader[ 0 ], new AssetLoadingRequest< object >( sendedBody[ 0 ], sourceAssset ) );

            Assert.AreEqual( 2, uncombinedHeader.Count );
            Assert.AreEqual( 2, uncombinedBody.Count );
            Assert.AreSame( sourceAssset, uncombinedBody[ 0 ].Asset );
            Assert.AreSame( sourceAssset, uncombinedBody[ 1 ].Asset );
            
            Assert.AreSame( sourceHeader1, uncombinedHeader[ 0 ] );
            Assert.AreSame( sourceHeader2, uncombinedHeader[ 1 ] );

            Assert.AreEqual( _requestFirstFile1.Url, uncombinedBody[ 0 ].Url );
            Assert.AreEqual( _requestFirstFile2.Url, uncombinedBody[ 1 ].Url );
        }
        
        [Test ]
        public void SuccessUncombineRequestWithExceptionsInHeader()
        {
            object sourceAssset = new object();
            string exceptionMessage = "CombinedMessage processing exception";

            var sourceHeader1 = new MessageHeader( new MetaData(), CancellationToken.None );
            var sourceHeader2 = new MessageHeader( new MetaData(), CancellationToken.None );
            
            List< MessageHeader > sendedHeader = new List< MessageHeader >();
            List< UrlLoadingRequest > sendedBody = new List< UrlLoadingRequest >();

            _combinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader.Add( args.Header );
                sendedBody.Add( ( UrlLoadingRequest ) args.Body );
            };

            _combinerProcessor.DefaultInput.ProcessMessage( sourceHeader1, _requestFirstFile1 );

            _combinerProcessor.DefaultInput.ProcessMessage( sourceHeader2, _requestFirstFile2 );

            Assert.AreEqual( 1, sendedHeader.Count );
            Assert.AreEqual( 1, sendedBody.Count );


            List< MessageHeader > uncombinedHeader = new List< MessageHeader >();
            List< AssetLoadingRequest< object > > uncombinedBody = new List< AssetLoadingRequest< object > >();

            _uncombinerProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                uncombinedHeader.Add( args.Header );
                uncombinedBody.Add( ( AssetLoadingRequest< object > ) args.Body );
            };

            sendedHeader[ 0 ].AddException( new Exception(exceptionMessage) );
            
            _uncombinerProcessor.DefaultInput.ProcessMessage( sendedHeader[ 0 ], new AssetLoadingRequest< object >( sendedBody[ 0 ], sourceAssset ) );

            Assert.AreEqual( 2, uncombinedHeader.Count );
            Assert.AreEqual( 2, uncombinedBody.Count );
            Assert.AreSame( sourceAssset, uncombinedBody[ 0 ].Asset );
            Assert.AreSame( sourceAssset, uncombinedBody[ 1 ].Asset );
            
            Assert.AreSame( sourceHeader1, uncombinedHeader[ 0 ] );
            Assert.AreSame( sourceHeader2, uncombinedHeader[ 1 ] );

            Assert.AreEqual( _requestFirstFile1.Url, uncombinedBody[ 0 ].Url );
            Assert.AreEqual( _requestFirstFile2.Url, uncombinedBody[ 1 ].Url );
            
            
            Assert.NotNull( uncombinedHeader[ 0 ].Exceptions );
            Assert.NotNull( uncombinedHeader[ 1 ].Exceptions );
            
            Assert.AreEqual( exceptionMessage, uncombinedHeader[ 0 ].Exceptions.InnerException.InnerException.Message );
            Assert.AreEqual( exceptionMessage, uncombinedHeader[ 1 ].Exceptions.InnerException.InnerException.Message );
            
        }
    }
}