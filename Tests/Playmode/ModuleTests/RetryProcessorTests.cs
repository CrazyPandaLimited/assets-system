using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class RetryProcessorTests
    {
        private RequestRetryProcessor _processor;
        UnsafeCompletionSource<Object> _taskSource;

        private AssetLoadingRequest<Object> _messageBodyFirstFile1;        

        private MessageHeader _messageHeaderFirstFile1;

        [SetUp]
        public void Setup()
        {   
            _taskSource = UnsafeCompletionSource<Object>.Create();

            _messageBodyFirstFile1 = new AssetLoadingRequest<Object>("1", typeof(UnityEngine.Object), new ProgressTracker<float>(), null);
            _messageHeaderFirstFile1 = new MessageHeader(new MetaData(), CancellationToken.None);
        }


        [AsyncTest]
        public async PandaTask SuccessFirstRetryTest()
        {
            _processor = new RequestRetryProcessor(new Dictionary<int, float>()
            {
                {1, 0.5f}
            },
            (header, request) => {
                return true;
            });

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            //_processor.RetryOutput.LinkTo(inputOutNode);
            _processor.RetryOutput.LinkTo(inputOutNode);
            
            _processor.DefaultInput.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            await PandaTasksUtilities.WaitWhile( () => !inputOutNode.ReceivedCalls().Any() );
            
            var rCalls = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            

            Assert.True(rCalls.Count>0);            
            
            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
            Assert.AreEqual(1, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor.RETRY_METADATA_KEY));
        }

        [AsyncTest]
        public async PandaTask SuccessSecondRetryTest()
        {
            _processor = new RequestRetryProcessor(new Dictionary<int, float>()
            {
                {2, 0.5f}
            }, 
            (header, request) => {
                return true;
            });

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RetryOutput.LinkTo(inputOutNode);

            var metaData = new MetaData();
            metaData.SetMeta(RequestRetryProcessor.RETRY_METADATA_KEY, 1);

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);
            _messageHeaderFirstFile1 = new MessageHeader(metaData, CancellationToken.None);

            _processor.DefaultInput.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            await PandaTasksUtilities.WaitWhile( () => !inputOutNode.ReceivedCalls().Any() );
            
            var rCalls = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            

            Assert.True(rCalls.Count > 0);

            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
            Assert.AreEqual(2, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor.RETRY_METADATA_KEY));

        }

        [AsyncTest]
        public async PandaTask GetAllRetryFallTest()
        {
            _processor = new RequestRetryProcessor(new Dictionary<int, float>()
            {
                {1, 0.5f}
            },
            (header, request) => {
                return true;
            });

            var inputRetryNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RetryOutput.LinkTo(inputRetryNode);

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.DefaultOutput.LinkTo(inputOutNode);
            
            var inputRetryExceptionNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.AllRetrysFailedOutput.LinkTo(inputRetryExceptionNode);

            var metaData = new MetaData();
            metaData.SetMeta(RequestRetryProcessor.RETRY_METADATA_KEY, 1);

            _messageHeaderFirstFile1 = new MessageHeader(metaData, CancellationToken.None);

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);
            _processor.DefaultInput.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            //_processor.ProcessRequest<Object>(loadingRequest);

            await PandaTasksUtilities.Delay( 1000 );
            
            var rCallsRetry = new List<ICall>(inputRetryNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.AreEqual( 0, rCallsRetry.Count );

            var rCallsOut = new List<ICall>(inputRetryExceptionNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.AreEqual( 1, rCallsOut.Count );

            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
            Assert.AreEqual(1, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor.RETRY_METADATA_KEY));
            Assert.True(_messageHeaderFirstFile1.Exceptions != null);
            Assert.True(_messageHeaderFirstFile1.Exceptions.InnerException is AllRequestRetrysFallException);
            
            //Assert.NotNull(_taskSource.ResultTask.Error);
            //Assert.True(loadingRequest.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
            //Assert.AreEqual(typeof(AllRequestRetrysFallException), _taskSource.ResultTask.Error.GetBaseException().GetType());
        }

        [AsyncTest]
        public async PandaTask OperationCancelTest()
        {
            _processor = new RequestRetryProcessor(new Dictionary<int, float>()
            {
                {1, 0.1f}
            },
            (header, request) => {
                return true;
            });

            var inputRetryNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RetryOutput.LinkTo(inputRetryNode);

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.DefaultOutput.LinkTo(inputOutNode);
            //var metaData = new MetaData();

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            _messageHeaderFirstFile1 = new MessageHeader(new MetaData(), tokenSource.Token);

            _processor.DefaultInput.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            tokenSource.Cancel();

            await PandaTasksUtilities.Delay( 1000 );
            
            var rCallsRetry = new List<ICall>(inputRetryNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.True(rCallsRetry.Count == 0);

            var rCallsOut = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.True(rCallsOut.Count == 0);

            Assert.True(_messageHeaderFirstFile1.CancellationToken.IsCancellationRequested);
            Assert.False(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
        }
    }
}