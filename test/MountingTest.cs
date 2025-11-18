using System;
using System.Threading;
using ui.components;
using ui.core;
using ui.math;
using ui.mouse;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    public class MountToggleButton : Button<EmptyStore, MountToggleButton>
    {
        internal GroupComponent Parent;
        internal GroupComponentConfig Target;
        public MountToggleButton(string text = null) : base(text)
        {

        }

        public void MountToggle(GroupComponent parent, GroupComponentConfig target)
        {
            Parent = parent;
            Target = target;
        }

        public override void OnClick(ConsoleLocation loc)
        {
            // base.OnClick(loc);
            if (Target.Component.GetMount() is null)
            {
                Parent.Add(Target);
            }
            else
            {
                Target.Component.GetMount().RemoveChildComponent(Target.Component);
            }
        }
    }

    public static class MountingTest
    {
        public static void Setup()
        {
            SingleLineInputField field1;
            SingleLineInputField field2;
            GroupComponentConfig mountTest;
            MountToggleButton toggle;
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
                        (toggle = new MountToggleButton("Toggle Mount"), new Fraction(1, 4))
                    }, 1),
                    (mountTest = (new HorizontalGroupComponent {
                        (field2 = new SingleLineInputField(), new Fraction(3, 4)),
                        (new ExitButton("Confirm"), new Fraction(1, 4))
                    }, 1))
                }
            );
            toggle.MountToggle((VerticalGroupComponent)mountTest.Component.GetMount(), mountTest);
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
