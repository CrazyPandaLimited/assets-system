using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class ResourceStorageLocalBundleTests
    {   
        private AssetBundleManifest _manifest;
        
        string bundleName = "experiment_aliceslots.bundle";
        
        protected IInputNode< AssetLoadingRequest< AssetBundle > > outProcessor;
        protected BundlesFromLocalFolderLoadProcessor _workProcessor;

        [ UnitySetUp ]
        public IEnumerator Setup()
        {
            AssetBundle.UnloadAllAssetBundles(true);
            
            _manifest = new AssetBundleManifest();
            _manifest.BundleInfos.Add(bundleName,
                                      new BundleInfo(new GameAssetType("Image"), bundleName)
                                      {
                                          AssetInfos = new List<string> {"assets/assetbundlesexperiments/stuff/aliceslots/images/gem_alice.png"}
                                      });

#if !UNITY_EDITOR
            _workProcessor = new BundlesFromLocalFolderLoadProcessor( Application.persistentDataPath,_manifest );
#else            
            _workProcessor = new BundlesFromLocalFolderLoadProcessor( $"{Application.dataPath}/UnityCoreSystems/ResourcesSystem/Tests/Bundle/Editor",_manifest );
#endif            
            outProcessor = Substitute.For< IInputNode< AssetLoadingRequest< AssetBundle > > >();
            _workProcessor.DefaultOutput.LinkTo( outProcessor );
            
            yield break;
        }


        [ AsyncTest ]
        public async IPandaTask SuccessLoadBundleAsync()
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

            await PandaTasksUtilities.WaitWhile( () => sendedBody == null ).OrTimeout( TimeSpan.FromSeconds( 1 ) );
            
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
        
        [AsyncTest]
        public async IPandaTask SuccessCancel()
        {
            var cancelTocken = new CancellationTokenSource();
            
            var requestBody = new UrlLoadingRequest( bundleName, typeof( AssetBundle ), new ProgressTracker< float >() );

            bool outCalled = false;
            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) => { outCalled = true; };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );
            
            cancelTocken.Cancel();

            await PandaTasksUtilities.Delay( TimeSpan.FromSeconds( 1 ) );
            Assert.Null( _workProcessor.Exception );
            Assert.IsFalse( outCalled );
        }
    }
}