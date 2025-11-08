using System;
using System.Collections.Generic;
using ui.core;
using ui.utils;

namespace ui.input
{
    public class InputFieldHandler : InputHandler
    {
        protected uint cursor = 0; // Position the place/delete // -1 for position to backspace

        private bool isActive = false;

        protected byte? currBuf = null;

        protected string content = "";

        public string GetContent()
        {
            return content;
        }

        public void SetContent(string v)
        {
            if (v is null) return;
            content = v;
            if (cursor > content.Length)
                cursor = (uint)content.Length;
        }

        public void SetCursorPosition(uint cursorPos)
        {
            cursor = cursorPos;
            if (cursorPos > content.Length)
            {
                cursor = (uint)content.Length;
            }
        }

        protected byte GetBuf()
        {
            byte value = Buffer[0];
            Buffer.RemoveAt(0);
            return value;
        }

        public uint size
        {
            get => (uint)content.Length;
        }

        public uint cursorPos
        {
            get => cursor;
        }

        protected void Deactive(RootInputHandler root)
        {
            SetActiveStatus(false);
            SetLockStatus(LockStatus.NoLock);
            root.LockChangeAnnounce(this);
            onDeactive();
        }

        protected virtual void onDeactive() { }

        protected override void Handle(RootInputHandler root)
        {
            if (Buffer.Count == 0) return;
            currBuf = GetBuf();
            if (!isActive) return;
            if (currBuf == (byte)KeyCode.ESC)
            {
                Deactive(root);
            }
            else
            {
                Handle((byte)currBuf);
            }
        }

        protected virtual void onDefault(byte value)
        {
            List<byte> byteArr = content.AsByteBuffer().AsList();
            byteArr.Insert((int)cursor, value);
            content = byteArr.AsByteBuffer().AsString();
            cursor += 1;
        }

        protected virtual void Handle(byte value)
        {
            if (!Enum.IsDefined(typeof(KeyCode), value))
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
                case KeyCode.NEWLINE:
                    onEnter();
                    break;
                case KeyCode.PASTE:
                    onPaste();
                    break;
                default:
                    onDefault(value);
                    break;

            }
        }
        protected virtual void onArrLeft()
        {
            if (cursor > 0) cursor--;
        }

        protected virtual void onArrRight()
        {
            if (cursor < size) cursor++;
        }

        protected virtual void onArrUp()
        {
            cursor = 0;
        }

        protected virtual void onArrDown()
        {
            cursor = size;
        }

        protected virtual void onPgUp()
        {
            cursor = 0;
        }

        protected virtual void onPgDown()
        {
            cursor = size;
        }

        protected virtual void onInsertToggle() { }

        protected virtual void onApplicationUnfocus() { }

        protected virtual void onApplicationFocus() { }

        protected virtual void onBackspace()
        {
            if (cursor > 0)
            {
                List<byte> data = content.AsByteBuffer().AsList();
                cursor--;
                data.RemoveAt((int)cursor);
                content = data.AsByteBuffer().AsString();
            }
        }

        protected virtual void onDelete()
        {
            if (cursor < size)
            {
                List<byte> data = content.AsByteBuffer().AsList();
                data.RemoveAt((int)cursor);
                content = data.AsByteBuffer().AsString();
            }
        }

        protected virtual void onPaste()
        {
            string content = ConsoleHandler.ConsoleIntermediateHandler.ReadClipboard();
            content = content.GetReadable();
            for (int i = 0; i < content.Length; i++)
            {
                Handle((byte)content[i]);
            }
        }


        public void SetActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        public bool GetActiveStatus() => isActive;

        protected override LockStatus Validate()
        {
            return (
                isActive ?
                LockStatus.SharedLock : // Reserve Exclusive Lock for ANSI, not lock against the handler when active
                LockStatus.NoLock
            );
        }


        protected virtual void onEnter()
        {
            List<byte> byteArr = content.AsByteBuffer().AsList();
            byteArr.Insert((int)cursor, (byte)'\n');
            content = byteArr.AsByteBuffer().AsString();
            cursor += 1;
        }
    }

    public class MultiLineInputFieldHandler : InputFieldHandler
    {
        protected (uint row, uint column) vir_loc = (0, 0); // Suggestive location in multiline tranversal, which might not be valid

        public (uint row, uint column) size2D
        {
            get => to2D(size);
        }

        public (uint row, uint column) cursorPos2D
        {
            get => loc;
        }

        // private (uint row, uint column) _loc = (0, 0);

        protected (uint row, uint column) loc
        {
            get => to2D(cursor);
            set
            {
                cursor = to1D(value);
            }
        }

        public uint to1D((uint row, uint column) loc)
        {
            string[] op = content.Split('\n');
            uint idx = 0;
            uint col = loc.column;
            uint row = loc.row;
            if (row >= op.Length)
            {
                row = (uint)op.Length - 1;
            }
            if (col > op[row].Length)
            {
                col = (uint)op[row].Length;
            }
            for (uint i = 0; i < row; i++)
            {
                idx += (uint)op[i].Length + 1;
            }
            idx += col;
            return idx;
        }
        public (uint row, uint column) to2D(uint idx)
        {
            uint row = 0;
            uint column = 0;
            for (uint i = 0; i < idx; i++)
            {
                byte value = (byte)content[(int)i];
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

        protected (uint row, uint column) Correct((uint row, uint column) loc)
        {
            return to2D(to1D(loc));
            // string[] strArr = content.Split('\n');
            // uint row = loc.row;
            // uint col = loc.column;
            // if (row < 0) row = 0;
            // if (col < 0) col = 0;
            // if (row >= strArr.Length)
            // {
            //     if (strArr.Length > 0)
            //         row = (uint)strArr.Length - 1;
            //     else
            //         row = (uint)strArr.Length;
            // }
            // string data = strArr[row];
            // if (col >= data.Length)
            // {
            //     if (data.Length > 0)
            //         col = (uint)data.Length - 1;
            //     else
            //         col = (uint)data.Length;
            // }
            // return (row, col);
        }

        protected override void Handle(byte value)
        {
            base.Handle(value);
            loc = to2D(cursor);
        }

        protected override void onDefault(byte value)
        {
            base.onDefault(value);
            vir_loc = loc = to2D(cursor);
        }

        protected override void onArrUp()
        {
            if (vir_loc.row > 0 && loc.row > 0)
            {
                vir_loc = (vir_loc.row - 1, vir_loc.column);
                loc = Correct(vir_loc);
            }
            else
            {
                cursor = 0;
                loc = to2D(cursor);
                vir_loc = to2D(cursor);
            }
        }

        protected override void onArrDown()
        {
            if (vir_loc.row < content.Split('\n').Length - 1 && loc.row < content.Split('\n').Length - 1)
            {
                vir_loc = (vir_loc.row + 1, vir_loc.column);
                loc = Correct(vir_loc);
            }
            else
            {
                cursor = size;
                loc = to2D(cursor);
                vir_loc = to2D(cursor);
            }

        }

        protected override void onArrLeft()
        {
            base.onArrLeft();
            loc = to2D(cursor);
            vir_loc = to2D(cursor);
        }

        protected override void onArrRight()
        {
            base.onArrRight();
            loc = to2D(cursor);
            vir_loc = to2D(cursor);
        }

    }
}
