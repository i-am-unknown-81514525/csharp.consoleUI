using System;

namespace ui.components.chainExt
{
    public static class PageSwitcherChain
    {
        public static T WithRequired<T>(this T v, Func<bool> required) where T : PageSwitcher
        {
            v.required = required;
            return v;
        }
    }
}
