using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class ButtonChain
    {


        public static T WithHandler<T>(this T v, Action<Button<T>, ConsoleLocation> onClickHandler) where T : Button<T>
        {
            v.onClickHandler = onClickHandler;
            return v;
        }

        public static T WithHandler<T>(this T v, Action<ConsoleLocation> onClickHandler) where T : Button<T>
        {
            v.onClickHandler = (_, loc) => onClickHandler(loc);
            return v;
        }

        public static T WithHandler<T, R>(this T v, Func<Button<T>, ConsoleLocation, R> onClickHandler) where T : Button<T>
        {
            v.onClickHandler = (b, loc) => { onClickHandler(b, loc); };
            return v;
        }

        public static T WithHandler<T, R>(this T v, Func<ConsoleLocation, R> onClickHandler) where T : Button<T>
        {
            v.onClickHandler = (_, loc) => { onClickHandler(loc); };
            return v;
        }
    }
}
