using System.Collections.Generic;
using System;
using ui.core;
using ui.utils;
using ui.fmt;

namespace ui.components
{
    public class Logger : TextLabel
    {
        private List<string> history = new List<string>();

        public Logger() : base()
        {
            vAlign = VerticalAlignment.TOP;
            hAlign = HorizontalAlignment.LEFT;
            foreground = ForegroundColorEnum.RED;
        }

        public string GetStrRender()
        {
            uint y = GetAllocSize().y;
            int start = (int)(history.Count - y);
            if (start < 0) start = 0;
            List<string> forRender = new List<string>();
            for (int iy = start; (iy - start) < y && iy < history.Count; iy++)
            {
                forRender.Add(history[iy]);
            }
            return String.Join("\n", forRender);
        }

        protected void InternalUpdate()
        {
            string result = GetStrRender();
            if (result != text)
            {
                text = result;
            }
        }

        protected override ConsoleContent[,] RenderPre(ConsoleContent[,] src)
        {
            InternalUpdate();
            return base.RenderPre(src);
        }

        public void Push(string content)
        {
            if (!(content is null))
            {
                history.Add(content);
                InternalUpdate();
            }
        }
    }
}
