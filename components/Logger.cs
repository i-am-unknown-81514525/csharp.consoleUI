using System;
using System.Collections.Generic;
using ui.core;
using ui.fmt;
using ui.utils;

namespace ui.components
{
    public class Logger : TextLabel
    {
        private List<string> _history = new List<string>();

        public Logger()
        {
            vAlign = VerticalAlignment.TOP;
            hAlign = HorizontalAlignment.LEFT;
            foreground = ForegroundColorEnum.RED;
        }

        public string GetStrRender()
        {
            uint y = GetAllocSize().y;
            int start = (int)(_history.Count - y);
            if (start < 0) start = 0;
            List<string> forRender = new List<string>();
            for (int iy = start; (iy - start) < y && iy < _history.Count; iy++)
            {
                forRender.Add(_history[iy]);
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
                _history.Add(content);
                InternalUpdate();
            }
        }
    }
}
