using ui.core;
using ui.fmt;

namespace ui.components
{
    public class HorizontalBar : TextLabel<EmptyStore>
    {
        // vAlign is ignored

        protected string DisplayChar;

        public HorizontalBar(char displayChar = ' ') : base()
        {
            DisplayChar = displayChar.ToString();
        }
        public HorizontalBar(ComponentConfig config, char displayChar = ' ') : base(config)
        {
            DisplayChar = displayChar.ToString();
        }

        protected override ConsoleContent[,] RenderSelf()
        {
            ConsoleContent[,] content = new ConsoleContent[GetAllocSize().x, GetAllocSize().y];
            uint placeY = 0;
            switch (vAlign)
            {
                case utils.VerticalAlignment.TOP:
                    placeY = 0;
                    break;
                case utils.VerticalAlignment.MIDDLE:
                    placeY = GetAllocSize().y / 2;
                    break;
                case utils.VerticalAlignment.BOTTOM:
                    placeY = GetAllocSize().y - 1;
                    break;
            }
            for (int y = 0; y < GetAllocSize().y; y++)
            {
                string placeChar = " ";
                if (y == placeY)
                {
                    placeChar = DisplayChar;
                }
                for (int x = 0; x < GetAllocSize().x; x++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = placeChar,
                        isContent = true,
                        ansiPrefix = TextColorFormatter.Constructor(foreground, background),
                        ansiPostfix = TextColorFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT)
                    };
                }
            }
            return content;
        }
    }

    public class VerticalBar : TextLabel<EmptyStore>
    {
        // vAlign is ignored

        protected string DisplayChar;

        public VerticalBar(char displayChar = ' ') : base()
        {
            DisplayChar = displayChar.ToString();
        }
        public VerticalBar(ComponentConfig config, char displayChar = ' ') : base(config)
        {
            DisplayChar = displayChar.ToString();
        }

        protected override ConsoleContent[,] RenderSelf()
        {
            ConsoleContent[,] content = new ConsoleContent[GetAllocSize().x, GetAllocSize().y];
            uint placeX = 0;
            switch (hAlign)
            {
                case utils.HorizontalAlignment.LEFT:
                    placeX = 0;
                    break;
                case utils.HorizontalAlignment.MIDDLE:
                    placeX = GetAllocSize().x / 2;
                    break;
                case utils.HorizontalAlignment.RIGHT:
                    placeX = GetAllocSize().x - 1;
                    break;
            }
            for (int x = 0; x < GetAllocSize().x; x++)
            {
                string placeChar = " ";
                if (x == placeX)
                {
                    placeChar = DisplayChar;
                }
                for (int y = 0; y < GetAllocSize().y; y++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = placeChar,
                        isContent = true,
                        ansiPrefix = TextColorFormatter.Constructor(foreground, background),
                        ansiPostfix = TextColorFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT)
                    };
                }
            }
            return content;
        }
    }
}
