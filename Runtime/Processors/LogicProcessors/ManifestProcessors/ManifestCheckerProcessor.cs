using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class ManifestCheckerProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        #region Protected Fields
        protected IManifest _manifest;
        protected Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > > _checkResultOutConnections = new Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > >( 2 );
        #endregion

        #region Constructors
        public ManifestCheckerProcessor( IManifest manifest )
        {
            _manifest = manifest ?? throw new ArgumentNullException( $"{nameof(manifest)} == null" );
        }
        #endregion

        #region Public Members
        public void RegisterExistOutConnection( IInputNode< UrlLoadingRequest > node )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( node );
            RegisterConnection( connection );
            _checkResultOutConnections.Add( true, connection );
        }

        public void RegisterNotExistOutConnection( IInputNode< UrlLoadingRequest > node )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( node );
            RegisterConnection( connection );
            _checkResultOutConnections.Add( false, connection );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            _checkResultOutConnections[ _manifest.ContainsAsset( body.Url ) ].ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
