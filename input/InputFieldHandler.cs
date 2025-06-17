using ui.core;

namespace ui.input
{
    public class InputFieldHandler : InputHandler
    {
        private uint cursor = 0; // Position the place // -1 for position to remove
        private uint size = 0;
        private bool isActive = false;

        private string content = "";

        internal override void Handle(RootInputHandler root)
        {
            if (Buffer.Count == 0) return;
            byte value = Buffer[0];
            Buffer.RemoveAt(0);
            
        }

        public void SetActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        internal override LockStatus Validate()
        {
            return (
                isActive ?
                LockStatus.SharedLock: // Reserve Exclusive Lock for ANSI, not lock against the handler when active
                LockStatus.NoLock
            ); 
        }
    }
}