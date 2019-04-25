#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class UnityResourceFolderLoaderException : ResourceSystemException
	{
		#region Constructors
		public UnityResourceFolderLoaderException( string message ) : base( message )
		{
		}

		public UnityResourceFolderLoaderException( string message, Exception innerException ) : base( message, innerException )
		{
		}
		#endregion
	}
}
#endif