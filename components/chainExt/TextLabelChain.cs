using ui.fmt;
using ui.utils;

namespace ui.components.chainExt
{
    public static class TextLabelChain
    {
        public static T WithForeground<TS, T>(this T v, ForegroundColor foreground) where T : TextLabel<TS> where TS : ComponentStore
        {
            v.foreground = foreground;
            return v;
        }

        public static T WithBackground<TS, T>(this T v, BackgroundColor background) where T : TextLabel<TS> where TS : ComponentStore
        {
            v.background = background;
            return v;
        }

        public static T WithVAlign<TS, T>(this T v, VerticalAlignment vAlign) where T : TextLabel<TS> where TS : ComponentStore
        {
            v.vAlign = vAlign;
            return v;
        }

        public static T WithHAlign<TS, T>(this T v, HorizontalAlignment hAlign) where T : TextLabel<TS> where TS : ComponentStore
        {
            v.hAlign = hAlign;
            return v;
        }
    }
}
