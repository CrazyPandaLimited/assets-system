using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public sealed class BundleDepsLoadingProcessor : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< UrlLoadingRequest, UrlLoadingRequest, UrlLoadingRequest >
    {
        private const float FinalProgress = 1.0f;

        private readonly AssetBundleManifest _manifest;
        private readonly IAssetsStorage _assetsStorage;

        public BundleDepsLoadingProcessor( AssetBundleManifest manifest, IAssetsStorage assetsStorage )
        {
            _manifest = manifest ?? throw new ArgumentNullException( nameof(manifest) );
            _assetsStorage = assetsStorage ?? throw new ArgumentNullException( nameof(assetsStorage) );
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            LoadBundles( header, body );
        }

        private IEnumerable< string > GetAssetBundlesNamesToLoad( string assetPath ) => _manifest.AssetInfos.TryGetValue( assetPath, out var assetInfo ) ? assetInfo.Dependencies : Enumerable.Empty< string >();
        
        private void LoadBundles( MessageHeader header, UrlLoadingRequest body )
        {
            var cancelToken = new CancellationTokenSource();
            var downloadableData = CreateDownloadableData( GetAssetBundlesNamesToLoad( body.Url ), MetaDataFactory.CreateMetadata( header ), cancelToken.Token );

            header.CancellationToken.Register( () => { cancelToken.Cancel(); } );

            var compositeProgressTracker = new CompositeProgressTracker( downloadableData.progressTrackers );
            compositeProgressTracker.OnProgressChanged += ( sender, args ) => { body.ProgressTracker.ReportProgress( args.progress ); };

            var thereIsNoAnyBundleToLoad = downloadableData.loadingTasks.Count == 0;

            if( thereIsNoAnyBundleToLoad )
            {
                InvokeDownloadFinishedEvent();
            }
            else
            {
                PandaTasksUtilitys.WaitAll( downloadableData.loadingTasks ).Done( InvokeDownloadFinishedEvent ).Fail( header.AddException );
            }
            
            void InvokeDownloadFinishedEvent()
            {
                body.ProgressTracker.ReportProgress( FinalProgress );
                SendOutput( header, body );
            }
        }

        private (IReadOnlyList< ProgressTracker< float > > progressTrackers, IReadOnlyList< IPandaTask< AssetBundle > > loadingTasks) CreateDownloadableData( IEnumerable< string > assetsToLoad, MetaData requestMetaData, CancellationToken taskCancelToken )
        {
            var progressTrackers = new List< ProgressTracker< float > >();
            var loadingTasks = new List< IPandaTask< AssetBundle > >();

            foreach( var bundleName in assetsToLoad )
            {
                var progressTracker = new ProgressTracker< float >();
                progressTrackers.Add( progressTracker );
                loadingTasks.Add( _assetsStorage.LoadAssetAsync< AssetBundle >( bundleName, requestMetaData, taskCancelToken, progressTracker ) );
            }

            return ( progressTrackers, loadingTasks );
        }
        
        public static class MetaDataFactory
        {
            public static MetaData CreateMetadata( MessageHeader header )
            {
                var doesItIsSyncRequest = header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
                var metadataForBundlesRequest = doesItIsSyncRequest ? new MetaData( MetaDataReservedKeys.SYNC_REQUEST_FLAG ) : new MetaData();
                metadataForBundlesRequest.SetMeta( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY, new object() );
                return metadataForBundlesRequest;
            }
        }
    }
}
