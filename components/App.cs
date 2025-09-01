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

    public class App<S> : App<S, App<S>> where S : ComponentStore
    {
        public App(BaseComponent component) : base(component) { }

        protected App(BaseComponent component, ActiveStatusHandler activeStatusHandler) : base(component, activeStatusHandler) { }
    }

    public class App<S, T> : SingleChildComponent<S> where T : App<S, T> where S : ComponentStore
    {

        public Action<App<S, T>> onTickHandler = (_) => { };

        public Action<App<S, T>> onExitHandler = (_) => { };

        public App(BaseComponent component) : base(new ComponentConfig(new ActiveStatusHandler()))
        {
            noParent = true;
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        protected App(BaseComponent component, ActiveStatusHandler activeStatusHandler) : base(new ComponentConfig(activeStatusHandler))
        {
            noParent = true;
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

        public App<S, T> Run()
        {
            try
            {
                ConsoleIntermediateHandler.Setup();
                ConsoleIntermediateHandler.ANSISetup();
                NornalANSISkipHandler ansiSkipHandler = new NornalANSISkipHandler();
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
                    Global.consoleCanva.EventLoopPre();
                    onTickHandler(this);
                    bool status = Global.InputHandler.Handle();
                    if (exitHandler.GetExitStatus())
                    {
                        return this;
                    }
                    UpdateAllocSize();
                    bool haveUpdate = GetHasUpdate();
                    Global.consoleCanva.ConsoleWindow = this.Render();
                    Global.consoleCanva.EventLoopPost(haveUpdate);
                    System.Threading.Thread.Sleep(1);
                }
            }
            finally
            {
                ConsoleIntermediateHandler.Reset();
                onExitHandler(this);
            }
            return this;
        }
    }
}
