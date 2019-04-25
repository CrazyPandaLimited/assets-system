using System;
using System.Collections;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public interface ICoroutineManager
	{
		#region Properties
		ITimeProvider TimeProvider { get; set; }
		#endregion

		#region Events
		event Action< object, Exception > OnError;
		#endregion

		#region Public Members
		ICoroutineProcessorPausable StartCoroutine( object target, IEnumerator enumerator, Action< object, Exception > handlerError = null, bool forcePutFirst = false );
		ICoroutineProcessorPausable StartCoroutineBefore( object target, IEnumerator enumerator, ICoroutineProcessor before, Action< object, Exception > handlerError = null );
		ICoroutineProcessorPausable CreateProcessor( IEnumerator enumerator );
		void StartProcessorImmediate( object target, ICoroutineProcessor processor, Action< object, Exception > handlerError = null );

		void StopAllCoroutinesForTarget( object target );
		void StopAllCoroutines();
		#endregion
	}
}
