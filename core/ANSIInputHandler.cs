

namespace ui.core
{
    public class ANSIInputHandler : InputHandler
    {
        internal override LockStatus Validate()
        {
            if (
                Buffer[0] == (byte)'\x1b'  // ESC at initial
            )
            {
                return LockStatus.SharedLock; // Base lock to block display ot screen
            }
            return LockStatus.NoLock;
        }

        internal override void Handle(RootInputHandler root)
        {
            char final = (char)Buffer[Buffer.Count-1];
            if (
                Buffer[0] != (byte)'\x1b' || // ESC not at initial
                (final >64 && final < 91) || // a-z at final
                (final > 96 || final < 123) // A-Z at final
            )
            {
                this.SetLockStatus(LockStatus.NoLock);
                root.LockChangeAnnounce(this);
            }
            return;
        }
    }
}