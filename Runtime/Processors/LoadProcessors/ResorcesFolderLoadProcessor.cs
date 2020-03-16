using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ResorcesFolderLoadProcessor : AbstractRequestProcessor< ResourceRequest, UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                Object asset = null;
                if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
                {
                    var subAssets = Resources.LoadAll( body.Url, body.AssetType );
                    var subAssetName = header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );

                    foreach( var subAsset in subAssets )
                    {
                        if( subAsset.name == subAssetName )
                        {
                            asset = subAsset;
                            break;
                        }
                    }
                }
                else
                {
                    asset = Resources.Load( body.Url, body.AssetType );
                }

                if( asset == null )
                {
                    header.AddException( new AssetSystemException( $"You try to load '{body.Url}' of type {body.AssetType}. This asset not found in project." ) );
                    _exceptionConnection.ProcessMessage( header, body );
                    return FlowMessageStatus.Accepted;
                }

                _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< Object >( body, asset ) );
                return FlowMessageStatus.Accepted;
            }

            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                header.AddException( new AssetSystemException( $"You try to ASYNC load subAsset:{header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET )} for asset '{body.Url}' of type {body.AssetType}. Async loading for subAssets not supported by Unity3d API!!" ) );
                _exceptionConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            ConfigureLoadingProcess( new RequestProcessorData( Resources.LoadAsync( body.Url, body.AssetType ), header, body ) );
            return FlowMessageStatus.Accepted;
        }

        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );

        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( FinalProgress );
            _defaultConnection.ProcessMessage( data.Header, new AssetLoadingRequest< Object >( data.Body, data.RequestLoadingOperation.asset ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.asset == null )
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
