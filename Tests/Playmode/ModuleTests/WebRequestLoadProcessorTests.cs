using System;
using System.Linq;
using Castle.Core.Internal;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using Object = UnityEngine.Object;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.AssetsSystem.Tests;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    [NUnit.Framework.Category("ModuleTests")]
    [NUnit.Framework.Category("ServerTests")]
    public sealed class WebRequestLoadProcessorTests : BaseProcessorModuleWithOneOutTest< WebRequestLoadProcessor< Object >, UrlLoadingRequest, AssetLoadingRequest< Object > >
    {
        private const float MaxDownloadProgress = 1.0f;
        private UrlLoadingRequest BaseSendedBody => _workProcessor.GetSendedHeaders().BaseSendedBody;
        private string ExistingFileOnServerUrl => ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" );

        protected override void InternalSetup() => _workProcessor = new WebRequestLoadProcessor< Object >( new List< IAssetDataCreator > { new TextureDataCreator(), new StringDataCreator() } );

        [ UnityTest ]
        public IEnumerator SuccessLoadFile()
        {
            var requestBody = new UrlLoadingRequest( ExistingFileOnServerUrl, typeof( Texture ), new ProgressTracker< float >() );

            WebRequestLoadProcessorTestUtils.MessageHeadersData data = _workProcessor.GetSendedHeaders();
            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            yield return WaitForTimeOut( BaseSendedBody );

            Assert.Null( _workProcessor.Exception );

            Assert.NotNull( data.BaseSendedBody );
            Assert.NotNull( data.AssetSendedBody.Asset );
            Assert.AreEqual( typeof( Texture2D ), data.AssetSendedBody.Asset.GetType() );
        }

        [ UnityTest ]
        public IEnumerator FailIfWrongURL()
        {
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl( "notExistFile.jpg" ), typeof( Texture ), new ProgressTracker< float >() );
            WebRequestLoadProcessorTestUtils.MessageHeadersData data = _workProcessor.GetSendedHeaders();

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            yield return WaitForTimeOut( BaseSendedBody );

            Assert.Null( _workProcessor.Exception );
            Assert.NotNull( data.BaseSendedBody );
            Assert.Null( data.AssetSendedBody );
            Assert.NotNull( data.SendedHeader.Exceptions );
        }

        [ UnityTest ]
        public IEnumerator SuccessCancel()
        {
            var cancelTocken = new CancellationTokenSource();
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" ), typeof( Texture ), new ProgressTracker< float >() );

            WebRequestLoadProcessorTestUtils.MessageHeadersData data = _workProcessor.GetSendedHeaders();

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), cancelTocken.Token ), requestBody );

            cancelTocken.Cancel();

            yield return WaitForTimeOut( BaseSendedBody );

            Assert.Null( _workProcessor.Exception );
            Assert.Null( data.BaseSendedBody );
        }

        [ Test ]
        public void SyncLoadFail()
        {
            AggregateException exception = null;
            var exceptionConnection = Substitute.For< IInputNode< UrlLoadingRequest > >();
            exceptionConnection.When( node => node.ProcessMessage( Arg.Any< MessageHeader >(), Arg.Any< UrlLoadingRequest >() ) )
                               .Do( callInfo =>
                               {
                                   var header =(MessageHeader) callInfo[ 0 ];
                                   exception = header.Exceptions;
                               });

            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl( "notExistFile.jpg" ), typeof( Texture ), new ProgressTracker< float >() );

            WebRequestLoadProcessorTestUtils.MessageHeadersData data = _workProcessor.GetSendedHeaders();
            _workProcessor.ExceptionOutput.LinkTo( exceptionConnection );

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ), CancellationToken.None ), requestBody );
            
            Assert.NotNull( exception );
            Assert.Null( _workProcessor.Exception );
        }

        [ UnityTest ]
        public IEnumerator FailWithAssetCreatorNotFound()
        {
            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl( "logo_test.jpg" ), typeof( GameObject ), new ProgressTracker< float >() );

            WebRequestLoadProcessorTestUtils.MessageHeadersData data = _workProcessor.GetSendedHeaders();

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            yield return WaitForTimeOut( BaseSendedBody );

            Assert.Null( _workProcessor.Exception );
            Assert.That( data.SendedHeader.Exceptions.InnerExceptions, Has.Some.Matches< Exception >( e => e is AssetDataCreatorNotFoundException ) );
            Assert.NotNull( data.SendedHeader );
            Assert.NotNull( data.BaseSendedBody );
        }

        [ UnityTest ]
        public IEnumerator ProgressTrackedTest()
        {
            var currentDownloadProgress = 0f;
            var requestBody = new UrlLoadingRequest( ExistingFileOnServerUrl, typeof( Texture ), new ProgressTracker< float >() );
            requestBody.ProgressTracker.OnProgressChanged += ( sender, args ) => currentDownloadProgress = args.progress;

            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData(), CancellationToken.None ), requestBody );

            yield return WaitForTimeOut( BaseSendedBody );

            Assert.AreEqual( MaxDownloadProgress, currentDownloadProgress );
            Assert.Null( _workProcessor.Exception );
        }
    }

    public static class WebRequestLoadProcessorTestUtils
    {
        public static MessageHeadersData GetSendedHeaders( this WebRequestLoadProcessor< UnityEngine.Object > workProcessor )
        {
            MessageHeadersData data = new MessageHeadersData();

            var outputs = workProcessor.Outputs;
            foreach( var output in outputs )
            {
                output.MessageSent += ( sender, args ) =>
                {
                    data.SendedHeader = args.Header;
                    data.AssetSendedBody = args.Body as AssetLoadingRequest< Object >;
                    data.BaseSendedBody = ( UrlLoadingRequest )args.Body;
                };
            }
            return data;
        }

        public sealed class MessageHeadersData
        {
            public MessageHeader SendedHeader { get; set; }
            public AssetLoadingRequest< Object > AssetSendedBody { get; set; }
            public UrlLoadingRequest BaseSendedBody { get; set; }
        }
    }
}