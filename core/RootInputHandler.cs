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
        NO_LOCK = 0,
        SHARED_LOCK = 1,
        EXCLUSIVE_LOCK = 32767
    }

    public class RootInputHandler
    // This code assuming that the code call from abstract function `Validate` and `Handle` in `InputHandler`
    // might have malicious intend, therefore it should attempt to defense against it.
    // The cause of an exception is not consider malicious and thus not required to be defense against
    // TODO: Add method to set prioity for InputHandler, which would rearrange the list
    {
        private List<InputHandler> _handlers = new List<InputHandler>();
        private List<AnsiInputHandler> _ansiHandlers = new List<AnsiInputHandler>();
        private List<InputHandler> _rmHandlers = new List<InputHandler>(); // When running, use these list to prevent invalid state by only process on next loop
        // This is not atomic and you cannot add and remove the same handler at the same frame or it might cause unexpected behaviour
        // The guarentee event is if you remove a handler and it is successful, the handler will be removed at the start of next frame
        // However, a handler might still be removed if you perform remove and then add in the same frame
        // It is not guarentee that the unexpected behaviour would occur
        private List<AnsiInputHandler> _rmAnsiHandlers = new List<AnsiInputHandler>();
        private List<InputHandler> _addHandlers = new List<InputHandler>();
        private List<AnsiInputHandler> _addAnsiHandlers = new List<AnsiInputHandler>();
        private bool _hasLockChange = false;
        private bool _recursivePreventLock = false; // Prevent iner nhandler to call dispatch whcih might cause recurrsion
        private SharedLock _lockStatus = new SharedLock();

        public RootInputHandler(IEnumerable<InputHandler> handlers = null, IEnumerable<AnsiInputHandler> ansiHandlers = null)
        {
            if (handlers != null)
            {
                _handlers = handlers.ToList();
            }
            if (ansiHandlers != null)
            {
                _ansiHandlers = ansiHandlers.ToList();
            }
        }

        public void Add(InputHandler handler)
        {
            if (!HasLock())
            {
                ProcessFrameAddOrRemoval();
            }
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot add handler: null");
            }
            if (!_handlers.Contains(handler))
            {
                if (HasLock()) _addHandlers.Add(handler);
                else _handlers.Add(handler);
            }
        }

        public void Add(AnsiInputHandler handler)
        {
            if (!HasLock())
            {
                ProcessFrameAddOrRemoval();
            }
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot add ANSI handler: null");
            }
            if (!_ansiHandlers.Contains(handler))
            {
                if (HasLock()) _addAnsiHandlers.Add(handler);
                else _ansiHandlers.Add(handler);
            }
        }

        public void Remove(InputHandler handler)
        {
            if (!HasLock())
            {
                ProcessFrameAddOrRemoval();
            }
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot remove handler: null");
            }
            if (Contains(handler))
            {
                if (HasLock()) _rmHandlers.Add(handler);
                else _handlers.Remove(handler);
            }
            else
                throw new InvalidOperationException("Cannot remove handler as handler does't exist");
        }

        public void Remove(AnsiInputHandler handler)
        {
            if (!HasLock())
            {
                ProcessFrameAddOrRemoval();
            }
            if (handler == null)
            {
                throw new InvalidOperationException("Cannot remove ANSI handler: null");
            }
            if (Contains(handler))
            {
                if (HasLock()) _rmAnsiHandlers.Add(handler);
                else _ansiHandlers.Remove(handler);
            }
            else
                throw new InvalidOperationException("Cannot remove ANSI handler as handler does't exist");
        }

        public bool Contains(InputHandler handler) => (_handlers.Contains(handler) || _addHandlers.Contains(handler)) && !_rmHandlers.Contains(handler);
        public bool Contains(AnsiInputHandler handler) => (_ansiHandlers.Contains(handler) || _addAnsiHandlers.Contains(handler)) && !_rmAnsiHandlers.Contains(handler);

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

        protected bool HasLock()
        {
            return _recursivePreventLock;
        }

        protected void CheckLock()
        {
            if (_recursivePreventLock)
            {
                throw new InvalidOperationException("Cannot dispatch from inner handler");
            }
        }

        private bool AnsiDispatch(List<byte> buf)
        {
            return _ansiHandlers
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

        private void ProcessFrameAddOrRemoval()
        {
            CheckLock();
            foreach (InputHandler handler in _addHandlers)
            {
                if (!_handlers.Contains(handler))
                    _handlers.Add(handler);
            }
            _addHandlers = new List<InputHandler>();
            foreach (AnsiInputHandler handler in _addAnsiHandlers)
            {
                if (!_ansiHandlers.Contains(handler))
                    _ansiHandlers.Add(handler);
            }
            _addAnsiHandlers = new List<AnsiInputHandler>();
            foreach (InputHandler handler in _rmHandlers)
            {
                if (_handlers.Contains(handler))
                    _handlers.Remove(handler);
            }
            _rmHandlers = new List<InputHandler>();
            foreach (AnsiInputHandler handler in _rmAnsiHandlers)
            {
                if (_ansiHandlers.Contains(handler))
                    _ansiHandlers.Remove(handler);
            }
            _rmAnsiHandlers = new List<AnsiInputHandler>();
        }

        public bool Handle()
        {
            if (!HasLock())
            {
                ProcessFrameAddOrRemoval();
            }
            if (!StdinDataRemain()) return false;
            byte value = Read();
            if (value != (byte)'\x1b')
            {
                LocalDispatch(value);
                return true;
            }
            List<byte> buf = new List<byte> { value };
            List<byte> unhandledBuf = new List<byte>();
            string newPart;
            while ((newPart = ReadStdinToEnd()) != "")
            {
                buf = (buf.AsByteBuffer() + newPart).AsList();
                List<byte> innerBuf = new List<byte>();
                foreach (byte b in buf)
                {
                    if (b == (byte)'\x1b')
                    {
                        bool result = false;
                        if (innerBuf.Count > 2) //  && inner_buf[1] == (byte)'['
                            result = AnsiDispatch(innerBuf);
                        if (!result)
                            unhandledBuf = (unhandledBuf.AsByteBuffer() + innerBuf.AsByteBuffer()).AsList();
                        innerBuf = new List<byte>();
                    }
                    innerBuf.Add(b);
                }
                MultiDispatch(unhandledBuf);
                unhandledBuf = new List<byte>();
                buf = innerBuf;
            }
            bool r1 = false;
            if (buf.Count > 2) //  && inner_buf[1] == (byte)'['
                r1 = AnsiDispatch(buf);
            if (!r1)
                MultiDispatch(buf);
            return true;
        }

        public void LocalDispatch(byte value)
        {
            CheckLock();
            ProcessFrameAddOrRemoval();
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
                if (status == LockStatus.EXCLUSIVE_LOCK)
                {
                    lockValue = new SharedLock(handler, true);
                    break;
                }
                else if (status == LockStatus.SHARED_LOCK)
                {
                    lockValue.AddMember(handler);
                }
            }
            if (lockValue.GetIsShared()) // if already exclusive => ignore
            {
                foreach (InputHandler handler in _handlers) // Then the normal order, the re-call of same handler doesn't matter
                {
                    LockStatus status = handler.GetLockStatus();
                    if (status == LockStatus.EXCLUSIVE_LOCK)
                    {
                        lockValue = new SharedLock(handler, true);
                        break;
                    }
                    else if (status == LockStatus.SHARED_LOCK)
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
