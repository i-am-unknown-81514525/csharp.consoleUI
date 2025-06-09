using System;
using System.Linq;
using System.Collections.Generic;

namespace ui.core
{
        public struct LockMetadata
        {
            public readonly InputHandler LockHelder;
            public readonly bool InnerSharable;

            public LockMetadata(InputHandler lockHelder, bool innerSharable)
            {
                this.LockHelder = lockHelder;
                this.InnerSharable = innerSharable;
            }
        }

        public class Lock
        {
            internal static readonly bool isShared;
        }

        public class SingleLock
        {
            internal static readonly bool isShared = false;
        }

        public class SharedLock: Lock
        // When many handler consider the data to be significant but not sure
        // (only shared lock would receive the info unless a handler take exclusive lock or all lock handler release their lock)
        {
            internal static readonly bool isShared = false;
            private List<LockMetadata> lockHandler = new List<LockMetadata>();

            public void AddMember(LockMetadata metadata)
            {
                bool added = false;
                for (int idx = 0; idx < lockHandler.Count; idx++)
                {
                    LockMetadata data = lockHandler[idx];
                    if (data.LockHelder == metadata.LockHelder)
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
                lockHandler = lockHandler.Where(metadata=>metadata.LockHelder!=handler).ToList();
                return initialSize != lockHandler.Count;
            }

            public LockMetadata[] GetLockedHandler() => lockHandler.ToArray();

            public static SharedLock operator +(SharedLock curr, SharedLock other)
            {
                SharedLock newLock = new SharedLock();
                foreach (LockMetadata metadata in curr.lockHandler)
                {
                    newLock.AddMember(metadata);
                }
                foreach (LockMetadata metadata in other.GetLockedHandler())
                {
                    newLock.AddMember(metadata);
                }
                return newLock;
            }

            public int GetLockCount() => this.GetLockedHandler().Length;
        }

        public enum LockStatus: int
        {
            NoLock=0,
            SharedLock=1,
            ExclusiveLock=2
        }

        public class InputHandler
        {
            private LockStatus _lockStatus = LockStatus.NoLock;
            internal List<byte> Buffer;

            internal void SetLock(bool isExclusive = false)
            {
                this._lockStatus = isExclusive ? LockStatus.ExclusiveLock : LockStatus.SharedLock;
            }

            public LockStatus GetLock() => this._lockStatus;

            public bool Reset() {}

        }
}