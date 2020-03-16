using System;
using System.IO;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class BundlesFromLocalFolderLoadProcessor : AbstractRequestProcessor< AssetBundleCreateRequest, UrlLoadingRequest, AssetLoadingRequest< AssetBundle>, UrlLoadingRequest >
    {
        #region Protected Fields
        protected readonly string _localFolder;
        #endregion

        #region Properties
        public AssetBundleManifest Manifest { get; }
        #endregion

        #region Constructors
        public BundlesFromLocalFolderLoadProcessor( string localFolder, AssetBundleManifest manifest )
        {
            _localFolder = localFolder ?? throw new ArgumentNullException( nameof(localFolder) );

            if( manifest == null )
            {
                Manifest = new AssetBundleManifest();
            }
            else
            {
                Manifest = manifest;
            }
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( !Manifest.BundleInfos.ContainsKey( body.Url ) )
            {
                header.AddException( new AssetSystemException( "Manifest not contains bundle" ));
                _exceptionConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            var bundleInfo = Manifest.BundleInfos[ body.Url ];

            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                var loadedBundle = string.IsNullOrEmpty( bundleInfo.CRC ) ? AssetBundle.LoadFromFile( Path.Combine( _localFolder, bundleInfo.Name ) ) : AssetBundle.LoadFromFile( Path.Combine( _localFolder, bundleInfo.Name ), uint.Parse( bundleInfo.CRC ) );
                _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< AssetBundle >( body, loadedBundle ) );
                return FlowMessageStatus.Accepted;
            }

            var loadingProcess = string.IsNullOrEmpty( bundleInfo.CRC ) ? AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ) ) : AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ), uint.Parse( bundleInfo.CRC ) );
            ConfigureLoadingProcess( new RequestProcessorData( loadingProcess, header, body ) );
            
            return FlowMessageStatus.Accepted;
        }

        protected override void InternalRestore() => Status = FlowNodeStatus.Working;
        
        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );
        
        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( FinalProgress );
            _defaultConnection.ProcessMessage( data.Header, new AssetLoadingRequest< AssetBundle >( data.Body, data.RequestLoadingOperation.assetBundle ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.assetBundle == null )
            {
                data.Header.AddException( new AssetSystemException( "Bundle not loaded" ));
                _exceptionConnection.ProcessMessage( data.Header, data.Body );
                return false;
            }

            return true;
        }
        
        #endregion
    }
}
