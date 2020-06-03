using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;
using UnityEngine.Networking;
using Object = System.Object;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class WebRequestLoadProcessor< T > : AbstractWebRequestLoadProcessor< T >
    {
        #region Protected Fields
        protected readonly IAntiCacheUrlResolver _antiCacheUrlResolver;
        protected readonly List< IAssetDataCreator > _assetTypeDataCreators;
        #endregion

        #region Constructors
        public WebRequestLoadProcessor( List< IAssetDataCreator > assetTypeDataCreators, WebRequestSettings webRequestSettings = null, IAntiCacheUrlResolver antiCacheUrlResolver = null ) : base( webRequestSettings )
        {
            _assetTypeDataCreators = assetTypeDataCreators ?? throw new ArgumentNullException( nameof(assetTypeDataCreators) );
            _antiCacheUrlResolver = antiCacheUrlResolver;
        }
        #endregion

        #region Protected Members
        
        protected override UnityWebRequest GetRequestData( MessageHeader header, UrlLoadingRequest body )
        {
            string uriWithAnticache = body.Url;
            if( _antiCacheUrlResolver != null )
            {
                uriWithAnticache = _antiCacheUrlResolver.ResolveURL( uriWithAnticache );
            }

            return UnityWebRequest.Get( uriWithAnticache );
        }

        
        protected override void OnLoadingCompleted( RequestProcessorData data )
        {
            bool isCreatorFounded = false;
            Object asset = null;
            var header = data.Header;
            var body = data.Body;
            var webRequest = data.RequestLoadingOperation.webRequest;
            foreach( var assetDataCreator in _assetTypeDataCreators )
            {
                if( assetDataCreator.Supports( body.AssetType ) )
                {
                    isCreatorFounded = true;
                    asset = assetDataCreator.Create( webRequest.downloadHandler.data, body.AssetType );
                }
            }

            webRequest.Dispose();

            if( !isCreatorFounded )
            {
                header.AddException( new AssetDataCreatorNotFoundException( body.AssetType, this, header, body ) );
                _exceptionConnection.ProcessMessage( header, body );
                return;
            }

            _defaultConnection.ProcessMessage( header, new AssetLoadingRequest< T >( body, ( T ) asset ) );
        }
        #endregion
    }
}
