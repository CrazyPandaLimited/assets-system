#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class WebRequestLoaderException : ResourceSystemException
	{
		public WebRequestLoaderException(string message) : base( message )
		{
		}

		public WebRequestLoaderException(string message, Exception innerException) : base( message, innerException )
		{
		}
	}
}
#endif