using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ResorcesFolderLoadProcessor : AbstractRequestProcessor< ResourceRequest, UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
        #region Protected Members
        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
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
                    header.AddException( new AssetNotLoadedException( $"Asset not found in project", this, header, body ) );
                    SendException( header, body );
                    return;
                }

                SendOutput( header, new AssetLoadingRequest< Object >( body, asset ) );
                return;
            }

            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                header.AddException( new AssetNotLoadedException( $"Async loading for subAssets not supported by Unity3d API", this, header, body ) );
                SendException( header, body );
                return;
            }

            ConfigureLoadingProcess( new RequestProcessorData( Resources.LoadAsync( body.Url, body.AssetType ), header, body ) );
        }

        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );

        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( FinalProgress );
            SendOutput( data.Header, new AssetLoadingRequest< Object >( data.Body, data.RequestLoadingOperation.asset ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.asset == null )
            {
                data.Header.AddException( new AssetNotLoadedException( "Asset not loaded", this, data.Header, data.Body ) );
                SendException( data.Header, data.Body );
                return false;
            }

            return true;
        }

        protected override void OnErrorLoading( RequestProcessorData data )
        {
            if( Exception != null && Status == FlowNodeStatus.Failed )
            {
                data.Header.AddException( Exception );
                SendException( data.Header, data.Body );
            }
        }
        #endregion
    }
}
