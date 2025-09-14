using System;

namespace ui.components
{
    public class PageSwitcher : Button
    {

        public Func<bool> required = () => true;

        public PageSwitcher(Switcher switcher, string content, int page) : base(content)
        {
            onClickHandler = (_, __) =>
            {
                if (required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, int page) : base($"Go To Page {page}")
        {
            onClickHandler = (_, __) =>
            {
                if (required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }
    }
}
