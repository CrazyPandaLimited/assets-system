using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.PandaTasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public sealed class AssetFromBundleLoadProcessor : AbstractRequestProcessor< AssetBundleRequest, UrlLoadingRequest, AssetLoadingRequest< Object >, UrlLoadingRequest >
    {
        private readonly AssetBundleManifest _manifest;
        private readonly IAssetsStorage _assetsStorage;

        public AssetFromBundleLoadProcessor( AssetBundleManifest manifest, IAssetsStorage assetsStorage )
        {
            _manifest = manifest ?? throw new ArgumentNullException( nameof(manifest) );
            _assetsStorage = assetsStorage ?? throw new ArgumentNullException( nameof(assetsStorage) );
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            StartToLoadMainBundle( header, body );
        }
        
        protected override void OnLoadingStarted( MessageHeader header, UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( InitialProgress );

        protected override void OnLoadingProgressUpdated( UrlLoadingRequest body, float currentProgress ) => body.ProgressTracker.ReportProgress( currentProgress );

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            ReportFinalProgress( data.Body );
            var finalAsset = GetFinalAsset( data.Header, () => data.RequestLoadingOperation.allAssets, () => data.RequestLoadingOperation.asset );
            SendOutput( data.Header, new AssetLoadingRequest< Object >( GetAssetNameToLoad( data.Body ), data.Body.AssetType, data.Body.ProgressTracker, finalAsset ) );
        }

        protected override bool LoadingFinishedWithoutErrors( RequestProcessorData data )
        {
            if( data.RequestLoadingOperation.asset == null )
            {
                data.Header.AddException( new AssetNotLoadedException( "Asset not loaded", this, data.Header, data.Body ) );
                SendException( data.Header, new AssetLoadingRequest< Object >( GetAssetNameToLoad( data.Body ), data.Body.AssetType, data.Body.ProgressTracker, data.RequestLoadingOperation.asset ) );
                return false;
            }

            return true;
        }

        private void StartToLoadMainBundle( MessageHeader header, UrlLoadingRequest body )
        {
            var cancelToken = new CancellationTokenSource();
            header.CancellationToken.RegisterIfCanBeCanceled( () => { cancelToken.Cancel(); } );

            var mainBundleLoader = _assetsStorage.LoadAssetAsync< AssetBundle >( GetMainBundleName( body ), BundleDepsLoadingProcessor.MetaDataFactory.CreateMetadata( header ), cancelToken.Token, body.ProgressTracker );
            mainBundleLoader.Done( mainBundle => StartToLoadAssetFromBundle( mainBundle, header, body ) ).Fail( header.AddException );
        }
        
        private void StartToLoadAssetFromBundle( AssetBundle mainBundle, MessageHeader header, UrlLoadingRequest body )
        {
            var doesItIsSyncRequest = header.MetaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );

            if( doesItIsSyncRequest )
            {
                LoadAssetFromBundle( mainBundle, header, body );
            }
            else
            {
                LoadAssetFromBundleAsync( mainBundle, header, body );
            }
        }

        private void LoadAssetFromBundle( AssetBundle mainBundle, MessageHeader header, UrlLoadingRequest body )
        {
            var finalAsset = GetFinalAsset( header, () => mainBundle.LoadAssetWithSubAssets( GetAssetNameToLoad( body ), body.AssetType ), () => mainBundle.LoadAsset( GetAssetNameToLoad( body ), body.AssetType ) );
            OnAssetLoaded( finalAsset, header, body );
        }

        private void LoadAssetFromBundleAsync( AssetBundle mainBundle, MessageHeader header, UrlLoadingRequest body )
        {
            ConfigureLoadingProcess( new RequestProcessorData( GetAssetLoadingRequest(), header, body ) );

            AssetBundleRequest GetAssetLoadingRequest()
            {
                var needToLoadSubAssets = header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET );
                var assetNameToLoad = GetAssetNameToLoad( body );
                var assetTypeToLoad = body.AssetType;

                if( needToLoadSubAssets )
                {
                    return mainBundle.LoadAssetWithSubAssetsAsync( assetNameToLoad, assetTypeToLoad );
                }
                else
                {
                    return mainBundle.LoadAssetAsync( assetNameToLoad, assetTypeToLoad );
                }
            }
        }

        private void OnAssetLoaded( Object asset, MessageHeader header, UrlLoadingRequest body )
        {
            ReportFinalProgress( body );

            var assetNameToLoad = GetAssetNameToLoad( body );
            var assetTypeToLoad = body.AssetType;
            
            if( asset == null )
            {
                header.AddException( new AssetNotLoadedException( "Asset not loaded", this, header, body ) );
                SendException( header, new AssetLoadingRequest< Object >( assetNameToLoad, assetTypeToLoad, body.ProgressTracker, null ) );
                return;
            }

            SendOutput( header, new AssetLoadingRequest< Object >( assetNameToLoad, assetTypeToLoad, body.ProgressTracker, asset ) );
        }

        private Object GetFinalAsset( MessageHeader header, Func< IReadOnlyCollection< Object > > getSubAssets, Func< Object > getDefaultAsset )
        {
            Object asset = null;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                var subAssets = getSubAssets.Invoke();
                var subAssetName = header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET );
                asset = subAssets.FirstOrDefault( subAsset => subAsset.name == subAssetName );
            }
            else
            {
                asset = getDefaultAsset.Invoke();
            }

            return asset;
        }

        private string GetMainBundleName( UrlLoadingRequest body ) => _manifest.GetBundleByAssetName( body.Url ).Name;

        private string GetAssetNameToLoad( UrlLoadingRequest body ) => body.Url;

        private void ReportFinalProgress( UrlLoadingRequest body ) => body.ProgressTracker.ReportProgress( FinalProgress );
    }
}
