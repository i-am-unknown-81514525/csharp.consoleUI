using System;
using ui.core;

namespace ui.components.chainExt
{
    public static class ITableChain
    {
        public static T WithComponentConstructor<T>(this T v, Func<IComponent> constructor) where T : ITable
        {
            return v.WithComponentConstructor((_, __) => constructor());
        }

        public static T WithComponentConstructor<T>(this T v, Func<(int, int), IComponent> constructor) where T : ITable
        {
            return v.WithComponentConstructor((_, s) => constructor(s));
        }

        public static T WithComponentConstructor<T>(this T v, Func<T, IComponent> constructor) where T : ITable
        {
            return v.WithComponentConstructor((table, _) => constructor(table));
        }

        public static T WithComponentConstructor<T>(this T v, Func<T, (int, int), IComponent> constructor) where T : ITable
        {
            for (int x = 0; x < v.GetSize().x; x++)
            {
                for (int y = 0; y < v.GetSize().y; y++)
                {
                    v[x, y] = constructor(v, (x, y));
                }
            }
            return v;
        }
    }
}
