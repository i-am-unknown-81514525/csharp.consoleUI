using System;
using ui.core;

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

        public static T WithChange<S, T>(this T v, Func<T, S> func) where T : IComponent
        {
            func(v);
            return v;
        }
    }
}
