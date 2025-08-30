using ui.fmt;
using ui.utils;

namespace ui.components.chainExt
{
    public static class TextLabelChain
    {
        public static T WithForeground<S, T>(this T v, ForegroundColor foreground) where T : TextLabel<S> where S : ComponentStore
        {
            v.foreground = foreground;
            return v;
        }

        public static T WithBackground<S, T>(this T v, BackgroundColor background) where T : TextLabel<S> where S : ComponentStore
        {
            v.background = background;
            return v;
        }

        public static T WithVAlign<S, T>(this T v, VerticalAlignment vAlign) where T : TextLabel<S> where S : ComponentStore
        {
            v.vAlign = vAlign;
            return v;
        }

        public static T WithHAlign<S, T>(this T v, HorizontalAlignment hAlign) where T : TextLabel<S> where S : ComponentStore
        {
            v.hAlign = hAlign;
            return v;
        }
    }
}
