#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class UnityResourceFromBundleLoaderException : ResourceSystemException
	{
		#region Constructors
		public UnityResourceFromBundleLoaderException( string message ) : base( message )
		{
		}

		public UnityResourceFromBundleLoaderException( string message, Exception innerException ) : base( message, innerException )
		{
		}
		#endregion
	}
}
#endif