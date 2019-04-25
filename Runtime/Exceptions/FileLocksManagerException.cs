#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class FileLocksManagerException : Exception
	{
		#region Constructors
		public FileLocksManagerException( string message ) : base( message )
		{
		}
		#endregion
	}
}
#endif