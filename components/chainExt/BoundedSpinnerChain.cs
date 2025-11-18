using System;

namespace ui.components.chainExt
{
    public static class BoundedSpinnerChain
    {
        public static T WithChangeHandler<T>(this T v, Action<int> handler) where T : BoundedSpinner
        {
            v.OnChange = handler;
            return v;
        }

        public static T WithChangeHandler<T>(this T v, Action<T, int> handler) where T : BoundedSpinner
        {
            v.OnChange = i => handler(v, i);
            return v;
        }

        public static T WithTriggerChange<T>(this T v) where T : BoundedSpinner
        {
            v.OnChange(v.amount);
            return v;
        }
    }
}
