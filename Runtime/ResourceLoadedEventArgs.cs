#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class ResourceLoadedEventArgs< T > : EventArgs where T : class
	{
		#region Properties
		public string Uri { get; private set; }
		public T Resource { get; private set; }
		public Exception Error { get; private set; }
		public bool IsCanceled { get; private set; }
		#endregion

		#region Constructors
		public ResourceLoadedEventArgs( string uri, T result, Exception exception, bool isCanceled = false )
		{
			Uri = uri;
			Resource = result;
			Error = exception;
			IsCanceled = isCanceled;
		}
		#endregion

		#region Public Members
		public static ResourceLoadedEventArgs< T > CompletedSuccess( string uri, T result )
		{
			return new ResourceLoadedEventArgs< T >( uri, result, null, false );
		}
		
		public static ResourceLoadedEventArgs< T > CompletedError( string uri, Exception error )
		{
			return new ResourceLoadedEventArgs< T >( uri, null, error, false );
		}
		
		public static ResourceLoadedEventArgs< T > Canceled( string uri )
		{
			return new ResourceLoadedEventArgs< T >( uri, null, null, true );
		}
		#endregion
	}
}
#endif