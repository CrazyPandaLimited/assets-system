#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public class MonoBehaviourTimeProvider : MonoBehaviour, ITimeProvider
	{
		#region Constants
		protected const string Name = "-MonoBehaviourTimeProvider";
		#endregion

		#region Private Fields
		private static MonoBehaviourTimeProvider _instance;
		#endregion

		#region Properties
		public static MonoBehaviourTimeProvider Instance
		{
			get
			{
				if( _instance == null )
				{
					_instance = FindObjectOfType< MonoBehaviourTimeProvider >();

					if( _instance == null )
					{
						var go = new GameObject( Name );
						DontDestroyOnLoad( go );
						_instance = go.AddComponent< MonoBehaviourTimeProvider >();
					}
				}
				return _instance;
			}
		}

		public float deltaTime
		{
			get { return Time.deltaTime; }
		}
		#endregion

		#region Events
		public event Action< object, Exception > OnError;
		public event Action OnUpdate;
		#endregion

		#region Public Members
		public void Update()
		{
			try
			{
				if( OnUpdate != null )
				{
					OnUpdate();
				}
			}
			catch( Exception exception )
			{
				if( OnError != null )
				{
					OnError.Invoke( this, exception );
				}
				else
				{
					Debug.LogException( exception );
				}
			}
		}
		#endregion
	}
}

#endif
