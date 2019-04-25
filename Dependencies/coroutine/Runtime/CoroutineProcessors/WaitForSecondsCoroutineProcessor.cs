#if CRAZYPANDA_UNITYCORE_COROUTINE

using System;


namespace CrazyPanda.UnityCore.CoroutineSystem
{
    class WaitForSecondsCoroutineProcessor : ICoroutineProcessor
    {
	    public Exception Exception { get; set; }
		#region Private Fields
		private double _currentTimer;
        private ITimeProvider _timeProvider;
        #endregion

        #region Properties
        public bool IsCompleted
        {
	        get
	        {
		        bool isComplete = _currentTimer <= 0;
		        if( isComplete && OnComplete != null )
		        {
			        OnComplete( this );
		        }
		        return _currentTimer <= 0;
	        }
        }

	    public event Action< ICoroutineProcessor > OnComplete;
	    #endregion

        #region Constructors
        public WaitForSecondsCoroutineProcessor( ITimeProvider timeProvider, double timer )
        {
            _timeProvider = timeProvider;
            _currentTimer = timer;
        }
        #endregion

        #region Public Members
        public void Update()
        {
            _currentTimer -= _timeProvider.deltaTime;
        }

        public void Stop()
        {
        }
        #endregion
    }
}

#endif
