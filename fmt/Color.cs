using System;

namespace ui.fmt
{
    public abstract class Color : AnsiFmtBase
    {
        public Color(string fmtString) : base(fmtString)
        {
        }
    }

    public class ForegroundColor : Color
    {
        public ForegroundColor(ForegroundColorEnum fEnum) : base(((int)fEnum).ToString())
        {
        }

        public ForegroundColor(byte c256) : base($"38;5;{c256}")
        {
        }

        public ForegroundColor(int r, int g, int b) : base($"38;2;{r};{g};{b}")
        {
        }

        public static implicit operator ForegroundColor(ForegroundColorEnum fEnum) => new ForegroundColor(fEnum);
    }

    public enum ForegroundColorEnum : int
    {
        BLACK = 30,
        RED = 31,
        GREEN = 32,
        YELLOW = 33,
        BLUE = 34,
        MAGENTA = 35,
        CYAN = 36,
        WHITE = 37,
        DEFAULT = 38,
        LIB_DEFAULT = 37,
    }

    public class BackgroundColor : Color
    {
        public BackgroundColor(BackgroundColorEnum bEnum) : base(((int)bEnum).ToString())
        {
        }

        public BackgroundColor(byte c256) : base($"48;5;{c256}")
        {
        }

        public BackgroundColor(int r, int g, int b) : base($"48;2;{r};{g};{b}")
        {
        }

        public static implicit operator BackgroundColor(BackgroundColorEnum bEnum) => new BackgroundColor(bEnum);
    }

    public enum BackgroundColorEnum : int
    {
        BLACK = 40,
        RED = 41,
        GREEN = 42,
        YELLOW = 43,
        BLUE = 44,
        MAGENTA = 45,
        CYAN = 46,
        WHITE = 47,
        DEFAULT = 48,
        LIB_DEFAULT = 40,
    }
}