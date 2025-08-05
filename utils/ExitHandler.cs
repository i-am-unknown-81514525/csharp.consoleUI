using ui.core;

namespace ui.utils
{
    public class ExitHandler : InputHandler
    {
        private bool exit = false;

        protected override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() > LockStatus.NoLock)
            {
                exit = true;
                this.SetLockStatus(LockStatus.NoLock);
                root.LockChangeAnnounce(this);
            }
        }

        protected override LockStatus Validate()
        {
            if (Buffer.Count > 0 && Buffer[0] == (byte)KeyCode.INTERRUPT) return LockStatus.ExclusiveLock;
            return LockStatus.NoLock;
        }

        public bool GetExitStatus() => exit;
    }
}
