using System;
using System.Collections.Generic;
using System.Linq;

namespace ui.core
{
    public static class InputConst
    {
        public const bool IgnoreHanlderValidateException = false;
        public const bool IgnoreHandlerHandleException = false;
    }

    public class LockConflictException : InvalidOperationException
    {
        public LockConflictException() { }
        public LockConflictException(string content) : base(content) { }
    }

    public class LockUnpermitChangeException : InvalidOperationException
    {
        public LockUnpermitChangeException() { }
        public LockUnpermitChangeException(string content) : base(content) { }
    }

    public class SharedLock
    // When many handler consider the data to be significant but not sure
    // (only shared lock would receive the info unless a handler take exclusive lock or all lock handler release their lock)
    {
        private bool _isShared = true;
        private List<InputHandler> _lockHandler = new List<InputHandler>();

        public SharedLock(IEnumerable<InputHandler> handlers = null)
        {
            if (handlers == null)
                return;
            foreach (InputHandler handler in handlers)
            {
                this.AddMember(handler);
            }
        }

        public SharedLock(InputHandler handler, bool isExclusive = false)
        {
            _lockHandler.Add(handler);
            this._isShared = !isExclusive;
        }

        public bool GetIsShared() => this._isShared;

        public bool AddMember(InputHandler handler)
        {
            if (!this.GetIsShared())
            {
                return false;
            }
            if (!_lockHandler.Contains(handler))
                _lockHandler.Add(handler);
            return true;
        }

        public bool DropMember(InputHandler handler)
        {
            int count = _lockHandler.RemoveAll(x => x == handler);
            if (this.GetLockCount() == 0)
            {
                this._isShared = true;
            }
            return count != 0;
        }

        public InputHandler[] GetLockedHandler() => _lockHandler.ToArray();

        public int GetLockCount() => this.GetLockedHandler().Length;
    }

    public enum LockStatus : int
    {
        NoLock = 0,
        SharedLock = 1,
        ExclusiveLock = 2
    }

    public abstract class InputHandler
    {
        private LockStatus _lockStatus = LockStatus.NoLock;
        internal List<byte> Buffer = new List<byte>(); // buffer should only be stored when a lock is acquired
        private bool _allowModifyStatus = false;

        public InputHandler() {
            this.Buffer = new List<byte>();
        }

        public LockStatus GetLockStatus() => this._lockStatus;

        private void ResetBuffer()
        {
            Buffer = new List<byte>();
        }

        public bool DropUnused()
        {
            if (this._lockStatus == LockStatus.NoLock)
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
                if (this._lockStatus != LockStatus.NoLock)
                {
                    throw new LockConflictException("Invalid lock status: the handler held a lock when parameter claim no lock");
                }
                return false; // No lock so no reset
            }
            if (this._lockStatus == LockStatus.NoLock)
            {
                ResetBuffer(); // Someone held lock and it is not itself
                return true;
            }
            else if (this._lockStatus == LockStatus.SharedLock && !lockStatus.GetIsShared())
            {
                ResetBuffer();
                this._lockStatus = LockStatus.NoLock;
                return true; // Itself held shared lock but someone took exclusive access
                             // Therefore the code should volunterarily give up access
            }
            else if (this._lockStatus == LockStatus.SharedLock) //  implies: && lockStatus.GetIsShared()
            {
                if (lockStatus.GetLockedHandler().Contains(this))
                {
                    return false; // Part of shared lock
                }
                throw new LockConflictException("Invalid lock status: the handler held an shared lock when parameter claim the handler don't held the lock");
            }
            else if (this._lockStatus == LockStatus.ExclusiveLock && lockStatus.GetIsShared())
            {
                throw new LockConflictException("Invalid lock status: the handler held an exclsuive lock when parameter claim only shared lock");
            }
            else if (this._lockStatus == LockStatus.ExclusiveLock) //  implies:  && !lockStatus.GetIsShared()
            {
                if (lockStatus.GetLockedHandler().Contains(this))
                {
                    if (lockStatus.GetLockedHandler().Length != 1)
                        throw new LockConflictException("Invalid lock status: Multiple handler held exclusive lock");
                    return false;
                }
                ResetBuffer();
                this._lockStatus = LockStatus.NoLock; // Exclusive lock held but handler at prioity take the exclusive lock first
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
                status = this.Validate();
            }
            catch (Exception e)
            {
                if (!InputConst.IgnoreHanlderValidateException) throw;
                return LockStatus.NoLock;
            }
            this._lockStatus = status;
            return status;
        }

        internal void SetLockStatus(LockStatus status)
        {
            if (!this._allowModifyStatus)
            {
                throw new LockUnpermitChangeException();
            }
            this._lockStatus = status;
        }

        internal abstract void Handle(RootInputHandler root);

