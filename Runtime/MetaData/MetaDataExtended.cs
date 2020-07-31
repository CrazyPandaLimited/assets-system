using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class MetaDataExtended : MetaData
    {
        #region Constructors
        public MetaDataExtended()
        {
        }

        public MetaDataExtended( object ownerReference )
        {
            SetMeta( MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY, ownerReference );
        }
        #endregion
    }
}
