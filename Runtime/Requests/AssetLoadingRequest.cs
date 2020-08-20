using System;
using CrazyPanda.UnityCore.PandaTasks.Progress;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetLoadingRequest< T > : UrlLoadingRequest
    {
        #region Properties
        public T Asset { get; private set; }
        #endregion

        #region Constructors
        public AssetLoadingRequest( string url, Type assetType, IProgressTracker< float > progressTracker, T asset ) : base( url, assetType, progressTracker )
        {
            Asset = asset;
        }

        public AssetLoadingRequest( UrlLoadingRequest urlRequest, T asset ) : base( urlRequest )
        {
            Asset = asset;
        }
        
        public override string ToString()
        {
            return $"AssetLoadingRequest hasAsset:{Asset != null} {base.ToString()}";
        }
        #endregion
    }
}
