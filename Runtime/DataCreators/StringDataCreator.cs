#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM

using System;
using System.Text;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class StringDataCreator : IResourceDataCreator
	{
		public bool Supports(Type requestedResourceType)
		{
			return requestedResourceType == typeof(String);
		}

		public TResourceType Create<TResourceType>(byte[] data) where TResourceType : class
		{
			return Encoding.UTF8.GetString( data ) as TResourceType;
		}
		
		public void Destroy( object resource )
		{
		}
	}
}

#endif