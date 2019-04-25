#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public class AsyncOperationProcessor : ICoroutineProcessor
	{
		#region Private Fields
		private readonly AsyncOperation _asyncOperation;
		private bool _isComplete;
		#endregion

		#region Properties
		public bool IsCompleted
		{
			get
			{
				if( _isComplete != _asyncOperation.isDone )
				{
					_isComplete = _asyncOperation.isDone;
					if( _isComplete && OnComplete != null )
					{
						OnComplete( this );
					}
				}
				return _isComplete;
			}
		}
		public Exception Exception { get; set; }
		#endregion

		#region Events
		public event Action< ICoroutineProcessor > OnComplete;
		#endregion

		#region Constructors
		public AsyncOperationProcessor( AsyncOperation asyncOperation )
		{
			_asyncOperation = asyncOperation;
			_isComplete = _asyncOperation.isDone;
			if( _isComplete && OnComplete != null )
			{
				OnComplete( this );
			}
		}
		#endregion

		#region Public Members
		public void Update()
		{
		}

		public void Stop()
		{
		}
		#endregion
	}
}

#endif
