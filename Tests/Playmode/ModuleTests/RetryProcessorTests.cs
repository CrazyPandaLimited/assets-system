using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using NSubstitute;
using NSubstitute.Core;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class RetryProcessorTests
    {
        private RequestRetryProcessor<Object> _processor;
        PandaTaskCompletionSource<Object> _taskSource;

        private AssetLoadingRequest<Object> _messageBodyFirstFile1;        

        private MessageHeader _messageHeaderFirstFile1;

        [SetUp]
        public void Setup()
        {   
            _taskSource = new PandaTaskCompletionSource<Object>();

            _messageBodyFirstFile1 = new AssetLoadingRequest<Object>("1", typeof(UnityEngine.Object), new ProgressTracker<float>(), null);
            _messageHeaderFirstFile1 = new MessageHeader(new MetaData(), CancellationToken.None);
        }


        [UnityTest]
        public IEnumerator SuccessFirstRetryTest()
        {
            _processor = new RequestRetryProcessor<Object>(new Dictionary<int, float>()
            {
                {1, 0.5f}
            },
            (header, request) => {
                return true;
            });

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RegisterRetryConnection(inputOutNode);
            
            var msgStatus = _processor.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            Assert.True(msgStatus == FlowMessageStatus.Accepted);
            
            yield return WaitResults();
            
            var rCalls = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            

            Assert.True(rCalls.Count>0);            
            
            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
            Assert.AreEqual(1, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
        }

        [UnityTest]
        public IEnumerator SuccessSecondRetryTest()
        {
            _processor = new RequestRetryProcessor<Object>(new Dictionary<int, float>()
            {
                {2, 0.5f}
            }, 
            (header, request) => {
                return true;
            });

            var inputOutNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RegisterRetryConnection(inputOutNode);

            var metaData = new MetaData();
            metaData.SetMeta(RequestRetryProcessor<Object>.RETRY_METADATA_KEY, 1);

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);
            _messageHeaderFirstFile1 = new MessageHeader(metaData, CancellationToken.None);

            var msgStatus = _processor.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            Assert.True(msgStatus == FlowMessageStatus.Accepted);

            yield return WaitResults();
            
            var rCalls = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            

            Assert.True(rCalls.Count > 0);

            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
            Assert.AreEqual(2, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));

        }

        [UnityTest]
        public IEnumerator GetAllRetryFallTest()
        {
            _processor = new RequestRetryProcessor<Object>(new Dictionary<int, float>()
            {
                {1, 0.5f}
            },
            (header, request) => {
                return true;
            });

            var inputRetryNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RegisterRetryConnection(inputRetryNode);

            var inputOutNode = Substitute.For<IInputNode<AssetLoadingRequest<Object>>>();
            _processor.RegisterOutputConnection(inputOutNode);
            
            var inputRetryExceptionNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RegisterAllRetrysFailedConnection(inputRetryExceptionNode);

            var metaData = new MetaData();
            metaData.SetMeta(RequestRetryProcessor<Object>.RETRY_METADATA_KEY, 1);

            _messageHeaderFirstFile1 = new MessageHeader(metaData, CancellationToken.None);

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);
            var msgStatus = _processor.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            //_processor.ProcessRequest<Object>(loadingRequest);

            Assert.True(msgStatus == FlowMessageStatus.Accepted);

            yield return WaitResults();
            
            var rCallsRetry = new List<ICall>(inputRetryNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.AreEqual(0,rCallsRetry.Count);

            var rCallsOut = new List<ICall>(inputRetryExceptionNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.AreEqual(2,rCallsOut.Count);

            Assert.True(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
            Assert.AreEqual(1, _messageHeaderFirstFile1.MetaData.GetMeta<int>(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
            Assert.True(_messageHeaderFirstFile1.Exceptions != null);
            Assert.True(_messageHeaderFirstFile1.Exceptions.InnerException is AllRequestRetrysFallException);
            
            //Assert.NotNull(_taskSource.ResultTask.Error);
            //Assert.True(loadingRequest.MetaData.IsMetaExist(RequestRetryProcessor.RETRY_METADATA_KEY));
            //Assert.AreEqual(typeof(AllRequestRetrysFallException), _taskSource.ResultTask.Error.GetBaseException().GetType());
        }

        [UnityTest]
        public IEnumerator OperationCancelTest()
        {
            _processor = new RequestRetryProcessor<Object>(new Dictionary<int, float>()
            {
                {1, 0.5f}
            },
            (header, request) => {
                return true;
            });

            var inputRetryNode = Substitute.For<IInputNode<UrlLoadingRequest>>();
            _processor.RegisterRetryConnection(inputRetryNode);

            var inputOutNode = Substitute.For<IInputNode<AssetLoadingRequest<Object>>>();
            _processor.RegisterOutputConnection(inputOutNode);
            //var metaData = new MetaData();

            //var loadingRequest = new UnityAssetLoadingRequest("1", metaData, _taskSource, CancellationToken.None, null);

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            _messageHeaderFirstFile1 = new MessageHeader(new MetaData(), tokenSource.Token);

            var msgStatus = _processor.ProcessMessage(_messageHeaderFirstFile1, _messageBodyFirstFile1);

            Assert.True(msgStatus == FlowMessageStatus.Accepted);

            tokenSource.Cancel();

            yield return WaitResults();
            
            var rCallsRetry = new List<ICall>(inputRetryNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.True(rCallsRetry.Count == 0);

            var rCallsOut = new List<ICall>(inputOutNode.ReceivedCalls());
            //Debug.Log($"Calls = {rCalls.Count}");            
            Assert.True(rCallsRetry.Count == 0);

            Assert.True(_messageHeaderFirstFile1.CancellationToken.IsCancellationRequested);
            Assert.False(_messageHeaderFirstFile1.MetaData.IsMetaExist(RequestRetryProcessor<Object>.RETRY_METADATA_KEY));
        }
        
        private IEnumerator WaitResults(float timeToWait = 0.6f)
        {
            var waitTime = DateTime.Now.AddSeconds(timeToWait);
            while (DateTime.Now < waitTime)
            {
                yield return null;
            }
        }
    }
}