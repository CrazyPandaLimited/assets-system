using System;
using CrazyPanda.UnityCore.PandaTasks.Progress;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class UrlLoadingRequest : TrackProgressLoadingRequest
    {
        public string Url { get; protected set; }
        public Type AssetType { get; protected set; }

        public UrlLoadingRequest( string url, Type assetType, IProgressTracker< float > progressTracker ) : base( progressTracker )
        {
            Url = url;
            AssetType = assetType;
        }

        public UrlLoadingRequest( UrlLoadingRequest urlRequest ) : base( urlRequest.ProgressTracker )
        {
            Url = urlRequest.Url;
            AssetType = urlRequest.AssetType;
        }

        public override string ToString()
        {
            return $"UrlLoadingRequest Url:{Url} AssetType:{AssetType.ToString()}";
        }
    }
}
