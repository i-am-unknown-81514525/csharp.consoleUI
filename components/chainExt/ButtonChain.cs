using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class ButtonChain
    {
        public static T WithHandler<T>(this T v, Action<ConsoleLocation> onClickHandler) where T : Button
        {
            v.onClickHandler = onClickHandler;
            return v;
        }

    }
}
