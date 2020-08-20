using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    class FileLocksManager
    {
        #region Private Fields
        private Dictionary< string, LockReason > _locks;
        #endregion

        #region Constructors
        public FileLocksManager()
        {
            _locks = new Dictionary< string, LockReason >();
        }
        #endregion

        #region Public Members
        public LockReason GetLockReason( string key )
        {
            LockReason lockReason;
            _locks.TryGetValue( key, out lockReason );
            return lockReason;
        }

        public bool HasAnyLock( string key )
        {
            return GetLockReason( key ) != LockReason.None;
        }

        public bool HasLock( string key, LockReason check )
        {
            var lockReason = GetLockReason( key );
            return InternalHasLock( lockReason, check );
        }

        public void AddReadLock( string key )
        {
            var lockReason = GetLockReason( key );

            if( InternalHasLock( lockReason, LockReason.Read ) )
            {
                throw new LockAlreadyCapturedException( key, LockReason.Read, LockReason.Read );
            }

            if( InternalHasLock( lockReason, LockReason.Write ) )
            {
                throw new LockAlreadyCapturedException( key, LockReason.Read, LockReason.Write );
            }

            _locks[ key ] = lockReason | LockReason.Read;
        }


        public void RemoveReadLock( string key )
        {
            var lockReason = GetLockReason( key );

            if( !InternalHasLock( lockReason, LockReason.Read ) )
            {
                throw new LockNotFoundException( key, LockReason.Read );
            }

            _locks[ key ] = lockReason & ~LockReason.Read;
        }

        public void AddWriteLock( string key )
        {
            var lockReason = GetLockReason( key );

            if( InternalHasLock( lockReason, LockReason.Write ) )
            {
                throw new LockAlreadyCapturedException( key, LockReason.Write, LockReason.Write );
            }

            if( InternalHasLock( lockReason, LockReason.Read ) )
            {
                throw new LockAlreadyCapturedException( key, LockReason.Write, LockReason.Read );
            }

            if( InternalHasLock( lockReason, LockReason.ManualLock ) )
            {
                throw new LockAlreadyCapturedException( key, LockReason.Write, LockReason.ManualLock );
            }

            _locks[ key ] = lockReason | LockReason.Write;
        }

        public void RemoveWriteLock( string key )
        {
            var lockReason = GetLockReason( key );

            if( !InternalHasLock( lockReason, LockReason.Write ) )
            {
                throw new LockNotFoundException( key, LockReason.Write );
            }

            _locks[ key ] = lockReason & ~LockReason.Write;
        }

        public void AddManualLock( string key )
        {
            var lockReason = GetLockReason( key );
            _locks[ key ] = lockReason | LockReason.ManualLock;
        }

        public void RemoveManualLock( string key )
        {
            var lockReason = GetLockReason( key );
            _locks[ key ] = lockReason & ~LockReason.ManualLock;
        }
        #endregion

        #region Private Members
        private static bool InternalHasLock( LockReason lockReason, LockReason check )
        {
            return ( lockReason & check ) != 0;
        }
        #endregion

        #region Nested Types
        [ Flags ]
        public enum LockReason
        {
            None,
            Read = 1 << 0,
            Write = 1 << 1,
            ManualLock = 1 << 2
        }
        #endregion
    }
}
