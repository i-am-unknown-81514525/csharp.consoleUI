using System;
using System.Linq;
using ui.input;
using ui.core;
using ui.events;
using ui.fmt;

namespace ui.components
{
    public class MultiLineComponentInputFieldHandler : MultiLineInputFieldHandler
    {
        protected Action PostHandler;
        protected Action<Event> ExitHandler;

        public MultiLineComponentInputFieldHandler(Action postHandler = null, Action<Event> exitHandler = null)
        {
            SetHandler(postHandler);
            SetHandler(exitHandler);
        }

        public bool SetHandler(Action postHandler)
        {
            if (!(PostHandler is null))
                return false;
            PostHandler = postHandler;
            return true;
        }

        public bool SetHandler(Action<Event> exitHandler)
        {
            if (!(ExitHandler is null))
                return false;
            ExitHandler = exitHandler;
            return true;
        }

        protected override void Handle(RootInputHandler root)
        {
            base.Handle(root);
            if (PostHandler != null)
                PostHandler();
        }

        protected override void OnEnter()
        {
            base.OnEnter();
            // this.Deactive(Global.InputHandler);
        }

        protected override void OnDeactive()
        {
            base.OnDeactive();
            if (ExitHandler != null)
                ExitHandler(new TypeEvent('\n'));
        }
    }

    public class MultiLineInputField : NoChildComponent
    {
        //Reactive of content with type string and default value: `""`, Trigger: SetHasUpdate();
        public string content { get => InputFieldHandler.GetContent(); set { InputFieldHandler.SetContent(value); SetHasUpdate(); } }

        //Reactive of active with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.BLACK, BackgroundColorEnum.WHITE)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _active = (ForegroundColorEnum.BLACK, BackgroundColorEnum.YELLOW);
        public (ForegroundColor foreground, BackgroundColor background) active { get => _active; set { _active = value; SetHasUpdate(); } }

        //Reactive of deactive with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _deactive = (ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK);
        public (ForegroundColor foreground, BackgroundColor background) deactive { get => _deactive; set { _deactive = value; SetHasUpdate(); } }

        protected MultiLineComponentInputFieldHandler InputFieldHandler = new MultiLineComponentInputFieldHandler();

        protected (uint row, uint column) TopLeft = (0, 0);

        public MultiLineInputField(string content = "") : base()
        {
            InputFieldHandler.SetHandler(OnTypeEventTrigger);
            InputFieldHandler.SetHandler(OnExitEventTrigger);
            this.content = content ?? "";
            SetHasUpdate();
        }

        protected void OnTypeEventTrigger()
        {
            SetHasUpdate();
            bool isActive = IsActive();
            CursorJumpOnType();
            ForceInBound();
            if (!isActive) // This would have been a toggle of state since type event only occur on change in active/deactive, or a type event
            {
                Global.ConsoleCanva.CursorPosition = null;
            }
            SetCursorPos();
        }

        protected void OnExitEventTrigger(Event curr)
        {
            if (IsActive())
            {
                Deactive(null);
                InputFieldHandler.SetCursorPosition((uint)InputFieldHandler.GetContent().Length);
                OnExitHandler();
                SetHasUpdate();
            }
        }

        protected virtual void OnExitHandler()
        {

        }

        protected override void OnActive()
        {
            if (!Global.InputHandler.Contains(InputFieldHandler))
                Global.InputHandler.Add(InputFieldHandler);
            InputFieldHandler.SetActiveStatus(true);
            SetHasUpdate();
            SetCursorPos();
        }

        protected override bool OnDeactive(Event deactiveEvent)
        {
            bool canConvertToTypeEvent = !((deactiveEvent as TypeEvent) is null);
            Debug.DebugStore.Append($"input field deactive event: {canConvertToTypeEvent}\r\n");
            if (canConvertToTypeEvent)
                return false;
            if (Global.InputHandler.Contains(InputFieldHandler))
            {
                Global.ConsoleCanva.CursorPosition = null;
                Global.InputHandler.Remove(InputFieldHandler);
                Debug.DebugStore.Append($"input field removed handler\r\n");
            }
            else
            {
                Debug.DebugStore.Append($"input field handler already missing\r\n");
            }
            Global.ConsoleCanva.CursorPosition = null;
            SetHasUpdate();
            return true;
        }

        protected override void OnHideInternal()
        {
            base.OnHideInternal();
            if (IsActive())
                Deactive(new NotRenderEvent());
        }

