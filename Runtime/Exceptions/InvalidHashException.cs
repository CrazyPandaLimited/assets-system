#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class InvalidHashException : FileCachingException
	{
		public InvalidHashException( string message ) : base( message )
		{
		}
	}
}
#endif