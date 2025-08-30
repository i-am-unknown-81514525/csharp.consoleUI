using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class ButtonChain
    {


        public static T WithHandler<S, T>(this T v, Action<Button<S, T>, ConsoleLocation> onClickHandler) where T : Button<S, T> where S : ComponentStore
        {
            v.onClickHandler = onClickHandler;
            return v;
        }

        public static T WithHandler<S, T>(this T v, Action<ConsoleLocation> onClickHandler) where T : Button<S, T> where S : ComponentStore
        {
            v.onClickHandler = (_, loc) => onClickHandler(loc);
            return v;
        }

        public static T WithHandler<S, T, R>(this T v, Func<Button<S, T>, ConsoleLocation, R> onClickHandler) where T : Button<S, T> where S : ComponentStore
        {
            v.onClickHandler = (b, loc) => { onClickHandler(b, loc); };
            return v;
        }

        public static T WithHandler<S, T, R>(this T v, Func<ConsoleLocation, R> onClickHandler) where T : Button<S, T> where S : ComponentStore
        {
            v.onClickHandler = (_, loc) => { onClickHandler(loc); };
            return v;
        }
    }
}
