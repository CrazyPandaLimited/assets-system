using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;
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
#if !UNITY_EDITOR
    [Ignore("")]
#endif
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
            
            _workProcessor = new BundlesFromLocalFolderLoadProcessor( $"{Application.dataPath}/UnityCoreSystems/ResourcesSystem/Tests/Bundle",_manifest );
        }
        
        [UnityTest]
        public IEnumerator SuccessLoadBundleAsync()
        {   
            var requestBody = new UrlLoadingRequest( bundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            MessageHeader sendedHeader = null;
            AssetLoadingRequest< AssetBundle > sendedBody = null;

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
            };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );
            
            yield return base.WaitForTimeOut(sendedBody);

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

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< AssetBundle > ) args.Body;
            };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(MetaDataReservedKeys.SYNC_REQUEST_FLAG), CancellationToken.None ), requestBody );

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
            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) => { outCalled = true; };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            
            cancelTocken.Cancel();

            yield return base.WaitForTimeOut();
            Assert.Null( _workProcessor.Exception );
            Assert.IsFalse( outCalled );
        }
    }
}