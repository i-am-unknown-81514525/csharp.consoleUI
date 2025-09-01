using ui.core;
using ui.fmt;

namespace ui.components
{
    public class HorizontalBar : TextLabel<EmptyStore>
    {
        // vAlign is ignored

        protected string displayChar;

        public HorizontalBar(char displayChar = ' ') : base()
        {
            this.displayChar = displayChar.ToString();
        }
        public HorizontalBar(ComponentConfig config, char displayChar = ' ') : base(config)
        {
            this.displayChar = displayChar.ToString();
        }

        protected override ConsoleContent[,] RenderSelf()
        {
            ConsoleContent[,] content = new ConsoleContent[GetAllocSize().x, GetAllocSize().y];
            uint place_y = 0;
            switch (vAlign)
            {
                case utils.VerticalAlignment.TOP:
                    place_y = 0;
                    break;
                case utils.VerticalAlignment.MIDDLE:
                    place_y = GetAllocSize().y / 2;
                    break;
                case utils.VerticalAlignment.BOTTOM:
                    place_y = GetAllocSize().y - 1;
                    break;
            }
            for (int y = 0; y < GetAllocSize().y; y++)
            {
                string placeChar = " ";
                if (y == place_y)
                {
                    placeChar = displayChar;
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

        protected string displayChar;

        public VerticalBar(char displayChar = ' ') : base()
        {
            this.displayChar = displayChar.ToString();
        }
        public VerticalBar(ComponentConfig config, char displayChar = ' ') : base(config)
        {
            this.displayChar = displayChar.ToString();
        }

        protected override ConsoleContent[,] RenderSelf()
        {
            ConsoleContent[,] content = new ConsoleContent[GetAllocSize().x, GetAllocSize().y];
            uint place_x = 0;
            switch (hAlign)
            {
                case utils.HorizontalAlignment.LEFT:
                    place_x = 0;
                    break;
                case utils.HorizontalAlignment.MIDDLE:
                    place_x = GetAllocSize().x / 2;
                    break;
                case utils.HorizontalAlignment.RIGHT:
                    place_x = GetAllocSize().x - 1;
                    break;
            }
            for (int x = 0; x < GetAllocSize().x; x++)
            {
                string placeChar = " ";
                if (x == place_x)
                {
                    placeChar = displayChar;
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
