using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddAssetToCacheWithRefcountProcessor< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< AssetLoadingRequest< T >, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Private Fields
        private ICacheControllerWithAssetReferences _cacheController;
        #endregion

        #region Constructors
        public AddAssetToCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cacheController )
        {
            _cacheController = cacheController;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( !header.MetaData.IsMetaExist( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY ) )
            {
                header.AddException( new MetaDataNotContainsReferenceObjectForAsset( "Owner not set" ) );
                _exceptionConnection.ProcessMessage( header, new UrlLoadingRequest( body ) );
                return FlowMessageStatus.Accepted;
            }

            _cacheController.Add( body.Asset, body.Url, header.MetaData.GetOwnerReference() );

            _defaultConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }

        protected override void InternalRestore()
        {
            Status = FlowNodeStatus.Working;
        }

        protected override void InternalDispose()
        {
        }
        #endregion
    }
}
