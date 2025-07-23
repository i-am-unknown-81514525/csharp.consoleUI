using System;
using ui;
using ui.components;
using ui.core;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public class CounterButton : Button
    {
        internal string _base_text;
        internal int _count;

        public new string text
        {
            get => $"{_base_text} [{_count}]";
        }

        public string base_text
        {
            get => _base_text;
            set { _base_text = value; base.text = text; }
        }

        public int count
        {
            get => _count;
            set { _count = value; ; base.text = text; }
        }
        
        public CounterButton(string baseText = "", int Count = 0) : base($"{baseText} [{Count}]") {
            base_text = baseText;
            count = Count;
            onClickHandler = (ConsoleLocation _) => { count++; };
        }

    }

    public static class ButtonTest
    {
        public static void Setup()
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
                App app = new App(
                    new CounterButton("Click me", 0)
                );
                bool isComplete = false;
                while (!isComplete)
                {
                    Global.consoleCanva.EventLoopPre();
                    app.UpdateAllocSize();
                    bool status = Global.InputHandler.Handle();
                    if (!status)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }
                    if (exitHandler.GetExitStatus())
                    {
                        return;
                    }
                    Global.consoleCanva.ConsoleWindow = app.Render();
                    Global.consoleCanva.EventLoopPost();
                }
            }
            finally
            {
                ConsoleIntermediateHandler.Reset();
            }
        }
    }
}