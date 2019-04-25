#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.Collections.Generic;
using CrazyPanda.UnityCore.CoroutineSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class UnityResourceFromBundleWorkerEditorLocal : BaseRequestWorker
	{
		#region Private Fields
		private Action< UnityResourceFromBundleWorkerEditorLocal > _onLoadingComplete;
		private AssetBundleRequest _loadingAsyncOperation;
		#endregion

		#region Properties
		public Object LoadedUnityObject { get; private set; }

		public override bool IsWaitDependentResource
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Constructors
		public UnityResourceFromBundleWorkerEditorLocal(
			string uri, Action< UnityResourceFromBundleWorkerEditorLocal > onLoadingComplete,
			ICoroutineManager coroutineManager ) : base( uri, coroutineManager )
		{
			_onLoadingComplete = onLoadingComplete;
		}
		#endregion

		#region Public Members
		public void RegisterMainDependency< T >( ILoadingOperation< T > dependendentLoader ) where T : class
		{
		}

		public void RegisterSecondDependency< T >( ILoadingOperation< T > dependendentLoader ) where T : class
		{
		}

		public override void Dispose()
		{
			base.Dispose();
			LoadedUnityObject = null;
		}
		#endregion

		#region Protected Members
		protected override void FireComplete()
		{
			_onLoadingComplete( this );
		}

		protected override IEnumerator LoadProcess()
		{
			var resourceName = UrlHelper.GetResourceName( Uri );
			Debug.Log( "Try get resource from bundle " + resourceName );
#if UNITY_EDITOR
			LoadedUnityObject = AssetDatabase.LoadAssetAtPath< Object >( resourceName );
#endif
			FireComplete();
			yield return null;
		}

		protected override void InnerCancelRequest()
		{
		}
		#endregion
	}
}
#endif