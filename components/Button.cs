using System;
using ui.core;
using ui.events;
using ui.fmt;

namespace ui.components
{
    public class Button<TS> : Button<TS, Button<TS>> where TS : ComponentStore
    {
        public Button(string text = null) : base(text) { }
        public Button(ComponentConfig config, string text = null) : base(config, text) { }
    }

    public class Button : Button<EmptyStore, Button>
    {
        public Button(string text = null) : base(text) { }
        public Button(ComponentConfig config, string text = null) : base(config, text) { }
    }

    public class Button<TS, T> : TextLabel<TS> where T : Button<TS, T> where TS : ComponentStore
    {

        // Not require component update

        public Action<Button<TS, T>, ConsoleLocation> OnClickHandler = (_, __) => { };

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

        public Button(TS store, string text = null) : base(store, text)
        {
            SetDefault();
        }

        public Button(ComponentConfig config, TS store, string text = null) : base(config, store, text)
        {
            SetDefault();
        }

        public override void OnClick(ConsoleLocation loc)
        {
            if (ActiveHandler.GetCurrActive() != null && ActiveHandler.GetCurrActive() != this)
            {
                bool r = SetActive(new ClickEvent(loc)); // Attempt to reset activity handler status
                if (r) Deactive(null);
                Debug.DebugStore.Append($"Attempt to deactivate handler, success: {r}");
            }
            OnClickHandler(this, loc);
        }
    }
}
