using System;
using ui.core;
using ui.events;
using ui.fmt;

namespace ui.components
{
    public class Button<S> : Button<S, Button<S>> where S : ComponentStore
    {
        public Button(string text = null) : base(text) { }
        public Button(ComponentConfig config, string text = null) : base(config, text) { }
    }

    public class Button : Button<EmptyStore, Button>
    {
        public Button(string text = null) : base(text) { }
        public Button(ComponentConfig config, string text = null) : base(config, text) { }
    }

    public class Button<S, T> : TextLabel<S> where T : Button<S, T> where S : ComponentStore
    {

        // Not require component update

        public Action<Button<S, T>, ConsoleLocation> onClickHandler = (_, __) => { };

        public void SetDefault()
        {
            foreground = ForegroundColorEnum.BLACK;
            background = BackgroundColorEnum.WHITE;
        }

        public Button(string text = null) : base(text)
        {
            SetDefault();
        }

        public Button(ComponentConfig config, string text = null) : base(config, text)
        {
            SetDefault();
        }

        public Button(S store, string text = null) : base(store, text)
        {
            SetDefault();
        }

        public Button(ComponentConfig config, S store, string text = null) : base(config, store, text)
        {
            SetDefault();
        }

        public override void OnClick(ConsoleLocation loc)
        {
            if (activeHandler.GetCurrActive() != null && activeHandler.GetCurrActive() != this)
            {
                bool r = SetActive(new ClickEvent(loc)); // Attempt to reset activity handler status
                if (r) Deactive(null);
                DEBUG.DebugStore.Append($"Attempt to deactivate handler, success: {r}");
            }
            onClickHandler(this, loc);
        }
    }
}
