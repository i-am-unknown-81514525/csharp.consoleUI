using System;

namespace ui.components
{
    public class PageSwitcher : Button
    {

        public Func<bool> Required = () => true;

        public PageSwitcher(Switcher switcher, string content, int page) : base(content)
        {
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, Func<bool> required, string content, int page) : base(content)
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, Func<bool> required, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, string content, Func<bool> required, int page) : base(content)
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, int page, Func<bool> required, string content = null) : base(content ?? $"Go To Page {page}")
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }


        public PageSwitcher(ComponentHolder<Switcher> switcher, string content, int page) : base(content)
        {
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, Func<bool> required, string content, int page) : base(content)
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, Func<bool> required, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, string content, Func<bool> required, int page) : base(content)
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, int page, Func<bool> required, string content = null) : base(content ?? $"Go To Page {page}")
        {
            Required = required;
            OnClickHandler = (_, __) =>
            {
                if (Required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }
    }
}
