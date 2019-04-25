#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public interface IResourceWorker
	{
		#region Properties
		List< IResourceSystemLoadingOperation > LoadingOperations { get; }

		Exception Error { get; }
		bool IsLoadingCanceled { get; }
		bool IsWaitDependentResource { get; }

		string Uri { get; }
		#endregion

		#region Public Members
		void RegisterLoadingOperation( IResourceSystemLoadingOperation loadingOperation );
		void UnregisterLoadingOperation( IResourceSystemLoadingOperation loadingOperation );
		
		void RemoveLoadingOperation( Object owner );
		IResourceSystemLoadingOperation GetLoadingOperationByOwner( Object owner );

		void StartLoading();
		void CancelLoading();

		void Dispose();
		#endregion
	}
}
#endif