using System;
using System.Collections.Generic;

namespace ui.fmt
{
    public static class TextColorFormatter
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
    public static class TextStyleFormatter
    {
        private static Dictionary<EnableStyle, string> enCache = new Dictionary<EnableStyle, string>();
        private static Dictionary<DisableStyle, string> diCache = new Dictionary<DisableStyle, string>();
        private static Dictionary<(EnableStyle, DisableStyle), string> bothCache = new Dictionary<(EnableStyle, DisableStyle), string>();

        public static string Constructor(EnableStyle enable)
        {
            if (enCache.ContainsKey(enable)) return enCache[enable];
            string outStr = enable.ToString();
            return enCache[enable] = $"\x1b[{outStr}m";
        }
        public static string Constructor(DisableStyle disable)
        {
            if (diCache.ContainsKey(disable)) return diCache[disable];
            string outStr = disable.ToString();
            return diCache[disable] = $"\x1b[{outStr}m";
        }
        public static string Constructor(EnableStyle enable, DisableStyle disable)
        {
            if (bothCache.ContainsKey((enable, disable))) return bothCache[(enable, disable)];
            string outStr = $"{enable};{disable}";
            return bothCache[(enable, disable)] = $"\x1b[{outStr}m";
        }

        public static string Constructor(DisableStyle disable, EnableStyle enable)
        {
            return Constructor(enable, disable);
        }

        public static string Constructor((EnableStyle enable, DisableStyle disable) style)
        {
            return Constructor(style.enable, style.disable);
        }

        public static string Constructor((DisableStyle disable, EnableStyle enable) style)
        {
            return Constructor(style.enable, style.disable);
        }
    }
}
