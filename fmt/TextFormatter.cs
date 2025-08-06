using System;
using System.Collections.Generic;

namespace ui.fmt
{
    public static class TextFormatter
    {
        private static Dictionary<ForegroundColor, string> foreCache = new Dictionary<ForegroundColor, string>();
        private static Dictionary<BackgroundColor, string> backCache = new Dictionary<BackgroundColor, string>();
        private static Dictionary<(ForegroundColor, BackgroundColor), string> forebackCache = new Dictionary<(ForegroundColor, BackgroundColor), string>();

        public static string Constructor(ForegroundColor foregroundColor)
        {
            if (foreCache.ContainsKey(foregroundColor)) return foreCache[foregroundColor];
            string outStr = foregroundColor.ToString();
            return foreCache[foregroundColor] = $"\x1b[{outStr}m";
        }
        public static string Constructor(BackgroundColor backgroundColor)
        {
            if (backCache.ContainsKey(backgroundColor)) return backCache[backgroundColor];
            string outStr = backgroundColor.ToString();
            return backCache[backgroundColor] = $"\x1b[{outStr}m";
        }
        public static string Constructor(ForegroundColor foregroundColor, BackgroundColor backgroundColor)
        {
            if (forebackCache.ContainsKey((foregroundColor, backgroundColor))) return forebackCache[(foregroundColor, backgroundColor)];
            string outStr = $"{foregroundColor};{backgroundColor}";
            return forebackCache[(foregroundColor, backgroundColor)] = $"\x1b[{outStr}m";
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
