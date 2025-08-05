using System;
using ui.components;
using ui.components.chainExt;
using ui.core;
using ui.mouse;
using ui.math;
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
                switcher = new Switcher()
                {
                    new VerticalGroupComponent() {
                        (
                            new Button("Switch")
                                .WithHandler(
                                    (loc) => {
                                        idx++;
                                        idx %= 2;
                                        switcher.SwitchTo(idx);
                                    }
                                ), new Fraction(3, 4)),
                        (new ExitButton("End"), new Fraction(1, 4))
                    },
                    new Button("Switch")
                        .WithHandler(
                            (loc) => {
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
                Console.WriteLine(DEBUG.DebugStore.ToString());
            }
        }
    }
}
