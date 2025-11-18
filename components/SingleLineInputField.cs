using System;
using ui.core;
using ui.events;
using ui.fmt;
using ui.input;

namespace ui.components
{
    public class ComponentInputFieldHandler : InputFieldHandler
    {
        protected Action PostHandler;
        protected Action<Event> ExitHandler;

        public ComponentInputFieldHandler(Action postHandler = null, Action<Event> exitHandler = null)
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
            // base.onEnter();
            Deactive(Global.InputHandler);
        }

        protected override void OnDeactive()
        {
            base.OnDeactive();
            if (ExitHandler != null)
                ExitHandler(new TypeEvent('\n'));
        }
    }

    public class SingleLineInputField<T> : NoChildComponent<T> where T : ComponentStore
    {
        //Reactive of content with type string and default value: `""`, Trigger: SetHasUpdate();
        public string content { get => InputFieldHandler.GetContent(); set { InputFieldHandler.SetContent(value); SetHasUpdate(); } }

        //Reactive of active with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.BLACK, BackgroundColorEnum.WHITE)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _active = (ForegroundColorEnum.BLACK, BackgroundColorEnum.YELLOW);
        public (ForegroundColor foreground, BackgroundColor background) active { get => _active; set { _active = value; SetHasUpdate(); } }

        //Reactive of deactive with type (ForegroundColor foreground, BackgroundColor background) and default value: `(ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK)`, Trigger: SetHasUpdate();
        private (ForegroundColor foreground, BackgroundColor background) _deactive = (ForegroundColorEnum.WHITE, BackgroundColorEnum.BLACK);
        public (ForegroundColor foreground, BackgroundColor background) deactive { get => _deactive; set { _deactive = value; SetHasUpdate(); } }

        protected ComponentInputFieldHandler InputFieldHandler = new ComponentInputFieldHandler();

        //Reactive of underline with type bool and default value: `true`, Trigger: SetHasUpdate();
        private bool _underline = true;
        public bool underline { get => _underline; set { _underline = value; SetHasUpdate(); } }

        public SingleLineInputField(string content = "")
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
                Debug.DebugStore.Append("input field removed handler\r\n");
            }
            else
            {
                Debug.DebugStore.Append("input field handler already missing\r\n");
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

        protected int GetStartIdx()
        {
            uint cursorIdx = InputFieldHandler.cursorPos;
            uint size = GetAllocSize().x;
            int startIdx = (int)cursorIdx - (int)size;
            if (startIdx < 0)
            {
                startIdx = 0;
            }
            return startIdx;
        }

        protected (string render, int cursorPos) GetRenderContent()
        {
            uint cursorIdx = InputFieldHandler.cursorPos;
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
            (string renderContent, int cursorPos) = GetRenderContent();
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
            SetCursorPos();
            return content;
        }

        public override void OnClick(ConsoleLocation consoleLocation)
        {
            (int y, int x) = GetAbsolutePos((consoleLocation.Y, consoleLocation.X));
            bool isActive = !IsActive() ? SetActive(new ClickEvent(new ConsoleLocation(x, y))) : true;
            if (isActive)
            {
                int startIdx = GetStartIdx();
                InputFieldHandler.SetCursorPosition((uint)(startIdx + x));
                SetCursorPos();
            }
            SetHasUpdate();
        }

        protected void SetCursorPos()
        {
            if (IsActive())
            {
                (string _, int cursorPos) = GetRenderContent();
                (int row, int col) = GetAbsolutePos((0, cursorPos));
                Global.ConsoleCanva.CursorPosition = (row + 1, col + 1);
            }
        }

        protected override void OnResize()
        {
            base.OnResize();
            SetCursorPos();
            SetHasUpdate();
        }

        public override string AsLatex()
        {
            return $"\\text{{{content}}}";
        }
    }

    public class SingleLineInputField : SingleLineInputField<EmptyStore>
    {
        public SingleLineInputField(string content = "") : base(content) { }
    }
}
