#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public partial class CoroutineManager : ICoroutineManager
	{
		#region Private Fields
		private LinkedList< Entry > _coroutines;
		private ITimeProvider _timeProvider;
		#endregion

		#region Properties
		public ITimeProvider TimeProvider
		{
			get { return _timeProvider; }
			set
			{
				if( _timeProvider != null )
				{
					_timeProvider.OnUpdate -= HandleFrameUpdate;
				}

				_timeProvider = value;

				if( _timeProvider != null )
				{
					_timeProvider.OnUpdate += HandleFrameUpdate;
				}
			}
		}
		#endregion

		#region Events
		public event Action< object, Exception > OnError;
		#endregion

		#region Constructors
		public CoroutineManager()
		{
#if UNITY_EDITOR
			_instance = this;
#endif
			_coroutines = new LinkedList< Entry >();
		}
		#endregion

		#region Public Members
		public ICoroutineProcessorPausable StartCoroutine( object target, IEnumerator enumerator, Action< object, Exception > handlerError = null, bool forcePutFirst = false )
		{
			if( _timeProvider == null )
			{
				throw new NullReferenceException( "TimeProvider" );
			}

			if( target == null )
			{
				throw new NullReferenceException( "parameter 'target'" );
			}

			var extendedCoroutine = CreateEnumeratorCoroutine( enumerator );
			var entry = new Entry( target, enumerator, extendedCoroutine, handlerError );
			if( forcePutFirst )
			{
				_coroutines.AddFirst( entry );
			}
			else
			{
				_coroutines.AddLast( entry );
			}
			return extendedCoroutine;
		}

		public ICoroutineProcessorPausable StartCoroutineBefore( object target, IEnumerator enumerator, ICoroutineProcessor before, Action< object, Exception > handlerError = null )
		{
			if( _timeProvider == null )
			{
				throw new NullReferenceException( "TimeProvider" );
			}

			if( target == null )
			{
				throw new NullReferenceException( "parameter 'target'" );
			}

			var extendedCoroutine = CreateEnumeratorCoroutine( enumerator );

			LinkedListNode< Entry > beforeNode = null;
			foreach( var entry in _coroutines )
			{
				if( entry.CoroutineProcessor == before )
				{
					beforeNode = _coroutines.Find( entry );
					break;
				}
			}

			if( beforeNode == null )
			{
				throw new ArgumentException( "There's no such element in coroutines list", "before" );
			}

			_coroutines.AddBefore( beforeNode, new Entry( target, enumerator, extendedCoroutine, handlerError ) );
			return extendedCoroutine;
		}

		public ICoroutineProcessorPausable CreateProcessor( IEnumerator enumerator )
		{
			return CreateEnumeratorCoroutine( enumerator );
		}

		public void StartProcessorImmediate( object target, ICoroutineProcessor processor, Action< object, Exception > handlerError = null )
		{
			try
			{
				while( !processor.IsCompleted )
				{
					processor.Update();
				}
			}
			catch( Exception exception )
			{
				if( handlerError != null )
				{
					handlerError( target, exception );
					return;
				}

				throw;
			}
		}

		public void StopAllCoroutinesForTarget( object target )
		{
			if( target == null )
			{
				throw new NullReferenceException();
			}

			foreach( var coroutine in _coroutines.Where( c => c.Target == target ) )
			{
				coroutine.CoroutineProcessor.Stop();
			}
		}


		public void StopAllCoroutines()
		{
			foreach( var coroutine in _coroutines )
			{
				coroutine.CoroutineProcessor.Stop();
			}
		}
		#endregion

		#region Protected Members
		
		public void Dispose()
		{
			TimeProvider = null;
			OnError = null;
			StopAllCoroutines();
		}
		#endregion

		#region Private Members
		private EnumeratorCoroutineProcessor CreateEnumeratorCoroutine( IEnumerator enumerator )
		{
			if( enumerator == null )
			{
				throw new NullReferenceException( "parameter 'enumerator'" );
			}

			if( _coroutines.Any( v => v.Enumerator == enumerator ) )
			{
				throw new DuplicateEnumeratorException( enumerator );
			}

			return new EnumeratorCoroutineProcessor( _timeProvider, enumerator );
		}

		private void HandleFrameUpdate()
		{
			var currentNode = _coroutines.First;
			while( currentNode != null )
			{
				var entry = currentNode.Value;

				// Check if entry complete before update, cos it could be stopped manually from outer code
				if( entry.CoroutineProcessor.IsCompleted )
				{
					currentNode = RemoveEntry( currentNode );
					continue;
				}

				if( !entry.IsAlive ) // GameObject destroyed OR someone forgot to unsubscribe
				{
					entry.CoroutineProcessor.Stop();
				}
				else
				{
					UpdateEntry( entry );
				}

				// Check if entry complete after update, cos it could be the last one and we need to get finish event in this frame, not next
				currentNode = entry.CoroutineProcessor.IsCompleted ? RemoveEntry( currentNode ) : currentNode.Next;
			}
		}

		private LinkedListNode< Entry > RemoveEntry( LinkedListNode< Entry > currentNode )
		{
			var nodeToRemove = currentNode;
			currentNode = nodeToRemove.Next;
			_coroutines.Remove( nodeToRemove );
			return currentNode;
		}

		private void UpdateEntry( Entry entry )
		{
			try
			{
				entry.CoroutineProcessor.Update();
			}
			catch( Exception ex )
			{
				entry.CoroutineProcessor.Stop();
				entry.CoroutineProcessor.Exception = ex;

				if( entry.HandlerError != null )
				{
					entry.HandlerError.Invoke( entry.Target, ex );
				}
				else if( OnError != null )
				{
					OnError.Invoke( entry.Target, ex );
				}
				else
				{
					throw;
				}
			}
		}
		#endregion

#if UNITY_EDITOR

		// нужен инстанс, чтобы отобразить редактор
		public static CoroutineManager Instance { get { return _instance; } }

		private static CoroutineManager _instance;

		public LinkedList< Entry > Coroutines { get { return _coroutines; } }
#endif
	}
}

#endif
