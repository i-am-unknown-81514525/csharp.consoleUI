using System.Collections.Generic;

namespace ui.fmt
{
    public static class TextColorFormatter
    {
        private static Dictionary<ForegroundColor, string> _foreCache = new Dictionary<ForegroundColor, string>();
        private static Dictionary<BackgroundColor, string> _backCache = new Dictionary<BackgroundColor, string>();
        private static Dictionary<(ForegroundColor, BackgroundColor), string> _forebackCache = new Dictionary<(ForegroundColor, BackgroundColor), string>();

        public static string Constructor(ForegroundColor foregroundColor)
        {
            if (_foreCache.ContainsKey(foregroundColor)) return _foreCache[foregroundColor];
            string outStr = foregroundColor.ToString();
            return _foreCache[foregroundColor] = $"\x1b[{outStr}m";
        }
        public static string Constructor(BackgroundColor backgroundColor)
        {
            if (_backCache.ContainsKey(backgroundColor)) return _backCache[backgroundColor];
            string outStr = backgroundColor.ToString();
            return _backCache[backgroundColor] = $"\x1b[{outStr}m";
        }
        public static string Constructor(ForegroundColor foregroundColor, BackgroundColor backgroundColor)
        {
            if (_forebackCache.ContainsKey((foregroundColor, backgroundColor))) return _forebackCache[(foregroundColor, backgroundColor)];
            string outStr = $"{foregroundColor};{backgroundColor}";
            return _forebackCache[(foregroundColor, backgroundColor)] = $"\x1b[{outStr}m";
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
        private static Dictionary<EnableStyle, string> _enCache = new Dictionary<EnableStyle, string>();
        private static Dictionary<DisableStyle, string> _diCache = new Dictionary<DisableStyle, string>();
        private static Dictionary<(EnableStyle, DisableStyle), string> _bothCache = new Dictionary<(EnableStyle, DisableStyle), string>();

        public static string Constructor(EnableStyle enable)
        {
            if (_enCache.ContainsKey(enable)) return _enCache[enable];
            string outStr = enable.ToString();
            return _enCache[enable] = $"\x1b[{outStr}m";
        }
        public static string Constructor(DisableStyle disable)
        {
            if (_diCache.ContainsKey(disable)) return _diCache[disable];
            string outStr = disable.ToString();
            return _diCache[disable] = $"\x1b[{outStr}m";
        }
        public static string Constructor(EnableStyle enable, DisableStyle disable)
        {
            if (_bothCache.ContainsKey((enable, disable))) return _bothCache[(enable, disable)];
            string outStr = $"{enable};{disable}";
            return _bothCache[(enable, disable)] = $"\x1b[{outStr}m";
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
