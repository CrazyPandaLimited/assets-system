using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Tests;
using CrazyPanda.UnityCore.CoroutineSystem;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class ResourceStorageRemoteBundleTests : BaseProcessorModuleWithOneOutTest< BundlesFromWebRequestLoadProcessor, UrlLoadingRequest, AssetLoadingRequest< AssetBundle > >
    {
        private const string BaseManifestName = "custom_manifest.json";
        private const string BundleName = "blue.bundle3d";

        private AssetBundleManifest _manifest;
        private ITimeProvider _timeProvider;

        #region Public Members
        protected override void InternalSetup()
        {
            AssetBundle.UnloadAllAssetBundles( true );

            _timeProvider = ResourceSystemTestTimeProvider.TestTimeProvider();
            var corman = new CoroutineManager();
            corman.TimeProvider = _timeProvider;

            LoadManifest( ResourceStorageTestUtils.ConstructTestBundlesUrl( BaseManifestName ) );

            _workProcessor = new BundlesFromWebRequestLoadProcessor( ResourceStorageTestUtils.ConstructTestBundlesUrl(), _manifest, corman );
        }

        private void LoadManifest( string manifestUrl )
        {
            var webRequest = UnityWebRequest.Get( manifestUrl );
            var loadingAsyncOperation = webRequest.SendWebRequest();
            while( !loadingAsyncOperation.isDone )
            {
            }

            var jsonSerializer = new NewtonsoftJsonSerializer( new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8 );
            _manifest = jsonSerializer.DeserializeString< CrazyPanda.UnityCore.AssetsSystem.AssetBundleManifest >( webRequest.downloadHandler.text );
        }

        [ UnityTest ]
        public IEnumerator SuccessLoadBundle()
        {
            var requestBody = new UrlLoadingRequest( BundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< AssetBundle > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
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
        }

        [ UnityTest ]
        public IEnumerator SuccessCancel()
        {
            var cancelTocken = new CancellationTokenSource();

            var requestBody = new UrlLoadingRequest( BundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            bool outCalled = false;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) => { outCalled = true; };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            cancelTocken.Cancel();

            var timeoutTime = DateTime.Now.AddSeconds( RemoteLoadingTimeoutSec );
            while( !outCalled && DateTime.Now < timeoutTime )
            {
                _timeProvider.OnUpdate += Raise.Event< Action >();
                yield return null;
            }

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );
            Assert.IsFalse( outCalled );
        }

        [ Test ]
        public void FailSyncLoading()
        {
            var exceptionsOutProcessor = Substitute.For< IInputNode< UrlLoadingRequest > >();
            _workProcessor.RegisterExceptionConnection( exceptionsOutProcessor );
            
            var requestBody = new UrlLoadingRequest( BundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            bool outCalled = false;
            Exception headerException = null;
            _workProcessor.GetOutputs().ElementAt( 1 ).OnMessageSended += ( sender, args ) => { outCalled = true;
                headerException = args.Header.Exceptions;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );
            Assert.IsTrue( outCalled );
        }
        #endregion
    }
}