#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class FileCachingException : Exception
	{
		public FileCachingException( string message ) : base( message )
		{
		}

		public FileCachingException( string message, Exception innerException ) : base( message, innerException )
		{
		}
	}

}
#endif