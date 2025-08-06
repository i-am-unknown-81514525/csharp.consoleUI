using System;
using ui.core;
using ui.events;
using ui.fmt;
using ui.utils;

namespace ui.components
{
    public class TextLabel : NoChildComponent
    {
        //Reactive of text with type string, Trigger SetHasUpdate();
        private string _text;
        public string text { get => _text; set { _text = value; SetHasUpdate(); } }

        //Reactive of foreground with type ForegroundColor and default value: `ForegroundColorEnum.WHITE`, Trigger: SetHasUpdate();
        private ForegroundColor _foreground = ForegroundColorEnum.WHITE;
        public ForegroundColor foreground { get => _foreground; set { _foreground = value; SetHasUpdate(); } }

        //Reactive of background with type BackgroundColor and default value: `BackgroundColorEnum.BLACK`, Trigger: SetHasUpdate();
        private BackgroundColor _background = BackgroundColorEnum.BLACK;
        public BackgroundColor background { get => _background; set { _background = value; SetHasUpdate(); } }

        //Reactive of vAlign with type VerticalAlignment and default value: `VerticalAlignment.MIDDLE`, Trigger: SetHasUpdate();
        private VerticalAlignment _vAlign = VerticalAlignment.MIDDLE;
        public VerticalAlignment vAlign { get => _vAlign; set { _vAlign = value; SetHasUpdate(); } }

        //Reactive of hAlign with type HorizontalAlignment and default value: `HorizontalAlignment.MIDDLE`, Trigger: SetHasUpdate();
        private HorizontalAlignment _hAlign = HorizontalAlignment.MIDDLE;
        public HorizontalAlignment hAlign { get => _hAlign; set { _hAlign = value; SetHasUpdate(); } }


        public TextLabel(string text = null) : base()
        {
            _text = text ?? "";
        }

        public TextLabel(ComponentConfig config, string text = null) : base(config)
        {
            _text = text ?? "";
        }

        public TextLabel(string text, ComponentConfig config) : base(config)
        {
            _text = text ?? "";
        }

        protected virtual ConsoleContent[,] RenderSelf()
        {
            (uint x, uint y) = this.GetAllocSize();
            ConsoleContent[,] content = new ConsoleContent[x, y];
            if (x == 0 || y == 0) return content;
            string iContent = (text ?? "").Align((vAlign, (int)y), (hAlign, (int)x));
            string[] splitedContent = iContent.Split('\n');
            for (uint ix = 0; ix < x; ix++)
            {
                for (uint iy = 0; iy < y; iy++)
                {
                    content[ix, iy] = new ConsoleContent
                    {
                        content = splitedContent[iy][(int)ix].ToString(),
                        ansiPrefix = TextFormatter.Constructor(foreground, background),
                        ansiPostfix = TextFormatter.Constructor(ForegroundColorEnum.LIB_DEFAULT, BackgroundColorEnum.LIB_DEFAULT),
                        isContent = true
                    };
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
