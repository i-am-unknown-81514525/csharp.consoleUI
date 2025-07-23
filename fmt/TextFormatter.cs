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
    }
}