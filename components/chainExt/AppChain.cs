using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class AppChain
    {
        public static T WithTickHandler<S, T>(this T v, Action<App<S, T>> onTickHandler) where S : ComponentStore where T : App<S, T>
        {
            v.onTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<S, T>(this T v, Action onTickHandler) where S : ComponentStore where T : App<S, T>
        {
            v.onTickHandler = (_) => onTickHandler();
            return v;
        }

        public static T WithExitHandler<S, T>(this T v, Action<App<S, T>> onExitHandler) where S : ComponentStore where T : App<S, T>
        {
            v.onExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<S, T>(this T v, Action onExitHandler) where S : ComponentStore where T : App<S, T>
        {
            v.onTickHandler = (_) => onExitHandler();
            return v;
        }

        public static T WithTickHandler<T>(this T v, Action<App<EmptyStore, T>> onTickHandler) where T : App<EmptyStore, T>
        {
            v.onTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<T>(this T v, Action onTickHandler) where T : App<EmptyStore, T>
        {
            v.onTickHandler = (_) => onTickHandler();
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action<App<EmptyStore, T>> onExitHandler) where T : App<EmptyStore, T>
        {
            v.onExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action onExitHandler) where T : App<EmptyStore, T>
        {
            v.onTickHandler = (_) => onExitHandler();
            return v;
        }
    }
}
