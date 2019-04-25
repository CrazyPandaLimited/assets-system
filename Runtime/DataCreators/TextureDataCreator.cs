#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class TextureDataCreator : IResourceDataCreator
    {
        public bool Supports(Type requestedResourceType)
        {
            return requestedResourceType == typeof(Texture) || requestedResourceType == typeof(Texture2D);
        }

        #region Public Members
        
        public TResourceType Create<TResourceType>(byte[] data) where TResourceType : class
        {
            var result = new Texture2D(2, 2);
            result.LoadImage(data);
            return result as TResourceType;
        }

        public void Destroy(object resource)
        {
            var texture = resource as Texture2D;
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Object.DestroyImmediate(texture);
                return;
            }
#endif
            Object.Destroy(texture);
        }

        #endregion
    }
}

#endif