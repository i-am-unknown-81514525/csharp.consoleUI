using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class AppChain
    {


        public static T WithTickHandler<T>(this T v, Action<App<T>> onTickHandler) where T : App<T>
        {
            v.onTickHandler = onTickHandler;
            return v;
        }

        public static T WithTickHandler<T>(this T v, Action onTickHandler) where T : App<T>
        {
            v.onTickHandler = (_) => onTickHandler();
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action<App<T>> onExitHandler) where T : App<T>
        {
            v.onExitHandler = onExitHandler;
            return v;
        }

        public static T WithExitHandler<T>(this T v, Action onExitHandler) where T : App<T>
        {
            v.onTickHandler = (_) => onExitHandler();
            return v;
        }
    }
}
