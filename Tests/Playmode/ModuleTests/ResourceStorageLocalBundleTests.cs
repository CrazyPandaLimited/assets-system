using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
    public class ResourceStorageLocalBundleTests : BaseProcessorModuleWithOneOutTest<BundlesFromLocalFolderLoadProcessor,UrlLoadingRequest, AssetLoadingRequest< AssetBundle >>
    {   
        private AssetBundleManifest _manifest;
        
        string bundleName = "experiment_aliceslots.bundle";
        
        protected override void InternalSetup()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            
            _manifest = new AssetBundleManifest();
            _manifest.BundleInfos.Add(bundleName,
                new BundleInfo(new GameAssetType("Image"), bundleName)
                {
                    AssetInfos = new List<string> {"assets/assetbundlesexperiments/stuff/aliceslots/images/gem_alice.png"}
                });
            
            _workProcessor = new BundlesFromLocalFolderLoadProcessor( $"{Application.dataPath}/UnityCoreSystems/Systems/Tests/ResourcesSystem/Bundle",_manifest );
        }
        
        [UnityTest]
        public IEnumerator SuccessLoadBundleAsync()
        {   
            var requestBody = new UrlLoadingRequest( bundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< AssetBundle > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );
            
            yield return base.WaitForTimeOut(sendedBody);

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }
        
        [Test]
        public void SuccessLoadBundleSync()
        {   
            var requestBody = new UrlLoadingRequest( bundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< AssetBundle > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
            };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(MetaDataReservedKeys.SYNC_REQUEST_FLAG), CancellationToken.None ), requestBody );

            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }
        
        [UnityTest]
        public IEnumerator SuccessCancel()
        {
            
            var cancelTocken = new CancellationTokenSource();
            
            var requestBody = new UrlLoadingRequest( bundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            bool outCalled = false;
            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) => { outCalled = true; };

            var processResult = _workProcessor.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            
            cancelTocken.Cancel();

            yield return base.WaitForTimeOut();
            Assert.AreEqual( FlowMessageStatus.Accepted, processResult );
            Assert.Null( _workProcessor.Exception );
            Assert.IsFalse( outCalled );
        }
    }
}