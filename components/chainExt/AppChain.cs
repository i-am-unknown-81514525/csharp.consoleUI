using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class AppChain
    {
        public static T WithTickHandler<S, T>(this T v, Action<App<S, T>> onTickHandler) where T : App<S, T> where S : ComponentStore
        {
            v.onTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<S, T>(this T v, Action onTickHandler) where T : App<S, T> where S : ComponentStore
        {
            v.onTickHandler = (_) => onTickHandler();
            return v;
        }

        public static T WithExitHandler<S, T>(this T v, Action<App<S, T>> onExitHandler) where T : App<S, T> where S : ComponentStore
        {
            v.onExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<S, T>(this T v, Action onExitHandler) where T : App<S, T> where S : ComponentStore
        {
            v.onTickHandler = (_) => onExitHandler();
            return v;
        }
    }
}
