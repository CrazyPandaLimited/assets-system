using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public static class MetaDataExtensions
    {
        #region Public Members
        public static bool GetIsStaticFlag( this MetaData metaData )
        {
            return metaData.HasFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );
        }

        public static object GetOwnerReference( this MetaData metaData )
        {
            return metaData.GetMeta< object >( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY );
        }
        #endregion
    }
}
