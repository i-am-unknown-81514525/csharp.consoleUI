using System;
using ui.components;
using ui.components.chainExt;
using ui.math;
using ui.fmt;

namespace ui.test
{
    public static class FrameTest
    {
        public static void Setup()
        {
            Switcher switcher = null;
            int idx = 0;
            new App(
                switcher = new Switcher()
                {
                    new VerticalGroupComponent() {
                        (
                            new Frame()
                                .WithInner(new Button("Switch")
                                    .WithHandler(
                                        (loc) => {
                                            idx++;
                                            idx %= 2;
                                            switcher.SwitchTo(idx);
                                        }
                                    )
                                ).WithTitle(
                                    (new TextLabel("Switch button"), 13)
                                ),
                            new Fraction(3, 4)
                        ),
                        (
                            new Frame()
                                .WithInner(
                                    new ExitButton("End")
                                ).WithTitle(
                                    (new TextLabel("Exit button"), 11)
                                ),
                            new Fraction(1, 4)
                        )
                    },
                    new Frame()
                        .WithInner(new Button("Switch")
                            .WithHandler(
                                (loc) => {
                                    idx++;
                                    idx %= 2;
                                    switcher.SwitchTo(idx);
                                }
                            )
                        ).WithTitle(
                            (new TextLabel("Switch button"), 13)
                        )
                }
            ).WithExitHandler(
                (app) =>
                {
                    Console.WriteLine(app.Debug_WriteStructure());
                }
            ).Run();
        }
    }
}
