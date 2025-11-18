using System;
using System.Linq;
using ui;
using ui.components;
using ui.core;

namespace ui.mouse
{
    public class MouseHoverHandler : MouseInteractionHandler
    {
        public readonly IComponent App;
        public MouseHoverHandler(IComponent app) : base((int)MouseOpCode.HOVER)
        {
            App = app;
        }

        public override void OnActive(int opCode, ConsoleLocation loc)
        {
            App.OnHover(loc);
        }

        public override void OnInactive(int opCode, ConsoleLocation loc) { }
    }
}
