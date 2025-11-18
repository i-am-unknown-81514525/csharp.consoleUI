using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class ButtonChain
    {


        public static T WithHandler<TS, T>(this T v, Action<Button<TS, T>, ConsoleLocation> onClickHandler) where T : Button<TS, T> where TS : ComponentStore
        {
            v.OnClickHandler = onClickHandler;
            return v;
        }

        public static T WithHandler<TS, T>(this T v, Action<ConsoleLocation> onClickHandler) where T : Button<TS, T> where TS : ComponentStore
        {
            v.OnClickHandler = (_, loc) => onClickHandler(loc);
            return v;
        }

        public static T WithHandler<TS, T, TR>(this T v, Func<Button<TS, T>, ConsoleLocation, TR> onClickHandler) where T : Button<TS, T> where TS : ComponentStore
        {
            v.OnClickHandler = (b, loc) => { onClickHandler(b, loc); };
            return v;
        }

        public static T WithHandler<TS, T, TR>(this T v, Func<ConsoleLocation, TR> onClickHandler) where T : Button<TS, T> where TS : ComponentStore
        {
            v.OnClickHandler = (_, loc) => { onClickHandler(loc); };
            return v;
        }

        public static T WithHandler<T>(this T v, Action<Button<EmptyStore, T>, ConsoleLocation> onClickHandler) where T : Button<EmptyStore, T>
        {
            v.OnClickHandler = onClickHandler;
            return v;
        }

        public static T WithHandler<T>(this T v, Action<ConsoleLocation> onClickHandler) where T : Button<EmptyStore, T>
        {
            v.OnClickHandler = (_, loc) => onClickHandler(loc);
            return v;
        }

        public static T WithHandler<T, TR>(this T v, Func<Button<EmptyStore, T>, ConsoleLocation, TR> onClickHandler) where T : Button<EmptyStore, T>
        {
            v.OnClickHandler = (b, loc) => { onClickHandler(b, loc); };
            return v;
        }

        public static T WithHandler<T, TR>(this T v, Func<ConsoleLocation, TR> onClickHandler) where T : Button<EmptyStore, T>
        {
            v.OnClickHandler = (_, loc) => { onClickHandler(loc); };
            return v;
        }
    }
}
