#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    class URIResourceSystemException : ResourceSystemException
    {
        #region Properties
        public string URI { get; protected set; }
        #endregion

        #region Constructors
        public URIResourceSystemException( string message, string uri ) : this( message, uri, null )
        {
        }

        public URIResourceSystemException( string message, string uri, Exception innerException ) : base( message, innerException )
        {
            URI = uri;
        }
        #endregion
    }
}
#endif