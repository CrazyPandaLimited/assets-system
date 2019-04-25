using System;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public interface ICoroutineProcessor
	{
		#region Properties
		bool IsCompleted { get; }
		Exception Exception { get; }
		#endregion

		#region Public Members
		event Action< ICoroutineProcessor > OnComplete;
		void Update();
		void Stop();
		#endregion
	}

	public interface ICoroutineProcessorPausable : ICoroutineProcessor
	{
		#region Public Members
		void Pause();
		void Resume();
		#endregion
	}
}
