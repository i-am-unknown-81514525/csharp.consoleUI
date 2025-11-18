using ui.core;

namespace ui.utils
{
    public class ExitHandler : InputHandler
    {
        private bool _exit = false;

        protected override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() > LockStatus.NO_LOCK)
            {
                _exit = true;
                this.SetLockStatus(LockStatus.NO_LOCK);
                root.LockChangeAnnounce(this);
            }
        }

        protected override LockStatus Validate()
        {
            if (Buffer.Count > 0 && Buffer[0] == (byte)KeyCode.INTERRUPT) return LockStatus.EXCLUSIVE_LOCK;
            return LockStatus.NO_LOCK;
        }

        public bool GetExitStatus() => _exit;
    }
}
