using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using UnityEngine.Networking;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class BundlesFromWebRequestLoadProcessor : AbstractWebRequestLoadProcessor< AssetBundle >
    {
        #region Properties
        public AssetBundleManifest Manifest { get; }
        public IAntiCacheUrlResolver AntiCacheUrlResolver { get; protected set; }
        public virtual string ServerUrl { get; set; }
        #endregion

        #region Constructors
        public BundlesFromWebRequestLoadProcessor( string serverUrl, AssetBundleManifest manifest, WebRequestSettings webRequestSettings = null, IAntiCacheUrlResolver antiCacheUrlResolver = null ) : base( webRequestSettings )
        {
            ServerUrl = serverUrl ?? throw new ArgumentNullException( nameof(serverUrl) );

            if( manifest == null )
            {
                Manifest = new AssetBundleManifest();
            }
            else
            {
                Manifest = manifest;
            }

            AntiCacheUrlResolver = antiCacheUrlResolver;
        }
        #endregion

        #region Protected Members


        protected override UnityWebRequest GetRequestData(MessageHeader header, UrlLoadingRequest body )
        {
            var bundleInfo = Manifest.BundleInfos[ body.Url ];
            string uriWithAnticache = ServerUrl + "/" + bundleInfo.Name;
            if( AntiCacheUrlResolver != null )
            {
                uriWithAnticache = AntiCacheUrlResolver.ResolveURL( ServerUrl, bundleInfo.Name );
            }

            var webRequest = bundleInfo.CRC == null || bundleInfo.Hash == null ? UnityWebRequestAssetBundle.GetAssetBundle( uriWithAnticache ) :
                       UnityWebRequestAssetBundle.GetAssetBundle( uriWithAnticache, Hash128.Parse( bundleInfo.Hash ), uint.Parse( bundleInfo.CRC ) );
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            return webRequest;
        }

        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            var body = data.Body;
            var header = data.Header;
            var webRequest = data.RequestLoadingOperation.webRequest;
            var assetBundleCreateRequest = AssetBundle.LoadFromMemoryAsync( webRequest.downloadHandler.data );
            assetBundleCreateRequest.completed += OnAssetBundleLoaded;

            void OnAssetBundleLoaded( AsyncOperation operation )
            {
                assetBundleCreateRequest.completed -= OnAssetBundleLoaded;
                SendOutput( header, new AssetLoadingRequest< AssetBundle >( body, assetBundleCreateRequest.assetBundle ) );
                webRequest.Dispose();
            }
        }
        
        #endregion
    }
}
