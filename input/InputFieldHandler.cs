using System.Collections.Generic;
using ui.core;

namespace ui.input
{
    public class InputFieldHandler : InputHandler
    {
        internal uint cursor = 0; // Position the place/delete // -1 for position to backspace
        internal uint size = 0;
        private bool isActive = false;

        internal byte? currBuf = null;

        internal string content = "";

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

        internal virtual void onDefault(byte value)
        {
            content += (char)value;
        }

        internal virtual void Handle(byte value)
        {
            if (!Enum.IsDefined(typeof(Keycode), value))
            {
                onDefault(value);
                return;
            }
            switch ((KeyCode)value)
            {
                case KeyCode.ARR_LEFT:
                    onArrLeft();
                    break;
                case KeyCode.ARR_RIGHT:
                    onArrRight();
                    break;
                case KeyCode.ARR_UP:
                    onArrUp();
                    break;
                case KeyCode.ARR_DOWN:
                    onArrDown();
                    break;
                case KeyCode.INSERT:
                    onInsertToggle();
                    break;
                case KeyCode.SPECIAL_FOCUS:
                    onApplicationFocus();
                    break;
                case KeyCode.SPECIAL_UNFOCUS:
                    onApplicationUnfocus();
                    break;
                case KeyCode.BACKSPACE:
                    onBackspace();
                    break;
                case KeyCode.DEL:
                    onDelete();
                    break;
                case KeyCode.PG_UP:
                    onPgUp();
                    break;
                case KeyCode.PG_DOWN:
                    onPgDown();
                    break;
                default:
                    onDefault(value);
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
                LockStatus.SharedLock : // Reserve Exclusive Lock for ANSI, not lock against the handler when active
                LockStatus.NoLock
            );
        }
    }

    public class MultiLineInputFieldHandler : InputFieldHandler
    {
        internal (int row, int column) loc = (0, 0);
        internal (int row, int column) vir_loc = (0, 0); // Suggestive location in multiline tranversal, which might not be valid

        internal int to1D((int row, int column) loc)
        {
            string[] op = content.Split('\n');
            int idx = 0;
            for (int i = 0; i < loc.row; i++)
            {
                idx += op[i].Length + 1;
            }
            idx += loc.column;
            return idx;
        }
        internal (int row, int column) to1D(int idx)
        {
            int row = 0;
            int column = 0;
            for (int i = 0; i < idx; i++)
            {
                byte value = content[i];
                if (value == '\n')
                {
                    row++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }
            return (row, column);
        }

        internal (int row, int column) Correct((int row, int column) loc)
        {
            string[] strArr = content.Split('\n');
            int row = loc.row;
            int col = loc.column;
            if (row < 0) row = 0;
            if (col < 0) col = 0;
            if (row >= strArr.Length)
            {
                if (strArr.Length > 0)
                    row = strArr.Length - 1;
                else
                    row = strArr.Length;
            }
            string data = strArr[row];
            if (col >= data.Length)
            {
                if (data.Length > 0)
                    col = data.Length - 1;
                else
                    col = data.Length;
            }
            return (row, col);
        }

        internal override void Handle(byte value)
        {
            base.Handle(value);
            loc = to2D(cursor);
        }

        internal override void onArrUp()
        {
            if (vir_loc.row > 0)
                vir_loc = (vir_loc.row - 1, vir_loc.column);
            loc = Correct(vir_loc);
        }

        internal override void onArrDown()
        {
            if (vir_loc.row < content.Split('\n').Length - 1)
                vir_loc = (vir_loc.row + 1, vir_loc.column);
            loc = Correct(vir_loc);
        }

        internal override void onArrLeft()
        {
            base.onArrLeft();
            loc = to2D(cursor);
            vir_loc = to2D(cursor);
        }

        internal override void onArrRight()
        {
            base.onArrRight();
            loc = to2D(cursor);
            vir_loc = to2D(cursor);
        }
        
    }
}