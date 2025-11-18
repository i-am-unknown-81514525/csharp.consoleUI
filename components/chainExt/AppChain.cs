using System;

namespace ui.components.chainExt
{
    public static class AppChain
    {
        public static T WithTickHandler<TS, T>(this T v, Action<App<TS, T>> onTickHandler) where TS : ComponentStore where T : App<TS, T>
        {
            v.OnTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<TS, T>(this T v, Action onTickHandler) where TS : ComponentStore where T : App<TS, T>
        {
            v.OnTickHandler = _ => onTickHandler();
            return v;
        }

        public static T WithExitHandler<TS, T>(this T v, Action<App<TS, T>> onExitHandler) where TS : ComponentStore where T : App<TS, T>
        {
            v.OnExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<TS, T>(this T v, Action onExitHandler) where TS : ComponentStore where T : App<TS, T>
        {
            v.OnTickHandler = _ => onExitHandler();
            return v;
        }

        public static T WithTickHandler<T>(this T v, Action<App<EmptyStore, T>> onTickHandler) where T : App<EmptyStore, T>
        {
            v.OnTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<T>(this T v, Action onTickHandler) where T : App<EmptyStore, T>
        {
            v.OnTickHandler = _ => onTickHandler();
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action<App<EmptyStore, T>> onExitHandler) where T : App<EmptyStore, T>
        {
            v.OnExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action onExitHandler) where T : App<EmptyStore, T>
        {
            v.OnTickHandler = _ => onExitHandler();
            return v;
        }
    }
}
