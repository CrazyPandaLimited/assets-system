#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections;
using System.IO;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityEngine;


namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class LocalFolderBundleWorker : BaseRequestWorker
	{
		#region Private Fields
		private BundleInfo _bundleInfo;

		private string localFolder;
		private Action< LocalFolderBundleWorker > _onLoadingComplete;
		#endregion

		#region Properties
		public AssetBundle AssetBundle { get; private set; }

		public override bool IsWaitDependentResource
		{
			get
			{
				return false;
			}
		}
		#endregion

		#region Constructors
		public LocalFolderBundleWorker( string localFolder, string uri, BundleInfo bundleInfo,
										Action< LocalFolderBundleWorker > onLoadingComplete, ICoroutineManager coroutineManager) :
			base( uri, coroutineManager )
		{
			this.localFolder = localFolder;
			_bundleInfo = bundleInfo;
			_onLoadingComplete = onLoadingComplete;
		}
		#endregion

		#region Public Members
		public override void Dispose()
		{
			base.Dispose();
			_bundleInfo = null;
			AssetBundle = null;
			_onLoadingComplete = null;
		}
		#endregion

		#region Protected Members
		protected override void FireComplete()
		{
			_onLoadingComplete( this );
		}

		protected override IEnumerator LoadProcess()
		{
			AssetBundleCreateRequest loadingAsyncOperation =
				AssetBundle.LoadFromFileAsync( Path.Combine( localFolder, _bundleInfo.Name ) );
			yield return UpdateLoadingOperations( loadingAsyncOperation );
			AssetBundle = loadingAsyncOperation.assetBundle;
			FireComplete();
		}

		protected override void InnerCancelRequest()
		{
		}
		#endregion
	}
}
#endif