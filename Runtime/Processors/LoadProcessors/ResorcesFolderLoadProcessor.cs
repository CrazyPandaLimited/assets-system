using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ResorcesFolderLoadProcessor :  AbstractRequestProcessor<  ResourceRequest, UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
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

            ConfigureLoadingProcess( new RequestProcessorData( Resources.LoadAsync( body.Url, body.AssetType ), header, body ) );

            return FlowMessageStatus.Accepted;
        }

        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( 0f );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( 1.0f );
            _defaultConnection.ProcessMessage( data.Header, new AssetLoadingRequest< Object >( data.Body, data.RequestLoadingOperation.asset ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.asset == null)
            {
                data.Header.AddException( new AssetSystemException( "Asset not loaded" ) );
                _exceptionConnection.ProcessMessage( data.Header, data.Body );
                return false;
            }

            return true;            
        }
        
        #endregion
    }
}
