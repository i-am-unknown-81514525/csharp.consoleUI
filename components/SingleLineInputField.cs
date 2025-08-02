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
            base.onEnter();
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
        }

        protected void OnExitEventTrigger(Event curr)
        {
            if (IsActive())
            {
                setInactive();
                inputFieldHandler.SetCursorPosition((uint)inputFieldHandler.GetContent().Length);
                SetHasUpdate();
            }
        }

        protected override void onActive()
        {
            Global.InputHandler.Add(inputFieldHandler);
            inputFieldHandler.SetActiveStatus(true);
            SetHasUpdate();
            Global.consoleCanva.cursorPosition = getAbsolutePos((0, 0));
        }

        protected override bool onDeactive(Event deactiveEvent)
        {
            bool canConvertToTypeEvent = !((deactiveEvent as TypeEvent) is null);
            if (canConvertToTypeEvent)
                return false;
            Global.InputHandler.Remove(inputFieldHandler);
            SetHasUpdate();
            return true;
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

        public bool IsActive()
        {
            return activeHandler.getCurrActive() == this;
        }

        protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        {
            bool isActive = IsActive();
            (ForegroundColor fore, BackgroundColor back) = isActive ? active : deactive;
            (string renderContent, int cursorPos) = getRenderContent();
            for (int x = 0; x < renderContent.Length; x++)
            {
                content[x, 0] = new ConsoleContent
                {
                    content = renderContent[x].ToString(),
                    ansiPrefix = TextFormatter.Constructor(fore, back),
                    ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                    isContent = true
                };
            }
            (uint x, uint y) size = GetAllocSize();
            for (int x = renderContent.Length; x < size.x; x++)
            {
                content[x, 0] = new ConsoleContent
                {
                    content = " ",
                    ansiPrefix = TextFormatter.Constructor(fore, back),
                    ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                    isContent = true
                };
            }
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 1; y < size.y; y++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = TextFormatter.Constructor(fore, back),
                        ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                        isContent = true
                    };
                }
            }
            return content;
        }

        public override void onClick(ConsoleLocation consoleLocation)
        {
            (int y, int x) = this.getAbsolutePos((consoleLocation.y, consoleLocation.x));
            bool isActive = !IsActive() ? setActive(new ClickEvent(new ConsoleLocation(x, y))) : true;
            if (isActive)
            {
                int startIdx = getStartIdx();
                inputFieldHandler.SetCursorPosition((uint)(startIdx + x));
            }
            SetHasUpdate();
        }
    }
}
