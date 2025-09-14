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

        public PageSwitcher(Switcher switcher, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            onClickHandler = (_, __) =>
            {
                if (required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, Func<bool> required, string content, int page) : base(content)
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, Func<bool> required, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, string content, Func<bool> required, int page) : base(content)
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(Switcher switcher, int page, Func<bool> required, string content = null) : base(content ?? $"Go To Page {page}")
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.SwitchTo(page);
                }
            };
        }


        public PageSwitcher(ComponentHolder<Switcher> switcher, string content, int page) : base(content)
        {
            onClickHandler = (_, __) =>
            {
                if (required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            onClickHandler = (_, __) =>
            {
                if (required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, Func<bool> required, string content, int page) : base(content)
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, Func<bool> required, int page, string content = null) : base(content ?? $"Go To Page {page}")
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, string content, Func<bool> required, int page) : base(content)
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }

        public PageSwitcher(ComponentHolder<Switcher> switcher, int page, Func<bool> required, string content = null) : base(content ?? $"Go To Page {page}")
        {
            this.required = required;
            onClickHandler = (_, __) =>
            {
                if (this.required())
                {
                    switcher.inner.SwitchTo(page);
                }
            };
        }
    }
}
