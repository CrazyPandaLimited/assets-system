using System;
using System.Linq;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using System.Collections;
using System.Threading;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("LocalTests")]
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

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), _body );

            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }

        [ UnityTest ]
        public IEnumerator LoadFromResourceFolderRootResourceAsync()
        {
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };
            
            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), _body );

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

            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };

            var metadata = new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
            metadata.SetMeta( MetaDataReservedKeys.GET_SUB_ASSET, subAssetName );
            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( metadata, CancellationToken.None ), _body );

            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( sendedBody );
            Assert.NotNull( sendedBody.Asset );
        }

        [ Test ]
        public void ErrorLoadFromResourceFolderRootResourceSubAssetAsync()
        {
            AggregateException exception = null;
            var exceptionConnection = Substitute.For< IInputNode< UrlLoadingRequest > >();
            exceptionConnection.When( node => node.ProcessMessage( Arg.Any< MessageHeader >(), Arg.Any< UrlLoadingRequest >() ) )
                               .Do( callInfo =>
                               {
                                   var header =(MessageHeader) callInfo[ 0 ];
                                   exception = header.Exceptions;
                               });

            _body = new UrlLoadingRequest( multipleSpriteAtlasUrl, typeof( Sprite ), new ProgressTracker< float >() );
            MessageHeader sendedHeader = null;
            AssetLoadingRequest< UnityEngine.Object > sendedBody = null;

            _workProcessor.ExceptionOutput.LinkTo( exceptionConnection );
            
            _workProcessor.DefaultOutput.MessageSent += ( sender, args ) =>
            {
                sendedHeader = args.Header;
                sendedBody = ( AssetLoadingRequest< UnityEngine.Object > ) args.Body;
            };
            
            var metadata = new MetaData();
            metadata.SetMeta( MetaDataReservedKeys.GET_SUB_ASSET, subAssetName );
            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( metadata, CancellationToken.None ), _body );

            Assert.Null( _workProcessor.Exception );
            Assert.Null( sendedBody );
            Assert.NotNull( exception );
        }
        
        
        #endregion
    }
}