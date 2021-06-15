using System;
using System.IO;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class BundlesFromLocalFolderLoadProcessor : AbstractRequestProcessor< AssetBundleCreateRequest, UrlLoadingRequest, AssetLoadingRequest< AssetBundle >, UrlLoadingRequest >
    {
        protected readonly string _localFolder;

        public AssetBundleManifest Manifest { get; }

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

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( !Manifest.BundleInfos.ContainsKey( body.Url ) )
            {
                header.AddException( new AssetNotLoadedException( "Manifest not contains bundle", this, header, body ) );
                SendException( header, body );
                return;
            }

            var bundleInfo = Manifest.BundleInfos[ body.Url ];

            if( header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) )
            {
                var loadedBundle = string.IsNullOrEmpty( bundleInfo.CRC ) ? AssetBundle.LoadFromFile( Path.Combine( _localFolder, bundleInfo.Name ) ) : AssetBundle.LoadFromFile( Path.Combine( _localFolder, bundleInfo.Name ), uint.Parse( bundleInfo.CRC ) );
                SendOutput( header, new AssetLoadingRequest< AssetBundle >( body, loadedBundle ) );
                return;
            }

            var loadingProcess = string.IsNullOrEmpty( bundleInfo.CRC ) ? AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ) ) : AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ), uint.Parse( bundleInfo.CRC ) );
            ConfigureLoadingProcess( new RequestProcessorData( loadingProcess, header, body ) );
        }
        
        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );
        
        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            data.Body.ProgressTracker.ReportProgress( FinalProgress );
            SendOutput( data.Header, new AssetLoadingRequest< AssetBundle >( data.Body, data.RequestLoadingOperation.assetBundle ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.assetBundle == null )
            {
                data.Header.AddException( new AssetNotLoadedException( "Bundle not loaded", this, data.Header, data.Body ) );
                SendException( data.Header, data.Body );
                return false;
            }

            return true;
        }
    }
}
