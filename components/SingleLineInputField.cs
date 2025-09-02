using System;
using ui.input;
using ui.core;
using ui.events;
using ui.fmt;

namespace ui.components
{
    public class ComponentInputFieldHandler : InputFieldHandler
    {
        protected Action postHandler;
        protected Action<Event> exitHandler;

        public ComponentInputFieldHandler(Action postHandler = null, Action<Event> exitHandler = null)
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
            // base.onEnter();
            this.Deactive(Global.InputHandler);
        }

        protected override void onDeactive()
        {
            base.onDeactive();
            if (exitHandler != null)
                exitHandler(new TypeEvent('\n'));
        }
    }

    public class SingleLineInputField : NoChildComponent
    {
        //Reactive of content with type string and default value: `""`, Trigger: SetHasUpdate();
        public string content { get => inputFieldHandler.GetContent(); set { inputFieldHandler.SetContent(content); SetHasUpdate(); } }

        //Reactive of active with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.BLACK, BackgroundColorEnum.WHITE)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _active = (ForegroundColorEnum.BLACK, BackgroundColorEnum.YELLOW);
        public (ForegroundColor foreground, BackgroundColor background) active { get => _active; set { _active = value; SetHasUpdate(); } }

        //Reactive of deactive with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _deactive = (ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK);
        public (ForegroundColor foreground, BackgroundColor background) deactive { get => _deactive; set { _deactive = value; SetHasUpdate(); } }

        protected ComponentInputFieldHandler inputFieldHandler = new ComponentInputFieldHandler();

        //Reactive of underline with type bool and default value: `true`, Trigger: SetHasUpdate();
        private bool _underline = true;
        public bool underline { get => _underline; set { _underline = value; SetHasUpdate(); } }

        public SingleLineInputField(string content = "") : base()
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

        protected int getStartIdx()
        {
            uint cursorIdx = inputFieldHandler.cursorPos;
            uint size = GetAllocSize().x;
            int startIdx = (int)cursorIdx - (int)size;
            if (startIdx < 0)
            {
                startIdx = 0;
            }
            return startIdx;
        }

        protected (string render, int cursorPos) getRenderContent()
        {
            uint cursorIdx = inputFieldHandler.cursorPos;
            uint size = GetAllocSize().x;
            int startIdx = (int)cursorIdx - (int)size;
            int displayCursorPos = (int)size - 1;
            if (startIdx < 0)
            {
                displayCursorPos = (int)size + startIdx;
                startIdx = 0;
            }
            return (content.Substring(startIdx, (int)Math.Min(size, content.Length - startIdx)), displayCursorPos);
        }

        protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        {
            (uint x, uint y) size = GetAllocSize();
            if (size.x < 1 || size.y < 1) return content;
            bool isActive = IsActive();
            (ForegroundColor fore, BackgroundColor back) = isActive ? active : deactive;
            (string renderContent, int cursorPos) = getRenderContent();
            string prefix = TextColorFormatter.Constructor(fore, back);
            string postfix = TextColorFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT);
            string lastlinepre = prefix;
            string lastlinepost = postfix;
            if (underline)
            {
                lastlinepre = prefix + TextStyleFormatter.Constructor(EnableStyleEnum.UNDERLINE);
                lastlinepost = postfix + TextStyleFormatter.Constructor(DisableStyleEnum.UNDERLINE);
            }
            for (int x = 0; x < renderContent.Length && x < size.x; x++)
            {
                content[x, 0] = new ConsoleContent
                {
                    content = renderContent[x].ToString(),
                    ansiPrefix = size.y == 1 ? lastlinepre : prefix,
                    ansiPostfix = size.y == 1 ? lastlinepost : postfix,
                    isContent = true
                };
            }
            for (int x = renderContent.Length; x < size.x; x++)
            {
                content[x, 0] = new ConsoleContent
                {
                    content = " ",
                    ansiPrefix = size.y == 1 ? lastlinepre : prefix,
                    ansiPostfix = size.y == 1 ? lastlinepost : postfix,
                    isContent = true
                };
            }
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 1; y < size.y - 1; y++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = prefix,
                        ansiPostfix = postfix,
                        isContent = true
                    };
                }
            }
            int fy = (int)size.y - 1;
            if (fy != 0)
            {
                for (int x = 0; x < size.x; x++)
                {
                    content[x, fy] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = lastlinepre,
                        ansiPostfix = lastlinepost,
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
                int startIdx = getStartIdx();
                inputFieldHandler.SetCursorPosition((uint)(startIdx + x));
                setCursorPos();
            }
            SetHasUpdate();
        }

        protected void setCursorPos()
        {
            if (IsActive())
            {
                (string _, int cursorPos) = getRenderContent();
                (int row, int col) = this.GetAbsolutePos((0, cursorPos));
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
