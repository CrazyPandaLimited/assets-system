#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class DataCreatorResourceSystemException : ResourceSystemException
    {
        #region Properties
        public Type RequestedType { get; protected set; }
        #endregion

        #region Constructors
        public DataCreatorResourceSystemException( string message, Type requestedType ) : this( message, requestedType, null )
        {
        }

        public DataCreatorResourceSystemException( string message, Type requestedType, Exception innerException ) : base( message, innerException )
        {
            RequestedType = requestedType;
        }
        #endregion
    }
}
#endif