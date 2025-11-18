using System;
using System.Collections.Generic;
using ui.core;
using ui.utils;

namespace ui.input
{
    public class InputFieldHandler : InputHandler
    {
        protected uint Cursor = 0; // Position the place/delete // -1 for position to backspace

        private bool _isActive = false;

        protected byte? CurrBuf = null;

        protected string Content = "";

        public string GetContent()
        {
            return Content;
        }

        public void SetContent(string v)
        {
            if (v is null) return;
            Content = v;
            if (Cursor > Content.Length)
                Cursor = (uint)Content.Length;
        }

        public void SetCursorPosition(uint cursorPos)
        {
            Cursor = cursorPos;
            if (cursorPos > Content.Length)
            {
                Cursor = (uint)Content.Length;
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
            get => (uint)Content.Length;
        }

        public uint cursorPos
        {
            get => Cursor;
        }

        protected void Deactive(RootInputHandler root)
        {
            SetActiveStatus(false);
            SetLockStatus(LockStatus.NO_LOCK);
            root.LockChangeAnnounce(this);
            OnDeactive();
        }

        protected virtual void OnDeactive() { }

        protected override void Handle(RootInputHandler root)
        {
            if (Buffer.Count == 0) return;
            CurrBuf = GetBuf();
            if (!_isActive) return;
            if (CurrBuf == (byte)KeyCode.ESC)
            {
                Deactive(root);
            }
            else
            {
                Handle((byte)CurrBuf);
            }
        }

        protected virtual void OnDefault(byte value)
        {
            List<byte> byteArr = Content.AsByteBuffer().AsList();
            byteArr.Insert((int)Cursor, value);
            Content = byteArr.AsByteBuffer().AsString();
            Cursor += 1;
        }

        protected virtual void Handle(byte value)
        {
            if (!Enum.IsDefined(typeof(KeyCode), value))
            {
                OnDefault(value);
                return;
            }
            switch ((KeyCode)value)
            {
                case KeyCode.ARR_LEFT:
                    OnArrLeft();
                    break;
                case KeyCode.ARR_RIGHT:
                    OnArrRight();
                    break;
                case KeyCode.ARR_UP:
                    OnArrUp();
                    break;
                case KeyCode.ARR_DOWN:
                    OnArrDown();
                    break;
                case KeyCode.INSERT:
                    OnInsertToggle();
                    break;
                case KeyCode.SPECIAL_FOCUS:
                    OnApplicationFocus();
                    break;
                case KeyCode.SPECIAL_UNFOCUS:
                    OnApplicationUnfocus();
                    break;
                case KeyCode.BACKSPACE:
                    OnBackspace();
                    break;
                case KeyCode.TAB:
                    OnTab();
                    break;
                case KeyCode.DEL:
                    OnDelete();
                    break;
                case KeyCode.PG_UP:
                    OnPgUp();
                    break;
                case KeyCode.PG_DOWN:
                    OnPgDown();
                    break;
                case KeyCode.NEWLINE:
                    OnEnter();
                    break;
                case KeyCode.PASTE:
                    OnPaste();
                    break;
                case KeyCode.NUL:
                case KeyCode.CTRLZ:
                    // Ignore
                    break;
                default:
                    OnDefault(value);
                    break;

            }
        }
        protected virtual void OnArrLeft()
        {
            if (Cursor > 0) Cursor--;
        }

        protected virtual void OnArrRight()
        {
            if (Cursor < size) Cursor++;
        }

        protected virtual void OnArrUp()
        {
            Cursor = 0;
        }

        protected virtual void OnArrDown()
        {
            Cursor = size;
        }

        protected virtual void OnPgUp()
        {
            Cursor = 0;
        }

        protected virtual void OnPgDown()
        {
            Cursor = size;
        }

        protected virtual void OnInsertToggle() { }

        protected virtual void OnApplicationUnfocus() { }

        protected virtual void OnApplicationFocus() { }

        protected virtual void OnBackspace()
        {
            if (Cursor > 0)
            {
                List<byte> data = Content.AsByteBuffer().AsList();
                Cursor--;
                data.RemoveAt((int)Cursor);
                Content = data.AsByteBuffer().AsString();
            }
        }

        protected virtual void OnTab()
        {
            OnDefault((byte)' ');
        }

        protected virtual void OnDelete()
        {
            if (Cursor < size)
            {
                List<byte> data = Content.AsByteBuffer().AsList();
                data.RemoveAt((int)Cursor);
                Content = data.AsByteBuffer().AsString();
            }
        }

        protected virtual void OnPaste()
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
            _isActive = isActive;
        }

        public bool GetActiveStatus() => _isActive;

        protected override LockStatus Validate()
        {
            return (
                _isActive ?
                LockStatus.SHARED_LOCK : // Reserve Exclusive Lock for ANSI, not lock against the handler when active
                LockStatus.NO_LOCK
            );
        }


        protected virtual void OnEnter()
        {
            List<byte> byteArr = Content.AsByteBuffer().AsList();
            byteArr.Insert((int)Cursor, (byte)'\n');
            Content = byteArr.AsByteBuffer().AsString();
            Cursor += 1;
        }
    }

    public class MultiLineInputFieldHandler : InputFieldHandler
    {
        protected (uint row, uint column) VirLoc = (0, 0); // Suggestive location in multiline tranversal, which might not be valid

        public (uint row, uint column) size2D
        {
            get => To2D(size);
        }

        public (uint row, uint column) cursorPos2D
        {
            get => loc;
        }

        // private (uint row, uint column) _loc = (0, 0);

        protected (uint row, uint column) loc
        {
            get => To2D(Cursor);
            set
            {
                Cursor = To1D(value);
            }
        }

        public uint To1D((uint row, uint column) loc)
        {
            string[] op = Content.Split('\n');
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
        public (uint row, uint column) To2D(uint idx)
        {
            uint row = 0;
            uint column = 0;
            for (uint i = 0; i < idx; i++)
            {
                byte value = (byte)Content[(int)i];
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
            return To2D(To1D(loc));
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
            loc = To2D(Cursor);
        }

        protected override void OnDefault(byte value)
        {
            base.OnDefault(value);
            VirLoc = loc = To2D(Cursor);
        }

        protected override void OnArrUp()
        {
            if (VirLoc.row > 0 && loc.row > 0)
            {
                VirLoc = (VirLoc.row - 1, VirLoc.column);
                loc = Correct(VirLoc);
            }
            else
            {
                Cursor = 0;
                loc = To2D(Cursor);
                VirLoc = To2D(Cursor);
            }
        }

        protected override void OnArrDown()
        {
            if (VirLoc.row < Content.Split('\n').Length - 1 && loc.row < Content.Split('\n').Length - 1)
            {
                VirLoc = (VirLoc.row + 1, VirLoc.column);
                loc = Correct(VirLoc);
            }
            else
            {
                Cursor = size;
                loc = To2D(Cursor);
                VirLoc = To2D(Cursor);
            }

        }

        protected override void OnArrLeft()
        {
            base.OnArrLeft();
            loc = To2D(Cursor);
            VirLoc = To2D(Cursor);
        }

        protected override void OnArrRight()
        {
            base.OnArrRight();
            loc = To2D(Cursor);
            VirLoc = To2D(Cursor);
        }

    }
}
