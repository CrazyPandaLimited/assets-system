using System;
using System.Collections;
using System.IO;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class BundlesFromLocalFolderLoadProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, AssetLoadingRequest< AssetBundle >, UrlLoadingRequest >
    {
        #region Protected Fields
        protected readonly ICoroutineManager _coroutineManager;
        protected readonly string _localFolder;
        #endregion

        #region Properties
        public AssetBundleManifest Manifest { get; }
        #endregion

        #region Constructors
        public BundlesFromLocalFolderLoadProcessor( ICoroutineManager coroutineManager, string localFolder, AssetBundleManifest manifest )
        {
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( nameof(coroutineManager) );
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

            _coroutineManager.StartCoroutine( this, LoadingProcess( header, body, bundleInfo ), ( o, exception ) => { ProcessException( header,body, exception ); } );
            return FlowMessageStatus.Accepted;
        }

        protected override void InternalRestore()
        {
            Status = FlowNodeStatus.Working;
        }

        protected IEnumerator LoadingProcess( MessageHeader header, UrlLoadingRequest body, BundleInfo bundleInfo )
        {
            var loadingProcess = string.IsNullOrEmpty( bundleInfo.CRC ) ? AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ) ) : AssetBundle.LoadFromFileAsync( Path.Combine( _localFolder, bundleInfo.Name ), uint.Parse( bundleInfo.CRC ) );

            while( !loadingProcess.isDone )
            {
                if( header.CancellationToken.IsCancellationRequested )
                {
                    yield break;
                }

                body.ProgressTracker.ReportProgress( loadingProcess.progress );
                yield return null;
            }

            if( loadingProcess.assetBundle == null )
            {
                header.AddException( new AssetSystemException( "Bundle not loaded" ));
                _exceptionConnection.ProcessMessage( header, body );
                yield break;
            }
            
            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< AssetBundle >( body, loadingProcess.assetBundle ) );
        }

        protected override void InternalDispose()
        {
            _coroutineManager.StopAllCoroutinesForTarget( this );
        }
        #endregion
    }
}
