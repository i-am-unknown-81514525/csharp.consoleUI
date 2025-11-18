using System;
using ui.components;
using ui.components.chainExt;
using ui.math;

namespace ui.test
{
    public class TestInnerMenu : Container
    {
        public TestInnerMenu(Switcher switcher)
        {
            Add(
                new VerticalGroupComponent {
                    (new Frame().WithInner(new Button("Button 1")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new Button("Test table").WithHandler(_=>switcher.SwitchTo(2))), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new Button("Test multi-line input").WithHandler(_=>switcher.SwitchTo(1))), new Fraction(1, 4)),
                    // (new Frame().WithInner(new Button("Button 3")), new Fraction(1, 4)),
                    (new Padding(), 1),
                    (new Frame().WithInner(new ExitButton("Exit")), new Fraction(1, 4)),
                }
            );
        }
    }


    public class TestOuterMenu : Container
    {
        public TestOuterMenu(Switcher switcher)
        {
            Add(
                new Frame(
                    (new TextLabel("Test Menu"), 9)
                ).WithInner(
                    new VerticalGroupComponent {
                        (new Padding(), new Fraction(3, 8)),
                        (
                            new HorizontalGroupComponent {
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
                new IComponent[] {
                    new TestOuterMenu(switcher),
                    new VerticalGroupComponent {
                        new MultiLineInputField(),
                        (new Button("Back")
                            .WithHandler(
                                _=>switcher.SwitchTo(0)
                            ), 1)
                    },
                    new VerticalGroupComponent {
                        (
                            new FormattedTable(
                                (5, 5)
                            )
                        ).WithComponentConstructor(
                            () => new SingleLineInputField().WithChange(c => c.underline = false)
                        ),
                        (new Button("Back")
                            .WithHandler(
                                _=>switcher.SwitchTo(0)
                            ), 1)
                    }
                }
            );
            app.WithExitHandler(appObj =>
            {
                Console.WriteLine(appObj.Debug_WriteStructure());
                Console.WriteLine(Debug.DebugStore.ToString());
            }).Run();
        }
    }
}
