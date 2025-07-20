using System;
using System.Linq;
using ui;
using ui.components;
using ui.core;

namespace ui.mouse
{
    public class MouseClickHandler : MouseInteractionHandler
    {
        public readonly App app;
        public MouseClickHandler(App app) : base((int)MouseOpCode.CLICK) {
            this.app = app;
        }

        public override void onActive(int opCode, int col, int row)
        {
            ConsoleLocation loc = new ConsoleLocation(row, col);
            app.onClick(loc);
        }

        public override void onInactive(int opCode, int col, int row) { } // Dragging is not a planned feature, and therefore isn't implemented
    }
}