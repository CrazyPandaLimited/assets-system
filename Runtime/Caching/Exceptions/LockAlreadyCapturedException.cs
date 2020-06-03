namespace CrazyPanda.UnityCore.AssetsSystem
{
    class LockAlreadyCapturedException : AssetsSystemException
    {
        #region Properties
        public string LockKey { get; }
        #endregion

        #region Constructors
        public LockAlreadyCapturedException( string lockKey, FileLocksManager.LockReason requestedLock, FileLocksManager.LockReason existingLock )
            : base( $"Cannot lock '{lockKey}' for reason {requestedLock}. Lock {existingLock} already exists" )
        {
            LockKey = lockKey;
        }
        #endregion
    }
}