        public void HandleExternal(RootInputHandler root)
        {
            try
            {
                this._allowModifyStatus = true;
                this.Handle(root);
            }
            catch (Exception e)
            {
                if (this._lockStatus != LockStatus.NoLock)
                {
                    this._lockStatus = LockStatus.NoLock;
                    root.LockChangeAnnounce(this);
                }
                this._allowModifyStatus = false;
                if (!InputConst.IgnoreHandlerHandleException) throw;
            }
            this._allowModifyStatus = false;
        }
    }

    public class RootInputHandler
    // This code assuming that the code call from abstract function `Validate` and `Handle` in `InputHandler`
    // might have malicious intend, therefore it should attempt to defense against it.
    // The cause of an exception is not consider malicious and thus not required to be defense against
    // TODO: Add method to set prioity for InputHandler, which would rearrange the list
    {
        private List<InputHandler> _handlers = new List<InputHandler>();
        private bool _hasLockChange = false;
        private bool _recursivePreventLock = false; // Prevent iner nhandler to call dispatch whcih might cause recurrsion
        private SharedLock _lockStatus = new SharedLock();

        public RootInputHandler(IEnumerable<InputHandler> handlers = null)
        {
            if (handlers != null)
            {
                this._handlers = handlers.ToList();
            }
        }

        public void Add(InputHandler handler)
        {
            if (this._recursivePreventLock) { 
                throw new InvalidOperationException("A handler cannot add a handler"); 
            }
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot add handler: null");
            }
            this._handlers.Add(handler);
        }

        public void LockChangeAnnounce(InputHandler handler)
        {
            LockStatus prev = LockStatus.SharedLock; // Make assumption that it is shared first, then check both
            if (!this._lockStatus.GetLockedHandler().Contains(handler))
            {
                prev = LockStatus.NoLock;
                if (handler.GetLockStatus() != LockStatus.NoLock)
                    throw new LockUnpermitChangeException();
                return;
            }
            else if (!this._lockStatus.GetIsShared())
            {
                prev = LockStatus.ExclusiveLock;
            }
            LockStatus current = handler.GetLockStatus();
            if (current > prev)
            {
                throw new LockUnpermitChangeException("The new lock level cannot be greater than het current level");
            }
            if (current == LockStatus.ExclusiveLock) { } // Nothing since unchanged
            else if (current == LockStatus.SharedLock)
            {
                this._lockStatus = new SharedLock(this._lockStatus.GetLockedHandler());
                this._hasLockChange = true;
            }
            else if (current == LockStatus.NoLock)
            {
                this._lockStatus.DropMember(handler);
                if (this._lockStatus.GetLockCount() == 0)
                {
                    this._lockStatus = new SharedLock(Array.Empty<InputHandler>());
                }
                this._hasLockChange = true;
            }
            else
            {
                throw new InvalidOperationException("Unexpected case");
            }
        }

        public void Dispatch(byte value)
        {
            if (this._recursivePreventLock)
            {
                throw new InvalidOperationException("Cannot dispatch from inner handler");
            }
            this._hasLockChange = false; // This is not thread safe
            foreach (InputHandler handler in this._handlers)
            {
                handler.AddBuffer(value);
            }
            InputHandler[] prevHandlers = {};
            if (this._lockStatus != null)
            {
                SharedLock prev = this._lockStatus;
                prevHandlers = prev.GetLockedHandler();
            }
            SharedLock lockValue = new SharedLock();
            foreach (InputHandler handler in this._handlers)
            {
                this._recursivePreventLock = true;
                try
                {
                    handler.ValidateExternal();
                }
                catch (Exception e)
                {
                    this._recursivePreventLock = false;
                    throw;
                }
                this._recursivePreventLock = false;
            }
            foreach (InputHandler handler in prevHandlers) // Previous handelr would be priotize
            {
                LockStatus status = handler.GetLockStatus();
                if (status == LockStatus.ExclusiveLock)
                {
                    lockValue = new SharedLock(handler, true);
                    break;
                }
                else if (status == LockStatus.SharedLock)
                {
                    lockValue.AddMember(handler);
                }
            }
            if (lockValue.GetIsShared()) // if already exclusive => ignore
            {
                foreach (InputHandler handler in this._handlers) // Then the normal order, the re-call of same handler doesn't matter
                {
                    LockStatus status = handler.GetLockStatus();
                    if (status == LockStatus.ExclusiveLock)
                    {
                        lockValue = new SharedLock(handler, true);
                        break;
                    }
                    else if (status == LockStatus.SharedLock)
                    {
                        lockValue.AddMember(handler);
                    }
                }
            }
            this._lockStatus = lockValue;
            foreach (InputHandler handler in this._handlers)
            {
                handler.Reset(lockValue);
            }
            foreach (InputHandler handler in this._handlers)
            {
                if (lockValue.GetLockCount() == 0 || lockValue.GetLockedHandler().Contains(handler))
                {
                    this._recursivePreventLock = true;
                    try
                    {
                        handler.HandleExternal(this);
                    }
                    catch (Exception e)
                    {
                        this._recursivePreventLock = false;
                        throw;
                    }
                    this._recursivePreventLock = false;
                }
            }
            if (this._hasLockChange)
            {
                foreach (InputHandler handler in this._handlers)
                {
                    handler.Reset(lockValue);
                }
            }
            foreach (InputHandler handler in this._handlers)
            {
                handler.DropUnused();
            }

        }
    }
}