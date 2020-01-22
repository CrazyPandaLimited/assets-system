using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public class ResourceStorageResourceFolderTests : BaseProcessorModuleWithOneOutTest< ResorcesFolderLoadProcessor,UrlLoadingRequest, AssetLoadingRequest< UnityEngine.Object > >
    {
        private UrlLoadingRequest _body;
        private string url = "Cube";
        private string multipleSpriteAtlasUrl = "MultipleSpritesAtlas";
        private string subAssetName = "MultipleSpritesAtlas_0";
        #region Public Members
        
        protected override void InternalSetup()
        {
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

            yield return WaitForTimeOut( sendedBody );
            
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }
        
        [ Test ]
        public void LoadFromResourceFolderRootResourceSubAssetSync()
        {
            _body = new UrlLoadingRequest( multipleSpriteAtlasUrl, typeof( Sprite ), new ProgressTracker< float >() );
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var metadata = new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
            metadata.SetMeta( MetaDataReservedKeys.GET_SUB_ASSET, subAssetName );
            var status = _workProcessor.ProcessMessage( new MessageHeader( metadata, CancellationToken.None ), _body );

            Assert.AreEqual( FlowMessageStatus.Accepted, status );
            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }

        [ Test ]
        public void ErrorLoadFromResourceFolderRootResourceSubAssetAsync()
        {
            _body = new UrlLoadingRequest( multipleSpriteAtlasUrl, typeof( Sprite ), new ProgressTracker< float >() );
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.GetOutputs().First().OnMessageSended += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };
            
            var metadata = new MetaData();
            metadata.SetMeta( MetaDataReservedKeys.GET_SUB_ASSET, subAssetName );
            var status = _workProcessor.ProcessMessage( new MessageHeader( metadata, CancellationToken.None ), _body );

            Assert.AreEqual( FlowMessageStatus.Rejected, status );
            
            Assert.NotNull( _workProcessor.Exception );
            Assert.Null( sendedBody );
        }
        
        
        #endregion
    }
}