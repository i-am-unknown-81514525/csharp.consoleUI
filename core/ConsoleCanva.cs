using System;

namespace ui.core
{
    public struct ConsoleContent
    {
        public string content { get; set; }
        public string ansiPrefix { get; set; }
        public string ansiPostfix { get; set; }

        public new string ToString() => (ansiPrefix ?? "") + content.ToString() + (ansiPostfix ?? "");
        

    }

    public class ConsoleCanva
    {
        private ConsoleSize _size = new ConsoleSize(60, 40);

        private ConsoleSize _minSize = new ConsoleSize(60, 40);
        public ConsoleContent[,] ConsoleWindow = new ConsoleContent[120, 80];

        private ConsoleContent[,] previous = null;

        internal void applyToNew((int width, int height) size)
        {
            ConsoleContent[,] newWindow = new ConsoleContent[size.width, size.height];
            for (int x = 0; x < previous.GetLength(0) && x < newWindow.GetLength(0); x++)
            {
                for (int y = 0; y < previous.GetLength(1) && y < newWindow.GetLength(1); y++)
                {
                    newWindow[x, y] = previous[x, y];
                }
                for (int y = previous.GetLength(1); y < newWindow.GetLength(1); y++)
                {
                    newWindow[x, y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = "",
                        ansiPostfix = ""
                    };
                }
            }
            for (int x = previous.GetLength(0); x < newWindow.GetLength(0); x++)
            {
                for (int y = 0; y < newWindow.GetLength(1); y++)
                {
                    newWindow[x, y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = "",
                        ansiPostfix = ""
                    };
                }
            }
        }

        public void SetMin(ConsoleSize size)
        {
            ConsoleSize MIN_SIZE = new ConsoleSize(5, 10);
            if (!(size < MIN_SIZE))
            {
                throw new InvalidOperationException("The minimal expected size is way too small");
            }
            _minSize = size;
        }

        public void SetSize(ConsoleSize size)
        {
            if (!(size > _minSize))
            {
                throw new InvalidOperationException("The defined console size is too small");
            }
            applyToNew((size.Width, size.Height));
        }

        public void EventLoopPre() // Any handling required before the higher level abstraction
        {
            _size = new ConsoleSize(Console.BufferWidth, Console.BufferHeight);
            previous = ConsoleWindow;
            applyToNew((Console.BufferWidth, Console.BufferHeight));
        }

        internal string GetContent()
        {
            string prefix = (
                //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?1049h") + // Enable alternative buffer
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?25l") + // Hide cursor
                                                                           //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("2J") + // Clear screen and move cursor to top left (Window ANSI.sys)
                                                                           //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("3J") + // CLearn screen and delete all lines saved in the scrollback buffer (xterm alive)
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("1;39m") + // Set colour to default (according to ANSI)
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0;0H") // Move cursor to 0,0 (top left)
            );
            string postfix = (
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0;0H") // Move cursor to 0,0 (top left)
            );
            string outputBuffer = "";
            for (int y = 1; y <= ConsoleWindow.GetLength(1); y++)
            {
                outputBuffer += ConsoleHandler.ConsoleIntermediateHandler.ToANSI($"{y + 1};0H");
                for (int x = 1; x <= ConsoleWindow.GetLength(0); x++)
                {
                    outputBuffer += ConsoleWindow[x - 1, y - 1].ToString();
                }
            }
            return prefix + outputBuffer + postfix;
        }

        public ConsoleSize GetConsoleSize()
        {
            return this._size;
        }

        public void EventLoopPost()
        {
            Console.Write(GetContent());
        }
    }
}