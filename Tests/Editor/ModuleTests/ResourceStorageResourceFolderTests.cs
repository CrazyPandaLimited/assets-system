using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using System.Threading;
using CrazyPanda.UnityCore.CoroutineSystem;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class ResourceStorageResourceFolderTests : BaseProcessorModuleWithOneOutTest< ResorcesFolderLoadProcessor,UrlLoadingRequest, AssetLoadingRequest< UnityEngine.Object > >
    {
        private ITimeProvider _timeProvider;
        private UrlLoadingRequest _body;
        private string url = "Cube";
        #region Public Members
        
        protected override void InternalSetup()
        {
            _timeProvider = ResourceSystemTestTimeProvider.TestTimeProvider();
            _workProcessor = new ResorcesFolderLoadProcessor();
            _body = new UrlLoadingRequest( url, typeof( GameObject ), new ProgressTracker< float >() );
        }

        [ Test ]
        public void LoadFromResourceFolderRootResourceSync()
        {
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var status = _workProcessor.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), _body );

            Assert.AreEqual( FlowMessageStatus.Accepted, status );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }

        [ UnityTest ]
        public IEnumerator LoadFromResourceFolderRootResourceAsync()
        {
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };
            
            var status = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _body );

            Assert.AreEqual( FlowMessageStatus.Accepted, status );
            
            var timeoutTime = DateTime.Now.AddSeconds(RemoteLoadingTimeoutSec);
            while( sendedBody == null && DateTime.Now < timeoutTime)
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }
            
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }
        #endregion
    }
}