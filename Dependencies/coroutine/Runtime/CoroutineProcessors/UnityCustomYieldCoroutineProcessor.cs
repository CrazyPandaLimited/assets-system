#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;
using UnityEngine;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
    public class UnityCustomYieldCoroutineProcessor : ICoroutineProcessor
    {
	    public Exception Exception { get; set; }
		#region Private Fields
		private CustomYieldInstruction _customInstruction;
	    private bool _isComplete;
        #endregion

        #region Properties
        public bool IsCompleted
        {
	        get
	        {
		        if( _isComplete == _customInstruction.keepWaiting )
		        {
			        _isComplete = !_customInstruction.keepWaiting;
			        if( _isComplete && OnComplete != null )
			        {
				        OnComplete( this );
			        }
		        }
		        return !_customInstruction.keepWaiting;
	        }
        }

	    public event Action< ICoroutineProcessor > OnComplete;
	    #endregion

        #region Constructors
        public UnityCustomYieldCoroutineProcessor( CustomYieldInstruction customInstruction )
        {
            _customInstruction = customInstruction;
	        _isComplete = !_customInstruction.keepWaiting;
			if (_isComplete && OnComplete != null)
			{
				OnComplete(this);
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
