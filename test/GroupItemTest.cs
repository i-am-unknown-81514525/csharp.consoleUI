using System;
using ui;
using ui.components;
using ui.core;
using ui.mouse;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public static class GroupItemTest
    {
        public static void Setup()
        {
            App app = new App(
                new VerticalGroupComponent()
                {
                    new HorizontalGroupComponent() {
                        new CounterButton("Click me 1", 0),
                        (new Seperator(), 2),
                        new CounterButton("Click me 2", 0)
                    },
                    (new Seperator(), 1),
                    new HorizontalGroupComponent() {
                        new CounterButton("Click me 3", 0),
                        (new Seperator(), 2),
                        new CounterButton("Click me 4", 0)
                    }
                }
            );
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
                MouseClickHandler mouseClickHandler = new MouseClickHandler(app);
                Global.InputHandler.Add(mouseClickHandler);
                bool isComplete = false;
                while (!isComplete)
                {
                    Global.consoleCanva.EventLoopPre();
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
                Console.WriteLine(app.Debug_WriteStructure());
            }
        }
    }
}
