using ui.fmt;
using ui.utils;

namespace ui.components.chainExt
{
    public static class TextLabelChain
    {
        public static T WithForeground<T>(this T v, ForegroundColor foreground) where T : TextLabel
        {
            v.foreground = foreground;
            return v;
        }

        public static T WithBackground<T>(this T v, BackgroundColor background) where T : TextLabel
        {
            v.background = background;
            return v;
        }

        public static T WithVAlign<T>(this T v, VerticalAlignment vAlign) where T : TextLabel
        {
            v.vAlign = vAlign;
            return v;
        }

        public static T WithHAlign<T>(this T v, HorizontalAlignment hAlign) where T : TextLabel
        {
            v.hAlign = hAlign;
            return v;
        }
    }
}
