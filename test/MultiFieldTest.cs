using System;
using System.Threading;
using ui.components;
using ui.core;
using ui.math;
using ui.mouse;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public static class MultiFieldTest
    {
        public static void Setup()
        {
            SingleLineInputField field1;
            SingleLineInputField field2;
            // App app = new App(
            //     new VerticalGroupComponent() {
            //         (new HorizontalGroupComponent() {
            //            ( (field = new SingleLineInputField()), new Fraction(3, 4)),
            //            ( new ExitButton("Confirm"), new Fraction(1, 4))
            //         }, 1)
            //     }
            // );
            App app = new App(
                new VerticalGroupComponent {
                    (new HorizontalGroupComponent {
                        (field1 = new SingleLineInputField(), new Fraction(3, 4)),
                        (new TextLabel(""), new Fraction(1, 4))
                    }, 1),
                    (new HorizontalGroupComponent {
                        (field2 = new SingleLineInputField(), new Fraction(3, 4)),
                        (new ExitButton("Confirm"), new Fraction(1, 4))
                    }, 1)
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
                Console.WriteLine(field1.content);
                Console.WriteLine(field2.content);
            }
        }
    }
}
