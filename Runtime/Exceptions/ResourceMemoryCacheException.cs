#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class ResourceMemoryCacheException : Exception
	{
		#region Constructors
		public ResourceMemoryCacheException( string message ) : base( message )
		{
		}

		public ResourceMemoryCacheException( string message, Exception innerException ) : base( message, innerException )
		{
		}
		#endregion
	}
}
#endif