using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using System.Text;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Tests;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.TestTools;
using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("ServerTests")]
    public class ResourceStorageRemoteBundleTests : BaseProcessorModuleWithOneOutTest< BundlesFromWebRequestLoadProcessor, UrlLoadingRequest, AssetLoadingRequest< AssetBundle > >
    {
        private const string BaseManifestName = "custom_manifest.json";
        private const string BundleName = "blue.bundle3d";

        private AssetBundleManifest _manifest;

        protected override void InternalSetup()
        {
            AssetBundle.UnloadAllAssetBundles( true );

            LoadManifest( ResourceStorageTestUtils.ConstructTestBundlesUrl( BaseManifestName ) );

            _workProcessor = new BundlesFromWebRequestLoadProcessor( ResourceStorageTestUtils.ConstructTestBundlesUrl(), _manifest );
        }

        private void LoadManifest( string manifestUrl )
        {
            var webRequest = UnityWebRequest.Get( manifestUrl );
            var loadingAsyncOperation = webRequest.SendWebRequest();
            while( !loadingAsyncOperation.isDone )
            {
            }

            var jsonSerializer = new NewtonsoftJsonSerializer( new JsonSerializerSettings { Formatting = Formatting.Indented }, Encoding.UTF8 );
            _manifest = jsonSerializer.Deserialize< AssetBundleManifest >( webRequest.downloadHandler.text );
        }

        [ UnityTest ]
        public IEnumerator SuccessLoadBundle()
        {
            var requestBody = new UrlLoadingRequest( BundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< AssetBundle > sendedBody = null;

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
            };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            yield return WaitForTimeOut( sendedBody );
            
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

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) => { outCalled = true; };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            cancelTocken.Cancel();

            yield return WaitForTimeOut( () => outCalled );
            
            Assert.Null( _workProcessor.Exception );
            Assert.IsFalse( outCalled );
        }

        [ Test ]
        public void FailSyncLoading()
        {
            var exceptionsOutProcessor = Substitute.For< IInputNode< UrlLoadingRequest > >();
            _workProcessor.ExceptionOutput.LinkTo( exceptionsOutProcessor );
            
            var requestBody = new UrlLoadingRequest( BundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            bool outCalled = false;
            Exception headerException = null;
            _workProcessor.ExceptionOutput.MessageSent += ( sender, args ) => { outCalled = true;
                headerException = args.Header.Exceptions;
            };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );

            Assert.Null( _workProcessor.Exception );
            Assert.IsTrue( outCalled );
        }
    }
}