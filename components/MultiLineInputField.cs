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
        protected Action postHandler;
        protected Action<Event> exitHandler;

        public MultiLineComponentInputFieldHandler(Action postHandler = null, Action<Event> exitHandler = null)
        {
            SetHandler(postHandler);
            SetHandler(exitHandler);
        }

        public bool SetHandler(Action postHandler)
        {
            if (!(this.postHandler is null))
                return false;
            this.postHandler = postHandler;
            return true;
        }

        public bool SetHandler(Action<Event> exitHandler)
        {
            if (!(this.exitHandler is null))
                return false;
            this.exitHandler = exitHandler;
            return true;
        }

        protected override void Handle(RootInputHandler root)
        {
            base.Handle(root);
            if (postHandler != null)
                postHandler();
        }

        protected override void onEnter()
        {
            base.onEnter();
            // this.Deactive(Global.InputHandler);
        }

        protected override void onDeactive()
        {
            base.onDeactive();
            if (exitHandler != null)
                exitHandler(new TypeEvent('\n'));
        }
    }

    public class MultiLineInputField : NoChildComponent
    {
        //Reactive of content with type string and default value: `""`, Trigger: SetHasUpdate();
        public string content { get => inputFieldHandler.GetContent(); set { inputFieldHandler.SetContent(content); SetHasUpdate(); } }

        //Reactive of active with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.BLACK, BackgroundColorEnum.WHITE)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _active = (ForegroundColorEnum.BLACK, BackgroundColorEnum.YELLOW);
        public (ForegroundColor foreground, BackgroundColor background) active { get => _active; set { _active = value; SetHasUpdate(); } }

        //Reactive of deactive with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _deactive = (ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK);
        public (ForegroundColor foreground, BackgroundColor background) deactive { get => _deactive; set { _deactive = value; SetHasUpdate(); } }

        protected MultiLineComponentInputFieldHandler inputFieldHandler = new MultiLineComponentInputFieldHandler();

        protected (uint row, uint column) top_left = (0, 0);

        public MultiLineInputField(string content = "") : base()
        {
            inputFieldHandler.SetHandler(OnTypeEventTrigger);
            inputFieldHandler.SetHandler(OnExitEventTrigger);
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
                Global.consoleCanva.cursorPosition = null;
            }
            setCursorPos();
        }

        protected void OnExitEventTrigger(Event curr)
        {
            if (IsActive())
            {
                Deactive(null);
                inputFieldHandler.SetCursorPosition((uint)inputFieldHandler.GetContent().Length);
                SetHasUpdate();
            }
        }

        protected override void OnActive()
        {
            if (!Global.InputHandler.Contains(inputFieldHandler))
                Global.InputHandler.Add(inputFieldHandler);
            inputFieldHandler.SetActiveStatus(true);
            SetHasUpdate();
            setCursorPos();
        }

        protected override bool OnDeactive(Event deactiveEvent)
        {
            bool canConvertToTypeEvent = !((deactiveEvent as TypeEvent) is null);
            DEBUG.DebugStore.Append($"input field deactive event: {canConvertToTypeEvent}\r\n");
            if (canConvertToTypeEvent)
                return false;
            if (Global.InputHandler.Contains(inputFieldHandler))
            {
                Global.consoleCanva.cursorPosition = null;
                Global.InputHandler.Remove(inputFieldHandler);
                DEBUG.DebugStore.Append($"input field removed handler\r\n");
            }
            else
            {
                DEBUG.DebugStore.Append($"input field handler already missing\r\n");
            }
            Global.consoleCanva.cursorPosition = null;
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
            if (inputFieldHandler.size2D.row < top_left.row)
            {
                top_left = inputFieldHandler.size2D;
                SetHasUpdate();
            }
            string[] content = inputFieldHandler.GetContent().Split('\n');
            int maxLength = content.Max(x => x.Length);
            if (maxLength < top_left.column)
            {
                top_left = (top_left.row, (uint)maxLength);
                SetHasUpdate();
            }
        }

        protected void CursorJumpOnType()
        {
            if (top_left.row >= inputFieldHandler.cursorPos2D.row)
            {
                top_left = (inputFieldHandler.cursorPos2D.row, top_left.column);
                SetHasUpdate();
            }

            if (top_left.row + GetAllocSize().y - 1 < inputFieldHandler.cursorPos2D.row)
            {
                top_left = (inputFieldHandler.cursorPos2D.row - GetAllocSize().y + 1, top_left.column);
                SetHasUpdate();
            }

            if (top_left.column >= inputFieldHandler.cursorPos2D.column)
            {
                top_left = (top_left.row, inputFieldHandler.cursorPos2D.column);
                SetHasUpdate();
            }

            if (top_left.column + GetAllocSize().x - 1 < inputFieldHandler.cursorPos2D.column)
            {
                top_left = (top_left.row, inputFieldHandler.cursorPos2D.column - GetAllocSize().x + 1);
                SetHasUpdate();
            }
        }

        protected (string[] render, (int r, int c) cursorPos) getRenderContent()
        {
            ForceInBound();
            string[] src = inputFieldHandler.GetContent().Split('\n');
            string[] render = new string[GetAllocSize().y];
            for (uint row = top_left.row; row < (top_left.row + GetAllocSize().y) && row < src.Length; row++)
            {
                string rowContent = src[row];
                uint renderRow = row - top_left.row;
                if (rowContent.Length <= top_left.column)
                {
                    render[renderRow] = "";
                    continue;
                }
                string result = rowContent.Substring((int)top_left.column);
                if (result.Length > GetAllocSize().x)
                {
                    result = result.Substring(0, (int)GetAllocSize().x - 1) + SpecialChar.SINGLE_CHAR_ELLIPSIS;
                }
                render[renderRow] = result;
            }
            for (uint row = (uint)src.Length; row < (top_left.row + GetAllocSize().y); row++)
            {
                uint renderRow = row - top_left.row;
                render[renderRow] = "";
            }
            (uint row, uint column) cursorLoc = inputFieldHandler.cursorPos2D;
            return (render, ((int)(cursorLoc.row - top_left.row), (int)(cursorLoc.column - top_left.column)));

        }

        protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        {
            (uint x, uint y) size = GetAllocSize();
            if (size.x < 1 || size.y < 1) return content;
            bool isActive = IsActive();
            (ForegroundColor fore, BackgroundColor back) = isActive ? active : deactive;
            (string[] renderContent, (int r, int c) cursorPos) = getRenderContent();
            string prefix = TextColorFormatter.Constructor(fore, back);
            string postfix = TextColorFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT);
            for (int y = 0; y < size.y; y++)
            {
                string line_content = renderContent[y];
                if (line_content.Length < GetAllocSize().x)
                {
                    line_content += new string(' ', (int)size.x - line_content.Length);
                }
                for (int x = 0; x < size.x; x++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = line_content[x].ToString(),
                        ansiPrefix = prefix,
                        ansiPostfix = postfix,
                        isContent = true
                    };
                }
            }
            setCursorPos();
            return content;
        }

        public override void OnClick(ConsoleLocation consoleLocation)
        {
            (int y, int x) = this.GetAbsolutePos((consoleLocation.y, consoleLocation.x));
            bool isActive = !IsActive() ? SetActive(new ClickEvent(new ConsoleLocation(x, y))) : true;
            if (isActive)
            {
                inputFieldHandler.SetCursorPosition(inputFieldHandler.to1D(((uint)(y + top_left.row), (uint)(x + top_left.column))));
                // int startIdx = getStartIdx();
                // inputFieldHandler.SetCursorPosition((uint)(startIdx + x));
                setCursorPos();
            }
            SetHasUpdate();
        }

        protected void setCursorPos()
        {
            if (IsActive())
            {
                (string[] _, (int r, int c) cursorPos) = getRenderContent();
                (int row, int col) = this.GetAbsolutePos(cursorPos);
                Global.consoleCanva.cursorPosition = (row + 1, col + 1);
            }
        }

        protected override void OnResize()
        {
            base.OnResize();
            setCursorPos();
            SetHasUpdate();
        }
    }
}
