#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using CrazyPanda.UnityCore.CoroutineSystem;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public interface ILoadingOperation< T > where T : class
	{
		string Uri { get; }
		T Resource { get; }
		float Progress { get; }
		bool IsCompleted { get; }

		Action<ResourceLoadedEventArgs<T>> OnResourceLoaded { get; set; }
		Action<ResourceLoadingProgressEventArgs> OnLoadStep{ get; set; }
		
		bool IsCanceled { get; }
		Exception Error { get; }
		void CancelLoading();
	}

	public interface IResourceSystemLoadingOperation
	{
		object Owner { get; }
		void LoadingProgressChange( ResourceLoadingProgressEventArgs args );
		void LoadingCanceled();
	}
}
#endif