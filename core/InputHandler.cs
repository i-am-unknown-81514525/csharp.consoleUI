using System;
using System.Collections.Generic;
using System.Linq;
using ui;

namespace ui.core
{
    public abstract class InputHandler
    {
        private LockStatus _prevLockStatus = LockStatus.NoLock;
        private LockStatus _lockStatus = LockStatus.NoLock;
        internal List<byte> Buffer = new List<byte>(); // buffer should only be stored when a lock is acquired
        private bool _allowModifyStatus = false;

        public InputHandler() {
            Buffer = new List<byte>();
        }

        public LockStatus GetLockStatus() => _lockStatus;
        internal LockStatus GetPrevLockStatus() => _prevLockStatus;

        private void ResetBuffer()
        {
            Buffer = new List<byte>();
        }

        public bool DropUnused()
        {
            if (_lockStatus == LockStatus.NoLock)
            {
                ResetBuffer();
                return true;
            }
            return false;
        }

        public void AddBuffer(byte value)
        {
            Buffer.Add(value);
        }

        public bool Reset(SharedLock lockStatus)
        {
            if (lockStatus == null || lockStatus.GetLockCount() == 0)
            {
                if (_lockStatus != LockStatus.NoLock)
                {
                    throw new LockConflictException("Invalid lock status: the handler held a lock when parameter claim no lock");
                }
                return false; // No lock so no reset
            }
            if (_lockStatus == LockStatus.NoLock)
            {
                ResetBuffer(); // Someone held lock and it is not itself
                return true;
            }
            else if (_lockStatus == LockStatus.SharedLock && !lockStatus.GetIsShared())
            {
                ResetBuffer();
                _lockStatus = LockStatus.NoLock;
                return true; // Itself held shared lock but someone took exclusive access
                             // Therefore the code should volunterarily give up access
            }
            else if (_lockStatus == LockStatus.SharedLock) //  implies: && lockStatus.GetIsShared()
            {
                if (lockStatus.GetLockedHandler().Contains(this))
                {
                    return false; // Part of shared lock
                }
                throw new LockConflictException("Invalid lock status: the handler held an shared lock when parameter claim the handler don't held the lock");
            }
            else if (_lockStatus == LockStatus.ExclusiveLock && lockStatus.GetIsShared())
            {
                throw new LockConflictException("Invalid lock status: the handler held an exclsuive lock when parameter claim only shared lock");
            }
            else if (_lockStatus == LockStatus.ExclusiveLock) //  implies:  && !lockStatus.GetIsShared()
            {
                if (lockStatus.GetLockedHandler().Contains(this))
                {
                    if (lockStatus.GetLockedHandler().Length != 1)
                        throw new LockConflictException("Invalid lock status: Multiple handler held exclusive lock");
                    return false;
                }
                ResetBuffer();
                _lockStatus = LockStatus.NoLock; // Exclusive lock held but handler at prioity take the exclusive lock first
                return true;
            }
            throw new NotImplementedException("Unexpected case");
        }

        internal abstract LockStatus Validate();

        public LockStatus ValidateExternal()
        {
            LockStatus status;
            try
            {
                status = Validate();
            }
            catch (Exception)
            {
                if (!DEBUG.InputHandler_IgnoreHanlderValidateException) throw;
                #pragma warning disable CS0162 // Unreachable code detected
                return LockStatus.NoLock;
                #pragma warning restore CS0162 // Unreachable code detected
            }
            _lockStatus = status;
            return status;
        }

        internal void SetLockStatus(LockStatus status)
        {
            if (!_allowModifyStatus)
            {
                throw new LockUnpermitChangeException();
            }
            _prevLockStatus = _lockStatus;
            _lockStatus = status;
        }

        internal abstract void Handle(RootInputHandler root);

        public void HandleExternal(RootInputHandler root)
        {
            try
            {
                _allowModifyStatus = true;
                Handle(root);
            }
            catch (Exception)
            {
                if (_lockStatus != LockStatus.NoLock)
                {
                    _lockStatus = LockStatus.NoLock;
                    root.LockChangeAnnounce(this);
                }
                if (!DEBUG.InputHandler_IgnoreHandlerHandleException) throw;
            }
            finally
            {
                _allowModifyStatus = false;
            }
        }
    }

    
}