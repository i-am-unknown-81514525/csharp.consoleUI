using System.Collections.Generic;

namespace ui.components.chainExt
{
    public static class SwitcherChain
    {
        public static T With<S, T>(this T v, IComponent component) where T : Switcher<S, T> where S : ComponentStore
        {
            v.Add(component);
            return v;
        }

        public static T With<S, T>(this T v, IEnumerable<IComponent> components) where T : Switcher<S, T> where S : ComponentStore
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
