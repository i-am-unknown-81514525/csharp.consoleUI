using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.utils;
using ui.core;
using ui.mouse;
using ui.input;

using static ui.core.ConsoleHandler;

namespace ui.components
{
    public class App : App<EmptyStore, App>
    {
        public App(BaseComponent component) : base(component) { }

        protected App(BaseComponent component, ActiveStatusHandler activeStatusHandler) : base(component, activeStatusHandler) { }
    }

    public class App<TS> : App<TS, App<TS>> where TS : ComponentStore
    {
        public App(BaseComponent component) : base(component) { }

        protected App(BaseComponent component, ActiveStatusHandler activeStatusHandler) : base(component, activeStatusHandler) { }
    }

    public class App<TS, T> : SingleChildComponent<TS> where T : App<TS, T> where TS : ComponentStore
    {

        public Action<App<TS, T>> OnTickHandler = (_) => { };

        public Action<App<TS, T>> OnExitHandler = (_) => { };

        public App(BaseComponent component) : base(new ComponentConfig(new ActiveStatusHandler()))
        {
            NoParent = true;
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        protected App(BaseComponent component, ActiveStatusHandler activeStatusHandler) : base(new ComponentConfig(activeStatusHandler))
        {
            NoParent = true;
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        protected override void OnResize()
        {
            if (GetMapping().Count == 0) throw new InvalidOperationException("An App must have a child component");
            SetChildAllocatedSize(GetMapping()[0].component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        public App<TS, T> Run()
        {
            try
            {
                ConsoleIntermediateHandler.Setup();
                ConsoleIntermediateHandler.AnsiSetup();
                NornalAnsiSkipHandler ansiSkipHandler = new NornalAnsiSkipHandler();
                Global.InputHandler.Add(ansiSkipHandler);
                KeyCodeTranslationHandler keyCodeHandler = new KeyCodeTranslationHandler(Global.InputHandler);
                Global.InputHandler.Add(keyCodeHandler);
                ExitHandler exitHandler = new ExitHandler();
                Global.InputHandler.Add(exitHandler);
                MouseClickHandler mouseClickHandler = new MouseClickHandler(this);
                Global.InputHandler.Add(mouseClickHandler);
                bool isComplete = false;
                SetHasUpdate();
                while (!isComplete)
                {
                    Global.ConsoleCanva.EventLoopPre();
                    OnTickHandler(this);
                    bool status = Global.InputHandler.Handle();
                    if (exitHandler.GetExitStatus())
                    {
                        // return this;
                        isComplete = true;
                    }
                    UpdateAllocSize();
                    bool haveUpdate = GetHasUpdate();
                    Global.ConsoleCanva.ConsoleWindow = Render();
                    Global.ConsoleCanva.EventLoopPost(haveUpdate);
                    System.Threading.Thread.Sleep(1);
                }
            }
            finally
            {
                ConsoleIntermediateHandler.Reset();
                OnExitHandler(this);
            }
            return this;
        }
    }
}
