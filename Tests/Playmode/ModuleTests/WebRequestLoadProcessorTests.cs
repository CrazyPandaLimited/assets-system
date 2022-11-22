using System;
using System.Linq;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using Object = UnityEngine.Object;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.AssetsSystem.Tests;
using NSubstitute;
using NUnit.Framework;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.PandaTasks;
using UnityEngine;
using UnityEngine.TestTools;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
#if !UNITY_EDITOR
    [Ignore("")]
#endif
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
            Assert.NotNull( data.BaseSendedBody );
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

        [ AsyncTest ]
        public async Task RetryUrlRequestShouldSucceed()
        {
            const uint maxRetriesCount = 5;
            uint retriesCount = 0;

            var retriesConfig = new Dictionary< int, float >();

            for( int i = 1; i <= maxRetriesCount; i++ )
            {
                retriesConfig[ i ] = 0.001f;
            }

            RequestRetryProcessor retryProcessor = new RequestRetryProcessor( retriesConfig, ( _, __ ) => true );

            TaskCompletionSource< object > taskCompletionSource = new TaskCompletionSource< object >();

            var retryConnection = Substitute.For< IInputNode< UrlLoadingRequest > >();

            retryConnection.When( node => node.ProcessMessage( Arg.Any< MessageHeader >(), Arg.Any< UrlLoadingRequest >() ) )
                               .Do( callInfo =>
                               {
                                   retriesCount++;
                                   _workProcessor.DefaultInput.ProcessMessage( ( MessageHeader )callInfo[0], ( UrlLoadingRequest )callInfo[1] );
                               });

            var finishConnection = Substitute.For< IInputNode< UrlLoadingRequest > >();
            finishConnection.When(  node => node.ProcessMessage( Arg.Any<MessageHeader>(), Arg.Any<UrlLoadingRequest>() ))
                            .Do( _ =>
                            {
                                taskCompletionSource.SetResult( default );
                            } );
            
            _workProcessor.ExceptionOutput.LinkTo( retryProcessor );
            retryProcessor.RetryOutput.LinkTo( retryConnection );
            retryProcessor.AllRetrysFailedOutput.LinkTo( finishConnection );

            var requestBody = new UrlLoadingRequest( ResourceStorageTestUtils.ConstructTestUrl( "notExistFile.jpg" ), typeof( Texture ), new ProgressTracker< float >() );
            _workProcessor.DefaultInput.ProcessMessage( new MessageHeader( new MetaData( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY ), CancellationToken.None ), requestBody );

            await taskCompletionSource.Task;
            
            Assert.That( maxRetriesCount , Is.EqualTo( retriesCount ) );
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
            requestBody.ProgressTracker.OnProgressChanged += ( progress ) => currentDownloadProgress = progress;

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