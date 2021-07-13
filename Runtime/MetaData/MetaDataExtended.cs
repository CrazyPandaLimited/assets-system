using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public static class MetaDataExtended 
    {
        public static MetaData CreateMetaDataWithOwner( object ownerReference )
        {
            var res = new MetaData();
            res.SetOwner( ownerReference );
            return res;
        }

        public static void SetOwner(this MetaData md, object ownerReference)
        {
            md.SetMeta( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY, ownerReference );
        }
    }
}
