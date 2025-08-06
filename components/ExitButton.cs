using ui.core;

namespace ui.components
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
}
