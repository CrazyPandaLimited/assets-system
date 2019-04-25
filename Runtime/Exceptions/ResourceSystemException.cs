#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ResourceSystemException : Exception
    {
        #region Constructors
        public ResourceSystemException( string message ) : base( message )
        {
        }

        public ResourceSystemException( string message, Exception innerException ) : base( message, innerException )
        {
        }
        #endregion
    }
}
#endif