using System;

namespace ui.fmt
{
    public static class TextFormatter
    {
        public static string Constructor(ForegroundColor foregroundColor)
        {
            string outStr = foregroundColor.ToString();
            return $"\x1b[{outStr}m";
        }
        public static string Constructor(BackgroundColor backgroundColor)
        {
            string outStr = backgroundColor.ToString();
            return $"\x1b[{outStr}m";
        }
        public static string Constructor(ForegroundColor foregroundColor, BackgroundColor backgroundColor)
        {
            string outStr = $"{foregroundColor};{backgroundColor}";
            return $"\x1b[{outStr}m";
        }

        public static string Constructor(BackgroundColor backgroundColor, ForegroundColor foregroundColor)
        {
            return Constructor(foregroundColor, backgroundColor);
        }

        public static string Constructor((ForegroundColor foreground, BackgroundColor background) color)
        {
            return Constructor(color.foreground, color.background);
        }

        public static string Constructor((BackgroundColor background, ForegroundColor foreground) color)
        {
            return Constructor(color.foreground, color.background);
        }
    }
}
