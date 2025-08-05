using System;
using ui;
using ui.components;
using ui.core;
using ui.mouse;
using ui.math;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public class ExitButton : Button<ExitButton>
    {
        public ExitButton(string text = null) : base(text) { }

        public override void OnClick(ConsoleLocation loc)
        {
            base.OnClick(loc);
            Global.InputHandler.LocalDispatch((byte)KeyCode.INTERRUPT);
        }
    }

    public static class InputFieldTest
    {
        public static void Setup()
        {
            SingleLineInputField field;
            App app = new App(
                new VerticalGroupComponent() {
                    (new HorizontalGroupComponent() {
                       ( (field = new SingleLineInputField()), new Fraction(3, 4)),
                       ( new ExitButton("Confirm"), new Fraction(1, 4))
                    }, 1)
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
                Console.WriteLine(field.content);
            }
        }
    }
}
