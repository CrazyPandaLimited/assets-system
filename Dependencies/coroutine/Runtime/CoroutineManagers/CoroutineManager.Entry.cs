#if CRAZYPANDA_UNITYCORE_COROUTINE

using System;
using System.Collections;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public partial class CoroutineManager
	{
		public class Entry
		{
			#region Private Fields
			private WeakReference _targetReference;
			private bool _isUnityObject;
			#endregion

			#region Properties
			public object Target { get { return _targetReference.Target; } }

			public bool IsAlive
			{
				get
				{
					var alive = _targetReference.IsAlive;
					if( !alive || !_isUnityObject )
					{
						return alive;
					}
					var unityObj = _targetReference.Target as Object;
					if( unityObj == null )
					{
						return false;
					}
					return true;
				}
			}

			public Action< object, Exception > HandlerError { get; private set; }
			public IEnumerator Enumerator { get; private set; }
			public EnumeratorCoroutineProcessor CoroutineProcessor { get; private set; }
			#endregion

			#region Constructors
			public Entry( object target, IEnumerator enumerator, EnumeratorCoroutineProcessor coroutineProcessor, Action< object, Exception > handlerError )
			{
				_isUnityObject = target is Object;
				_targetReference = new WeakReference( target );
				Enumerator = enumerator;
				CoroutineProcessor = coroutineProcessor;
				HandlerError = handlerError;
			}
			#endregion
		}
	}
}

#endif