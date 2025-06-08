using System;
using System.Collections.Generic;
using System.Linq;

namespace ui.core
{
    public struct LockMetadata
    {
        public readonly InputHandler lockHelder;
        public readonly bool innerSharable;

        public LockMetadata(InputHandler lockHelder, bool innerSharable)
        {
            this.lockHelder = lockHelder;
            this.innerSharable = innerSharable;
        }
    }

    public class SharedLock
    // When many handler consider the data to be significant but not sure
    // (only shared lock would receive the info unless a handler take exclusive lock or all lock handler release their lock)
    {
        private List<LockMetadata> lockHandler = new List<LockMetadata>();

        public void AddMember(LockMetadata metadata)
        {
            bool added = false;
            for (int idx = 0; idx < lockHandler.Count; idx++)
            {
                LockMetadata data = lockHandler[idx];
                if (data.lockHelder == metadata.lockHelder)
                {
                    lockHandler[idx] = metadata;
                    added = true;
                }
            }
            if (!added)
            {
                lockHandler.Add(metadata);
            }
        }

        public bool DropMember(InputHandler handler)
        {
            int initialSize = lockHandler.Count;
            lockHandler = lockHandler.Where(metadata=>metadata.lockHelder!=handler).ToList();
            return initialSize != lockHandler.Count;
        }

        public LockMetadata[] GetLockedHandler() => lockHandler.ToArray();
    }

    public class InputHandler
    {
        private readonly InputHandler _parent;
        private readonly List<InputHandler> _childs = new List<InputHandler>();
        internal bool SelfForceLock = false; // Lock that cannot be removed except themself
        internal bool SelfRemovableLock = false; // Lock that can be remove by child
        internal List<byte> Buffer;

        public InputHandler(InputHandler parent = null)
        {
            this._parent = parent;
        }

        public InputHandler GetParent() => this._parent;
        public InputHandler[] GetChild() => this._childs.ToArray();

        internal void AddChild(InputHandler handler)
        {
            if (CheckParent(handler))
            {
                throw new InvalidOperationException("This handler is already parent of current handler");
            } else if (CheckChild(handler))
            {
                throw new InvalidOperationException("This handler is already child of current handler");
            }
            this._childs.Add(handler);
        }

        public bool CheckChild(InputHandler handler)
        {
            return _childs.Any(child=>child==handler||child.CheckChild(handler));
        }

        public bool CheckParent(InputHandler handler)
        {
            if (handler.GetParent() == null)
            {
                return false;
            }
            return handler.GetParent() == handler || handler.GetParent().CheckParent(handler);
        }

        public bool GetSelfLock() => this.SelfForceLock || this.SelfRemovableLock;

        public bool GetAllLock()
        {
            if (this.GetSelfLock())
            {
                return true;
            }
            return this._childs.Any(handler=>handler.GetAllLock());
        }

        public void AddBuffer(byte value)
        {
            if (this.GetSelfLock())
            {
                this.Buffer.Add(value); // If exclusive lock
            } else if (this.GetAllLock()) // If other lock
            {
                foreach (InputHandler handler in this._childs)
                {
                    if (handler.GetAllLock())
                    {
                        handler.AddBuffer(value);
                    }
                }
            } else // No lock
            {
                this.Buffer.Add(value);
                foreach (InputHandler handler in this._childs)
                {
                    handler.AddBuffer(value);
                }
            }
        }

        public void ResetBuffer()
        {
            if (this.SelfRemovableLock)
            {
                return;
            }
            foreach (InputHandler handler in this._childs)
            {
                handler.ResetBuffer();
            }
            if (!this.GetAllLock())
            {
                this.Buffer = new List<byte>();
            }
        }

        public LockMetadata? GetLockMetaData()
        {
            if (!this.GetSelfLock())
            {
                return null;
            }
            else
            {
                return new LockMetadata(this, this.SelfRemovableLock);
            }
        }
    }
}