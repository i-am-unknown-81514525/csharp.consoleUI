using System;
using ui.core;
using ui.events;
using ui.fmt;

namespace ui.components
{
    public class Button : TextLabel
    {

        // Not require component update

        public Action<ConsoleLocation> onClickHandler = (_) => { };

        public Button(string text = null) : base(text)
        {
            foreground = ForegroundColorEnum.BLACK;
            background = BackgroundColorEnum.WHITE;
        }

        public override void OnClick(ConsoleLocation loc)
        {
            if (activeHandler.GetCurrActive() != null && activeHandler.GetCurrActive() != this)
            {
                bool r = SetActive(new ClickEvent(loc)); // Attempt to reset activity handler status
                if (r) Deactive(null);
                DEBUG.DebugStore.Append($"Attempt to deactivate handler, success: {r}");
            }
            onClickHandler(loc);
        }
    }
}
