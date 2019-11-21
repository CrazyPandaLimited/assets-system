using System;
using System.Collections;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ResorcesFolderLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected ICoroutineManager _coroutineManager;
        #endregion

        #region Constructors
        public ResorcesFolderLoadProcessor( ICoroutineManager coroutineManager )
        {
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( nameof(coroutineManager) );
        }

        public ResorcesFolderLoadProcessor()
        {
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                var asset = Resources.Load( body.Url, body.AssetType );

                if( asset == null )
                {
                    header.AddException( new AssetSystemException( $"You try to load '{body.Url}' of type {body.AssetType}. This asset not found in project." ) );
                    _exceptionConnection.ProcessMessage( header, body );
                    return FlowMessageStatus.Accepted;
                }

                _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body, asset ) );
                return FlowMessageStatus.Accepted;
            }

            if( _coroutineManager == null )
            {
                ProcessException( header, body, new Exception( "Need ICoroutineManager for async processing this request" ) );
                return FlowMessageStatus.Rejected;
            }

            _coroutineManager.StartCoroutine( this, LoadProcess( header, body ), ( o, exception ) => { ProcessException( header, body, exception ); } );
            return FlowMessageStatus.Accepted;
        }

        protected override void InternalRestore()
        {
        }

        protected IEnumerator LoadProcess( MessageHeader header, UrlLoadingRequest body )
        {
            var operation = Resources.LoadAsync( body.Url, body.AssetType );

            while( !operation.isDone )
            {
                body.ProgressTracker.ReportProgress( operation.progress );
                yield return null;
            }

            if( operation.asset == null )
            {
                header.AddException( new UnityAssetFolderLoaderException( "Asset not loaded" ) );
                _exceptionConnection.ProcessMessage( header, body );
                yield break;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body, operation.asset ) );
        }


        protected override void InternalDispose()
        {
        }
        #endregion
    }
}
