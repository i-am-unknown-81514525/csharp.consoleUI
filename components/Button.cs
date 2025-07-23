using System;
using ui.core;
using ui.fmt;

namespace ui.components {
    public class Button : Component
    {
        // Require component update
        private string _text;
        private ForegroundColor _fore = new ForegroundColor(ForegroundColorEnum.BLACK);
        private BackgroundColor _back = new BackgroundColor(BackgroundColorEnum.WHITE);
        
        // Not require component update

        public Action<ConsoleLocation> onClickHandler = (_) => { };



        public Button(ComponentConfig config, string text = null) : base(config)
        {
            _text = text;
        }

        protected ConsoleContent[,] RenderSelf()
        {
            (uint x, uint y) = this.GetAllocSize();
            ConsoleContent[,] content = new ConsoleContent[x, y];
            if (x == 0 || y == 0) return content;
            uint midY = y / 2;
            uint offsetX = (x - (uint)text.Length) / 2;
            for (uint ix = 0; ix < x; ix++)
            {
                for (uint iy = 0; iy < y; iy++)
                {
                    if (iy == midY && ix >= offsetX && ix < offsetX + (uint)text.Length)
                    {
                        content[ix, iy] = new ConsoleContent
                        {
                            content = text[(int)ix - (int)offsetX].ToString(),
                            ansiPrefix = TextFormatter.Constructor(foreground, background),
                            ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                            isContent = true
                        };
                    }
                    else
                    {
                        content[ix, iy] = ConsoleContent.getDefault();
                    }
                }
            }
            return content;
        }

        public string text
        {
            get => _text;
            set
            {
                _text = value;
                SetHasUpdate();
            }
        }

        public ForegroundColor foreground
        {
            get => _fore;
            set
            {
                _fore = value;
                SetHasUpdate();
            }
        }
        public BackgroundColor background
        {
            get => _back;
            set
            {
                _back = value;
                SetHasUpdate();
            }
        }

        protected override void onResize()
        {
            throw new System.NotImplementedException();
        }

        public override void onClick(ConsoleLocation loc)
        {
            onClickHandler(loc);
        }
    }
}