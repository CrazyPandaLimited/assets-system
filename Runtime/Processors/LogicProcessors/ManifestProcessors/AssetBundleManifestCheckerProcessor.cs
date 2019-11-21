using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetBundleManifestCheckerProcessor : AbstractRequestInputOutputProcessor< UrlLoadingRequest, UrlLoadingRequest >
    {
        #region Protected Fields
        protected AssetBundleManifest _manifest;
        protected bool _checkInBundles;
        protected Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > > _checkResultOutConnections = new Dictionary< bool, NodeOutputConnection< UrlLoadingRequest > >( 2 );
        #endregion

        #region Constructors
        public AssetBundleManifestCheckerProcessor( AssetBundleManifest manifest, bool checkInBundles, bool checkInAssets )
        {
            if( checkInBundles && checkInAssets || !checkInBundles && !checkInAssets )
            {
                throw new InvalidOperationException( "Invalid" );
            }

            _manifest = manifest ?? throw new ArgumentNullException( $"{nameof(manifest)} == null" );
            _checkInBundles = checkInBundles;
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
            _checkResultOutConnections[ _checkInBundles ? _manifest.ContainsBundle( body.Url ) : _manifest.ContainsAsset( body.Url ) ].ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}
