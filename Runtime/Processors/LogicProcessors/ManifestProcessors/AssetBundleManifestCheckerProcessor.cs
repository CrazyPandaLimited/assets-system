using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetBundleManifestCheckerProcessor : AbstractRequestInputProcessor< UrlLoadingRequest >
    {
        protected AssetBundleManifest _manifest;
        protected bool _checkInBundles;

        private BaseOutput< UrlLoadingRequest > _existOutput = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );
        private BaseOutput< UrlLoadingRequest > _notExistOutput = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );

        public IOutputNode< UrlLoadingRequest > ExistOutput => _existOutput;
        public IOutputNode< UrlLoadingRequest > NotExistOutput => _notExistOutput;

        public AssetBundleManifestCheckerProcessor( AssetBundleManifest manifest, bool checkInBundles, bool checkInAssets )
        {
            if( checkInBundles && checkInAssets || !checkInBundles && !checkInAssets )
            {
                throw new InvalidOperationException( "Invalid" );
            }

            _manifest = manifest ?? throw new ArgumentNullException( nameof( manifest ) );
            _checkInBundles = checkInBundles;
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            var exists = _checkInBundles ? _manifest.ContainsBundle( body.Url ) : _manifest.ContainsAsset( body.Url );

            if( exists )
                _existOutput.ProcessMessage( header, body );
            else
                _notExistOutput.ProcessMessage( header, body );
        }
    }
}
