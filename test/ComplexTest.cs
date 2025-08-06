using System;
using ui.components;
using ui.components.chainExt;
using ui.math;

namespace ui.test
{
    public class TestInnerMenu : Container
    {
        public TestInnerMenu(Switcher switcher) : base()
        {
            Add(
                new VerticalGroupComponent() {
                    (new Frame().WithInner(new Button("Button 1")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new Button("Button 2")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new SingleLineInputField()), 3),
                    // (new Frame().WithInner(new Button("Button 3")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new ExitButton("Exit")), new Fraction(1, 4)),
                }
            );
        }
    }


    public class TestOuterMenu : Container
    {
        public TestOuterMenu(Switcher switcher) : base()
        {
            Add(
                new Frame(
                    (new TextLabel("Test Menu"), 9)
                ).WithInner(
                    new VerticalGroupComponent() {
                        (new Padding(), new Fraction(3, 8)),
                        (
                            new HorizontalGroupComponent() {
                                (new Padding(), new Fraction(1, 3)),
                                (new TestInnerMenu(switcher), new Fraction(1, 3)),
                                (new Padding(), new Fraction(1, 3))
                            },
                            19 // new Fraction(3, 8)
                        ),
                        (new Padding(), new Fraction(2, 8))
                    }
                )
            );
        }
    }

    public static class ComplexTest
    {
        public static void Setup()
        {
            Switcher switcher = new Switcher();
            App app = new App(
                switcher
            );
            switcher.AddMulti(
                new[] {
                    new TestOuterMenu(switcher)
                }
            );
            app.WithExitHandler((appObj) =>
            {
                Console.WriteLine(appObj.Debug_WriteStructure());
                Console.WriteLine(ui.DEBUG.DebugStore.ToString());
            }).Run();
        }
    }
}
