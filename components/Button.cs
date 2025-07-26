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

        public override void onClick(ConsoleLocation loc)
        {
            if (activeHandler.getCurrActive() != null && activeHandler.getCurrActive() != this)
            {
                bool r = setActive(new ClickEvent(loc)); // Attempt to reset activity handler status
                if (r) setInactive();
            }
            onClickHandler(loc);
        }
    }
}
