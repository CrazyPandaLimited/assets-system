using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class MetaDataExtended : MetaData
    {
        public MetaDataExtended()
        {
        }

        public MetaDataExtended( object ownerReference )
        {
            SetMeta( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY, ownerReference );
        }
    }
}
