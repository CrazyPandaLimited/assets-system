#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class CachedFileNotFoundException : FileCachingException
	{
		public CachedFileNotFoundException( string message ) : base( message )
		{
		}
	}
}
#endif