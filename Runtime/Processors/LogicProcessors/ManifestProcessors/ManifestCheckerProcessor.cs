using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ManifestCheckerProcessor : AbstractRequestInputProcessor< UrlLoadingRequest >
    {
        protected IManifest _manifest;

        private BaseOutput< UrlLoadingRequest > _existOutput = new BaseOutput<UrlLoadingRequest>( OutputHandlingType.Optional );
        private BaseOutput< UrlLoadingRequest > _notExistOutput = new BaseOutput<UrlLoadingRequest>( OutputHandlingType.Optional );

        public IOutputNode< UrlLoadingRequest > ExistOutput => _existOutput;
        public IOutputNode< UrlLoadingRequest > NotExistOutput => _notExistOutput;

        public ManifestCheckerProcessor( IManifest manifest )
        {
            _manifest = manifest ?? throw new ArgumentNullException( nameof( manifest ) );
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( _manifest.ContainsAsset( body.Url ) )
                _existOutput.ProcessMessage( header, body );
            else
                _notExistOutput.ProcessMessage( header, body );
        }
    }
}
