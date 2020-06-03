namespace CrazyPanda.UnityCore.AssetsSystem
{
    class LockNotFoundException : AssetsSystemException
    {
        #region Properties
        public string LockKey { get; }
        #endregion

        #region Constructors
        public LockNotFoundException( string lockKey, FileLocksManager.LockReason requestedReason )
            : base( $"Cannot remove lock {lockKey} with reason {requestedReason} because this lock was not set" )
        {
            LockKey = lockKey;
        }
        #endregion
    }
}
