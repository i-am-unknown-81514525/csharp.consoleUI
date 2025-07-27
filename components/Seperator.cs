using ui.fmt;
using ui.core;

namespace ui.components
{
    public class Seperator : Component
    {
        //Reactive of foreground with type ForegroundColor and default value: `ForegroundColorEnum.WHITE`, Trigger: SetHasUpdate();
        private ForegroundColor _foreground = ForegroundColorEnum.WHITE;
        public ForegroundColor foreground { get => _foreground; set { _foreground = value; SetHasUpdate(); } }

        //Reactive of background with type BackgroundColor and default value: `BackgroundColorEnum.BLACK`, Trigger: SetHasUpdate();
        private BackgroundColor _background = BackgroundColorEnum.BLACK;
        public BackgroundColor background { get => _background; set { _background = value; SetHasUpdate(); } }

        protected virtual ConsoleContent[,] RenderSelf()
        {
            (uint x, uint y) = this.GetAllocSize();
            ConsoleContent[,] content = new ConsoleContent[x, y];
            ConsoleContent item = new ConsoleContent()
            {
                content = " ",
                ansiPrefix = TextFormatter.Constructor(foreground, background),
                ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                isContent = true
            };
            if (x == 0 || y == 0) return content;
            for (uint ix = 0; ix < x; ix++)
            {
                for (uint iy = 0; iy < y; iy++)
                {
                    content[ix, iy] = item;
                }
            }
            return content;
        }


        protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        {
            return RenderSelf();
        }
    }
}
