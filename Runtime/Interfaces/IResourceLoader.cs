#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public interface IResourceLoader
	{	
		void OnRegisteredToResourceStorage(ResourceStorage resourceStorege, WorkersQueue workersQueue, Action<Exception> onResourceLoadError);
		
		bool CanLoadResourceImmediately< T >( object owner, string uri ) where T : Object;
		T LoadResourceImmediately< T >( object owner, string uri ) where T : Object;

		ILoadingOperation< T > CreateLoadingWorkerAsync< T >( object owner, string uri ) where T : class;
		bool IsCached( string uri );

        Dictionary<string, object> ReleaseAllFromCache(bool destroy = true);
        Dictionary<string, object> ReleaseFromCache( object owner, string uri , bool destroy = true);

        Dictionary<string, object> ForceReleaseFromCache(string uri , bool destroy = true);
        Dictionary<string, object> ReleaseAllOwnerResourcesFromCache( object owner, bool destroy = true );
        Dictionary<string, object> ReleaseUnusedFromCache(bool destroy = true);
		void DestroyResource(string uri, object resource );
		
		string SupportsMask { get; }
	}

}
#endif