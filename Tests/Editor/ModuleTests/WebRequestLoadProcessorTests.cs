using System.Linq;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks.Progress;
#if CRAZYPANDA_UNITYCORE_TESTS && CRAZYPANDA_UNITYCORE_ASSETSSYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Tests;
using CrazyPanda.UnityCore.CoroutineSystem;
using CrazyPanda.UnityCore.PandaTasks;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class WebRequestLoadProcessorTests:BaseProcessorModuleWithOneOutTest<WebRequestLoadProcessor<UnityEngine.Object>, UrlLoadingRequest, AssetLoadingRequest<UnityEngine.Object> >
    {
        private ITimeProvider _timeProvider;
        
        protected override void InternalSetup()
        {
            _timeProvider = ResourceSystemTestTimeProvider.TestTimeProvider();
            var corman = new CoroutineManager();
            corman.TimeProvider = _timeProvider;
            _workProcessor = new WebRequestLoadProcessor<UnityEngine.Object>(corman, new List<IAssetDataCreator> {new TextureDataCreator(), new StringDataCreator()});
        }


        [UnityTest]
        public IEnumerator SuccessLoadFile()
        {
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl("logo_test.jpg"), typeof( Texture ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), sendedBody.Asset.GetType() );
        }

        [UnityTest]
        public IEnumerator FailIfWrongURL()
        {
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl("notExistFile.jpg"), typeof( Texture ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.Null( sendedBody.Asset );
            Assert.NotNull( sendedHeader.Exceptions );
        }
        
        [UnityTest]
        public IEnumerator SuccessCancel()
        {
            var cancelTocken = new CancellationTokenSource();
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl("logo_test.jpg"), typeof( Texture ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            
            cancelTocken.Cancel();

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );
            Assert.Null( sendedBody );
        }
        
        [Test]
        public void SyncLoadFail()
        {
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl("notExistFile.jpg"), typeof( Texture ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(MetaDataReservedKeys.SYNC_REQUEST_FLAG), CancellationToken.None ), requestBody );
            
            Assert.AreEqual( FlowMessageStatus.Rejected, processResult );
            Assert.NotNull( _workProcessor.Exception );
            Assert.Null( sendedBody );
            Assert.Null( sendedHeader );
        }
        
        [UnityTest]
        public IEnumerator FailWithAssetCreatorNotFound()
        {   
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl("logo_test.jpg"), typeof( GameObject ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( sendedBody == null && DateTime.Now < timeoutTime )
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.AreEqual( FlowNodeStatus.Failed, _workProcessor.Status );
            Assert.NotNull( _workProcessor.Exception );
            Assert.AreEqual("Asset creator not found", _workProcessor.Exception.GetBaseException().Message);
            
            Assert.Null( sendedHeader );
            Assert.Null( sendedBody );
        }
    }
}
#endif