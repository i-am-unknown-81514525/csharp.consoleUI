using System;
using System.Threading;
using ui.components;
using ui.components.chainExt;
using ui.core;
using ui.mouse;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public class CounterButton : Button<EmptyStore, CounterButton>
    {
        internal string BaseText;
        internal int Count;

        public new string text
        {
            get => $"{BaseText} [{Count}]";
        }

        public string baseText
        {
            get => BaseText;
            set { BaseText = value; base.text = text; }
        }

        public int count
        {
            get => Count;
            set { Count = value; base.text = text; }
        }

        public CounterButton(string baseText = "", int count = 0) : base($"{baseText} [{count}]")
        {
            this.baseText = baseText;
            this.count = count;
            this.WithHandler(_ => { this.count++; });
        }

        public override string Debug_Info() => text;

    }

    public static class ButtonTest
    {
        public static void Setup()
        {
            App app = new App(
                new CounterButton("Click me")
            );
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
                MouseClickHandler mouseClickHandler = new MouseClickHandler(app);
                Global.InputHandler.Add(mouseClickHandler);
                bool isComplete = false;
                while (!isComplete)
                {
                    Global.ConsoleCanva.EventLoopPre();
                    bool status = Global.InputHandler.Handle();
                    if (!status)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    if (exitHandler.GetExitStatus())
                    {
                        return;
                    }
                    Global.ConsoleCanva.ConsoleWindow = app.Render();
                    Global.ConsoleCanva.EventLoopPost();
                }
            }
            finally
            {
                ConsoleIntermediateHandler.Reset();
                Console.WriteLine(app.Debug_WriteStructure());
            }
        }
    }
}
