using System.Collections.Generic;

namespace ui.components.chainExt
{
    public static class SwitcherChain
    {
        public static T With<TS, T>(this T v, IComponent component) where T : Switcher<TS, T> where TS : ComponentStore
        {
            v.Add(component);
            return v;
        }

        public static T With<TS, T>(this T v, IEnumerable<IComponent> components) where T : Switcher<TS, T> where TS : ComponentStore
        {
            v.AddMulti(components);
            return v;
        }

        public static T With<T>(this T v, IComponent component) where T : Switcher<EmptyStore, T>
        {
            v.Add(component);
            return v;
        }

        public static T With<T>(this T v, IEnumerable<IComponent> components) where T : Switcher<EmptyStore, T>
        {
            v.AddMulti(components);
            return v;
        }
    }
}
