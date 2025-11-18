using System;

namespace ui.components.chainExt
{
    public static class IComponentChain
    {
        public static T WithChange<T>(this T v, Action<T> func) where T : IComponent
        {
            func(v);
            return v;
        }

        public static T WithChange<T>(this T v, Func<T, T> func) where T : IComponent
        {
            return func(v);
        }

        public static T WithChange<TS, T>(this T v, Func<T, TS> func) where T : IComponent
        {
            func(v);
            return v;
        }
    }
}
