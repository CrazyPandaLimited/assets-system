using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AddAssetToCacheWithRefcountProcessor< T > : AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< AssetLoadingRequest< T >, AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        private ICacheControllerWithAssetReferences _cacheController;
        
        public AddAssetToCacheWithRefcountProcessor( ICacheControllerWithAssetReferences cacheController )
        {
            _cacheController = cacheController;
        }

        protected override void InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            var assetName = body.Url;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.GET_SUB_ASSET ) )
            {
                assetName = Utils.ConstructAssetWithSubassetName( body.Url, header.MetaData.GetMeta< string >( MetaDataReservedKeys.GET_SUB_ASSET ) );
            }

            if( !header.MetaData.IsMetaExist( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY ) )
            {
                header.AddException( new MetaDataNotContainsReferenceObjectForAsset( this, header, body ) );
                SendException( header, new UrlLoadingRequest( body ) );
                return;
            }

            _cacheController.Add( body.Asset, assetName, header.MetaData.GetOwnerReference() );

            SendOutput( header, body );
        }
    }
}