        protected void ForceInBound()
        {
            // Enforce top_left to be within content
            if (InputFieldHandler.size2D.row < TopLeft.row)
            {
                TopLeft = InputFieldHandler.size2D;
                SetHasUpdate();
            }
            string[] arrContent = InputFieldHandler.GetContent().Split('\n');
            int maxLength = arrContent.Max(x => x.Length);
            if (maxLength < TopLeft.column)
            {
                TopLeft = (TopLeft.row, (uint)maxLength);
                SetHasUpdate();
            }
        }

        protected void CursorJumpOnType()
        {
            if (TopLeft.row >= InputFieldHandler.cursorPos2D.row)
            {
                TopLeft = (InputFieldHandler.cursorPos2D.row, TopLeft.column);
                SetHasUpdate();
            }

            if (TopLeft.row + GetAllocSize().y - 1 < InputFieldHandler.cursorPos2D.row)
            {
                TopLeft = (InputFieldHandler.cursorPos2D.row - GetAllocSize().y + 1, TopLeft.column);
                SetHasUpdate();
            }

            if (TopLeft.column >= InputFieldHandler.cursorPos2D.column)
            {
                TopLeft = (TopLeft.row, InputFieldHandler.cursorPos2D.column);
                SetHasUpdate();
            }

            if (TopLeft.column + GetAllocSize().x - 1 < InputFieldHandler.cursorPos2D.column)
            {
                TopLeft = (TopLeft.row, InputFieldHandler.cursorPos2D.column - GetAllocSize().x + 1);
                SetHasUpdate();
            }
        }

        protected (string[] render, (int r, int c) cursorPos) GetRenderContent()
        {
            ForceInBound();
            string[] src = InputFieldHandler.GetContent().Split('\n');
            string[] render = new string[GetAllocSize().y];
            for (uint row = TopLeft.row; row < (TopLeft.row + GetAllocSize().y) && row < src.Length; row++)
            {
                string rowContent = src[row];
                uint renderRow = row - TopLeft.row;
                if (rowContent.Length <= TopLeft.column)
                {
                    render[renderRow] = "";
                    continue;
                }
                string result = rowContent.Substring((int)TopLeft.column);
                if (result.Length > GetAllocSize().x)
                {
                    result = result.Substring(0, (int)GetAllocSize().x - 1) + SpecialChar.SingleCharEllipsis;
                }
                render[renderRow] = result;
            }
            for (uint row = (uint)src.Length; row < (TopLeft.row + GetAllocSize().y); row++)
            {
                uint renderRow = row - TopLeft.row;
                render[renderRow] = "";
            }
            (uint row, uint column) cursorLoc = InputFieldHandler.cursorPos2D;
            return (render, ((int)(cursorLoc.row - TopLeft.row), (int)(cursorLoc.column - TopLeft.column)));

        }

        protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        {
            (uint x, uint y) size = GetAllocSize();
            if (size.x < 1 || size.y < 1) return content;
            bool isActive = IsActive();
            (ForegroundColor fore, BackgroundColor back) = isActive ? active : deactive;
            (string[] renderContent, (int r, int c) cursorPos) = GetRenderContent();
            string prefix = TextColorFormatter.Constructor(fore, back);
            string postfix = TextColorFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT);
            for (int y = 0; y < size.y; y++)
            {
                string lineContent = renderContent[y];
                if (lineContent.Length < GetAllocSize().x)
                {
                    lineContent += new string(' ', (int)size.x - lineContent.Length);
                }
                for (int x = 0; x < size.x; x++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = lineContent[x].ToString(),
                        ansiPrefix = prefix,
                        ansiPostfix = postfix,
                        isContent = true
                    };
                }
            }
            SetCursorPos();
            return content;
        }

        public override void OnClick(ConsoleLocation consoleLocation)
        {
            (int y, int x) = GetAbsolutePos((consoleLocation.Y, consoleLocation.X));
            bool isActive = !IsActive() ? SetActive(new ClickEvent(new ConsoleLocation(x, y))) : true;
            if (isActive)
            {
                InputFieldHandler.SetCursorPosition(InputFieldHandler.To1D(((uint)(y + TopLeft.row), (uint)(x + TopLeft.column))));
                // int startIdx = getStartIdx();
                // inputFieldHandler.SetCursorPosition((uint)(startIdx + x));
                SetCursorPos();
            }
            SetHasUpdate();
        }

        protected void SetCursorPos()
        {
            if (IsActive())
            {
                (string[] _, (int r, int c) cursorPos) = GetRenderContent();
                (int row, int col) = GetAbsolutePos(cursorPos);
                Global.ConsoleCanva.CursorPosition = (row + 1, col + 1);
            }
        }

        protected override void OnResize()
        {
            base.OnResize();
            SetCursorPos();
            SetHasUpdate();
        }
    }
}
