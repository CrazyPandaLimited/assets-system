#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class ByteArrayDataCreator : IResourceDataCreator
    {
        public bool Supports(Type requestedResourceType)
        {
            return requestedResourceType == typeof(byte[]);
        }

        public TResourceType Create<TResourceType>(byte[] data) where TResourceType : class
        {
            return data as TResourceType;
        }
        public void Destroy(object resource)
        {
            
        }
    }
}
#endif