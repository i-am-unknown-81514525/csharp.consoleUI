using System;
using System.Threading;
using ui.components;
using ui.components.chainExt;
using ui.core;
using ui.math;
using ui.mouse;
using static ui.core.ConsoleHandler;

namespace ui.test
{

    public static class SwitcherTest
    {
        public static void Setup()
        {
            Switcher switcher = null;
            int idx = 0;
            App app = new App(
                switcher = new Switcher
                {
                    new VerticalGroupComponent {
                        (
                            new Button("Switch")
                                .WithHandler(
                                    loc => {
                                        idx++;
                                        idx %= 2;
                                        switcher.SwitchTo(idx);
                                    }
                                ), new Fraction(3, 4)),
                        (new ExitButton("End"), new Fraction(1, 4))
                    },
                    new Button("Switch")
                        .WithHandler(
                            loc => {
                                idx++;
                                idx %= 2;
                                switcher.SwitchTo(idx);
                            }
                        )
                }
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
                Console.WriteLine(Debug.DebugStore.ToString());
            }
        }
    }
}
