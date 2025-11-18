using ui.components;
using ui.core;

namespace ui.mouse
{
    public class MouseClickHandler : MouseInteractionHandler
    {
        public readonly IComponent App;
        public MouseClickHandler(IComponent app) : base((int)MouseOpCode.CLICK)
        {
            App = app;
        }

        public override void OnActive(int opCode, ConsoleLocation loc)
        {
            App.OnClick(loc);
        }

        public override void OnInactive(int opCode, ConsoleLocation loc) { } // Dragging is not a planned feature, and therefore isn't implemented
    }
}
