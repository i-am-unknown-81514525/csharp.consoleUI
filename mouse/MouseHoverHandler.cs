using System;
using System.Linq;
using ui;
using ui.components;
using ui.core;

namespace ui.mouse
{
    public class MouseHoverHandler : MouseInteractionHandler
    {
        public readonly App app;
        public MouseHoverHandler(App app) : base((int)MouseOpCode.HOVER) {
            this.app = app;
        }

        public override void onActive(int opCode, int col, int row)
        {
            ConsoleLocation loc = new ConsoleLocation(row, col);
            app.onHover(loc);
        }

        public override void onInactive(int opCode, int col, int row) { }
    }
}