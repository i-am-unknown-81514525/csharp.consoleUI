using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ui.core.ConsoleHandler.ConsoleIntermediateHandler;
using ui.utils;

namespace ui.core
{
    
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
                AddMember(handler);
            }
        }

        public SharedLock(InputHandler handler, bool isExclusive = false)
        {
            _lockHandler.Add(handler);
            _isShared = !isExclusive;
        }

        public bool GetIsShared() => _isShared;

        public bool AddMember(InputHandler handler)
        {
            if (!GetIsShared())
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
            if (GetLockCount() == 0)
            {
                _isShared = true;
            }
            return count != 0;
        }

        public InputHandler[] GetLockedHandler() => _lockHandler.ToArray();

        public int GetLockCount() => GetLockedHandler().Length;
    }

    public enum LockStatus : int
    {
        NoLock = 0,
        SharedLock = 1,
        ExclusiveLock = 32767
    }

    public class RootInputHandler
    // This code assuming that the code call from abstract function `Validate` and `Handle` in `InputHandler`
    // might have malicious intend, therefore it should attempt to defense against it.
    // The cause of an exception is not consider malicious and thus not required to be defense against
    // TODO: Add method to set prioity for InputHandler, which would rearrange the list
    {
        private List<InputHandler> _handlers = new List<InputHandler>();
        private List<ANSIInputHandler> _ansi_handlers = new List<ANSIInputHandler>();
        private bool _hasLockChange = false;
        private bool _recursivePreventLock = false; // Prevent iner nhandler to call dispatch whcih might cause recurrsion
        private SharedLock _lockStatus = new SharedLock();

        public RootInputHandler(IEnumerable<InputHandler> handlers = null, IEnumerable<ANSIInputHandler> ansi_handlers = null)
        {
            if (handlers != null)
            {
                _handlers = handlers.ToList();
            }
            if (ansi_handlers != null)
            {
                _ansi_handlers = ansi_handlers.ToList();
            }
        }

        public void Add(InputHandler handler)
        {
            checkLock();
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot add handler: null");
            }
            if (!_handlers.Contains(handler))
                _handlers.Add(handler);
        }

        public void Add(ANSIInputHandler handler)
        {
            checkLock();
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot add ANSI handler: null");
            }
            if (!_ansi_handlers.Contains(handler))
                _ansi_handlers.Add(handler);
        }

        public void Remove(InputHandler handler)
        {
            checkLock();
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot remove handler: null");
            }
            if (_handlers.Contains(handler))
                _handlers.Remove(handler);
            else
                throw new InvalidOperationException("Cannot remove handler as handler does't exist");
        }

        public void Remove(ANSIInputHandler handler)
        {
            checkLock();
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot remove ANSI handler: null");
            }
            if (_ansi_handlers.Contains(handler))
                _ansi_handlers.Remove(handler);
            else
                throw new InvalidOperationException("Cannot remove ANSI handler as handler does't exist");
        }

        public void LockChangeAnnounce(InputHandler handler)
        {
            LockStatus prev = handler.GetPrevLockStatus();
            LockStatus current = handler.GetLockStatus();
            if (current > prev)
            {
                throw new LockUnpermitChangeException("The new lock level cannot be greater than het current level");
            }
            if (current == prev)
            {
                return;
            }
            _lockStatus = new SharedLock(_lockStatus.GetLockedHandler());
            _hasLockChange = true;
            _lockStatus.DropMember(handler);
            if (_lockStatus.GetLockCount() == 0)
            {
                _lockStatus = new SharedLock(Array.Empty<InputHandler>());
            }
        }

        protected void checkLock()
        {
            if (_recursivePreventLock)
            {
                throw new InvalidOperationException("Cannot dispatch from inner handler");
            }
        }

        private bool ANSIDispatch(List<byte> buf)
        {
            return _ansi_handlers
                    .Select(handler => handler.Handle(buf.ToArray()))
                    .ToArray() // Make sure all handler is ran, before checking if handled by one of the handler
                               // As there might be other handler need to run actions
                    .Any(x => x);
        }

        protected void MultiDispatch(List<byte> data)
        {
            foreach (byte v in data)
            {
                LocalDispatch(v);
            }
        }

        public bool Handle()
        {


            if (!StdinDataRemain()) return false;
            byte value = Read();
            if (value != (byte)'\x1b')
            {
                LocalDispatch(value);
                return true;
            }
            List<byte> buf = new List<byte> { value };
            List<byte> unhandled_buf = new List<byte> { };
            string new_part = "";
            while ((new_part = ReadStdinToEnd()) != "")
            {
                buf = (buf.AsByteBuffer() + new_part).AsList();
                List<byte> inner_buf = new List<byte>();
                foreach (byte b in buf)
                {
                    if (b == (byte)'\x1b')
                    {
                        bool result = false;
                        if (inner_buf.Count > 2) //  && inner_buf[1] == (byte)'['
                            result = ANSIDispatch(inner_buf);
                        if (!result)
                            unhandled_buf = (unhandled_buf.AsByteBuffer() + inner_buf.AsByteBuffer()).AsList();
                        inner_buf = new List<byte>();
                    }
                    inner_buf.Add(b);
                }
                MultiDispatch(unhandled_buf);
                unhandled_buf = new List<byte>();
                buf = inner_buf;
            }
            bool r1 = false;
            if (buf.Count > 2) //  && inner_buf[1] == (byte)'['
                r1 = ANSIDispatch(buf);
            if (!r1)
                MultiDispatch(buf);
                return true;
        }

        public void LocalDispatch(byte value)
        {
            checkLock();
            _hasLockChange = false; // This is not thread safe
            foreach (InputHandler handler in _handlers)
            {
                handler.AddBuffer(value);
            }
            InputHandler[] prevHandlers = { };
            if (_lockStatus != null)
            {
                SharedLock prev = _lockStatus;
                prevHandlers = prev.GetLockedHandler();
            }
            SharedLock lockValue = new SharedLock();
            foreach (InputHandler handler in _handlers)
            {
                _recursivePreventLock = true;
                try
                {
                    handler.ValidateExternal();
                }
                catch (Exception)
                {
                    _recursivePreventLock = false;
                    throw;
                }
                
                _recursivePreventLock = false;
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
                foreach (InputHandler handler in _handlers) // Then the normal order, the re-call of same handler doesn't matter
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
            _lockStatus = lockValue;
            foreach (InputHandler handler in _handlers)
            {
                handler.Reset(lockValue);
            }
            foreach (InputHandler handler in _handlers)
            {
                if (lockValue.GetLockCount() == 0 || lockValue.GetLockedHandler().Contains(handler))
                {
                    _recursivePreventLock = true;
                    try
                    {
                        handler.HandleExternal(this);
                    }
                    catch (Exception)
                    {
                        _recursivePreventLock = false;
                        throw;
                    }
                    _recursivePreventLock = false;
                }
            }
            if (_hasLockChange)
            {
                foreach (InputHandler handler in _handlers)
                {
                    handler.Reset(lockValue);
                }
            }
            foreach (InputHandler handler in _handlers)
            {
                handler.DropUnused();
            }

        }
    }
}