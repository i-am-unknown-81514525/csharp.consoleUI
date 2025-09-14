namespace ui.components
{
    class PageSwitcher : Button
    {
        public PageSwitcher(Switcher switcher, string content, int page) : base(content)
        {
            onClickHandler = (_, __) => switcher.SwitchTo(page);
        }

        public PageSwitcher(Switcher switcher, int page) : base($"Go To Page {page}")
        {
            onClickHandler = (_, __) => switcher.SwitchTo(page);
        }
    }
}
