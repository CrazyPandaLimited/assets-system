#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using CrazyPanda.UnityCore.Serialization;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class JsonDataCreator: IResourceDataCreator
    {
        private ISerializer _serializer;


        public JsonDataCreator(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public bool Supports(Type requestedResourceType)
        {
            return requestedResourceType == typeof(IJsonResource);
        }

        public TResourceType Create<TResourceType>(byte[] data) where TResourceType:class
        {
            return _serializer.Deserialize<TResourceType>(data);
        }
        
        public void Destroy( object resource )
        {
        }
    }
}
#endif