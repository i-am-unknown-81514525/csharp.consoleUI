using System;

namespace ui.core
{
    public struct ConsoleSize
    {
        public readonly int Height;
        public readonly int Width;

        public ConsoleSize(int height, int width)
        {
            if (height < 0 || width < 0) throw new InvalidOperationException("Cannot set negative size");
            Height = height;
            Width = width;
        }

        public bool Validate() => Height > 0 && Width > 0; // The difference is this function check for if it can be displayed


        public static bool operator >(ConsoleSize left, ConsoleSize right) => left.Height > right.Height && left.Width > right.Width;
        public static bool operator <(ConsoleSize left, ConsoleSize right) => left.Height < right.Height && left.Width < right.Width;
        public static bool operator ==(ConsoleSize left, ConsoleSize right) => left.Height == right.Height && left.Width == right.Width;
        public static bool operator !=(ConsoleSize left, ConsoleSize right) => !(left == right);
        public static bool operator >=(ConsoleSize left, ConsoleSize right) => left > right || left == right;
        public static bool operator <=(ConsoleSize left, ConsoleSize right) => left < right || left == right;
        public override bool Equals(object obj)
        {
            if (!(obj is ConsoleSize)) return false;
            return this == (ConsoleSize)obj;
        }




    }

    public struct ConsoleContent
    {
        public string content { get; set; }
        public string ansiPrefix { get; set; }
        public string ansiPostfix { get; set; }

        public new string ToString() => (ansiPrefix ?? "") + content.ToString() + (ansiPostfix ?? "");
        

    }

    public class ConsoleCanva
    {
        private ConsoleSize _size;

        private ConsoleSize _minSize = new ConsoleSize(60, 40);
        public ConsoleContent[,] ConsoleWindow = new ConsoleContent[120, 80];

        private ConsoleContent[,] previous = null;

        internal void applyToNew((int height, int width) size)
        {
            ConsoleContent[,] newWindow = new ConsoleContent[size.height, size.width];
            for (int x = 0; x < previous.GetLength(0) && x < newWindow.GetLength(0); x++)
            {
                for (int y = 0; y < previous.GetLength(1) && y < newWindow.GetLength(1); y++)
                {
                    newWindow[x, y] = previous[x, y];
                }
            }
            for (int x = previous.GetLength(0); x < newWindow.GetLength(0); x++)
            {
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
        }

        public void SetMin(ConsoleSize size)
        {
            ConsoleSize MIN_SIZE = new ConsoleSize(5, 10);
            if (!(size < MIN_SIZE))
            {
                throw new InvalidOperationException("The minimal expected size is way too small");
            }
            this._minSize = size;
        }

        public void SetSize(ConsoleSize size)
        {
            if (!(size > _minSize))
            {
                throw new InvalidOperationException("The defined console size is too small");
            }
            this.applyToNew((size.Height, size.Width));
        }

        public void EventLoopPre() // Any handling required before the higher level abstraction
        {
            _size = new ConsoleSize(Console.BufferHeight, Console.BufferWidth);
            previous = ConsoleWindow;
            applyToNew((Console.BufferHeight, Console.BufferWidth));
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

        public void EventLoopPost()
        {
            Console.Write(this.GetContent());
        }
    }
}