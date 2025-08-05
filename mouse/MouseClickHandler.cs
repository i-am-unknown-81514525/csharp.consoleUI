using System;
using System.Linq;
using ui;
using ui.components;
using ui.core;
using ui.utils;

namespace ui.mouse
{
    public class MouseClickHandler : MouseInteractionHandler
    {
        public readonly IComponent app;
        public MouseClickHandler(IComponent app) : base((int)MouseOpCode.CLICK)
        {
            this.app = app;
        }

        public override void onActive(int opCode, ConsoleLocation loc)
        {
            app.OnClick(loc);
        }

        public override void onInactive(int opCode, ConsoleLocation loc) { } // Dragging is not a planned feature, and therefore isn't implemented
    }
}
