using System.Collections.Generic;
using ui.core;

namespace ui.input
{
    public abstract class InputFieldHandler : InputHandler
    {
        private uint cursor = 0; // Position the place/delete // -1 for position to backspace
        private uint size = 0;
        private bool isActive = false;

        internal byte? currBuf = null;

        private string content = "";

        internal byte GetBuf()
        {
            byte value = Buffer[0];
            Buffer.RemoveAt(0);
            return value;
        }

        internal override void Handle(RootInputHandler root)
        {
            if (Buffer.Count == 0) return;
            currBuf = GetBuf();
            if (!isActive) return;
            if (currBuf == (byte)KeyCode.ESC)
            {
                SetActiveStatus(false);
                SetLockStatus(LockStatus.NoLock);
                root.LockChangeAnnounce(this);
            }
            else
            {
                Handle((byte)currBuf);
            }
        }

        internal virtual void Handle(byte value)
        {
            switch (value)
            {
                case (byte)KeyCode.ARR_LEFT:
                    onArrLeft();
                    break;
                case (byte)KeyCode.ARR_RIGHT:
                    onArrRight();
                    break;
                case (byte)KeyCode.ARR_UP:
                    onArrUp();
                    break;
                case (byte)KeyCode.ARR_DOWN:
                    onArrDown();
                    break;
                case (byte)KeyCode.INSERT:
                    onInsertToggle();
                    break;
                case (byte)KeyCode.SPECIAL_FOCUS:
                    onApplicationFocus();
                    break;
                case (byte)KeyCode.SPECIAL_UNFOCUS:
                    onApplicationUnfocus();
                    break;
                case (byte)KeyCode.BACKSPACE:
                    onBackspace();
                    break;
                case (byte)KeyCode.DEL:
                    onDelete();
                    break;
                case (byte)KeyCode.PG_UP:
                    onPgUp();
                    break;
                case (byte)KeyCode.PG_DOWN:
                    onPgDown();
                    break;
                default:
                    content += (char)value;
                    break;
                
            }   
        }
        internal virtual void onArrLeft()
        {
            if (cursor > 0) cursor--;
        }

        internal virtual void onArrRight()
        {
            if (cursor < size) cursor++;
        }

        internal virtual void onArrUp()
        {
            cursor = 0;
        }

        internal virtual void onArrDown()
        {
            cursor = size;
        }

        internal virtual void onPgUp()
        {
            cursor = 0;
        }

        internal virtual void onPgDown()
        {
            cursor = size;
        }

        internal virtual void onInsertToggle() { }

        internal virtual void onApplicationUnfocus() { }

        internal virtual void onApplicationFocus() { }

        internal virtual void onBackspace()
        {
            if (cursor > 0)
            {
                List<byte> data = content.AsByteBuffer().AsList();
                cursor--;
                data.RemoveAt((int)cursor);
                content = data.AsByteBuffer().AsString();
            }
        }

        internal virtual void onDelete()
        {
            if (cursor < size)
            {
                List<byte> data = content.AsByteBuffer().AsList();
                data.RemoveAt((int)cursor);
                content = data.AsByteBuffer().AsString();
            }
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