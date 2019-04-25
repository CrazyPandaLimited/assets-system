#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public interface IResourceDataCreator
    {
        #region Public Members
        bool Supports( Type requestedResourceType );
        //object Create( byte[ ] data );
        TResourceType Create<TResourceType>(byte[] data) where TResourceType : class;
	    void Destroy( object resource );
	    #endregion
    }
}
#endif