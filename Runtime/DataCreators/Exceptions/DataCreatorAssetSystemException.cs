using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class DataCreatorAssetSystemException : AssetSystemException
    {
        #region Properties
        public Type RequestedType { get; protected set; }
        #endregion

        #region Constructors
        public DataCreatorAssetSystemException( string message, Type requestedType ) : this( message, requestedType, null )
        {
        }

        public DataCreatorAssetSystemException( string message, Type requestedType, Exception innerException ) : base( message, innerException )
        {
            RequestedType = requestedType;
        }
        #endregion
    }
}
