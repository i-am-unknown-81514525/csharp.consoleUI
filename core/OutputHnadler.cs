using System;

namespace ui.core
{
    public struct ConsoleSize
    {
        public readonly int Height;
        public readonly int Width;

        public ConsoleSize(int height, int width)
        {
            Height = height;
            Width = width;
        }
    }

    public struct ConsoleContent {
        public string content {get; set;}
        public string ansiPrefix {get; set;}
        public string ansiPostfix {get; set;}

        public new string ToString() => (ansiPrefix ?? "") + content.ToString() + (ansiPostfix ?? "");

    }

    public class ConsoleCanva
    {
        private ConsoleSize _size;
        public ConsoleContent[,] ConsoleWindow = new ConsoleContent[120,80];

        public void EventLoopPre() // Any handling required before the higher level abstraction
        {
            _size = new ConsoleSize(Console.BufferHeight, Console.BufferWidth);
            ConsoleContent[,] newWindow = new ConsoleContent[Console.BufferHeight, Console.BufferWidth];
            for (int x = 0; x < ConsoleWindow.GetLength(0) && x < newWindow.GetLength(0); x++)
            {
                for (int y = 0; y < ConsoleWindow.GetLength(1) && y < newWindow.GetLength(1); y++)
                {
                    newWindow[x,y] = ConsoleWindow[x,y];
                }
            }
            for (int x = ConsoleWindow.GetLength(0); x < newWindow.GetLength(0); x++)
            {
                for (int y = ConsoleWindow.GetLength(1); y < newWindow.GetLength(1); y++)
                {
                    newWindow[x,y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = "",
                        ansiPostfix = ""
                    };
                }
            }
        }

        internal string GetContent()
        {
            string prefix = (
                //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?1049h") + // Enable alternative buffer
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?25l") + // Hide cursor
                //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("2J") + // Clear screen and move cursor to top left (WIndow ANSI.sys)
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
                outputBuffer +=  ConsoleHandler.ConsoleIntermediateHandler.ToANSI($"{y + 1};0H");
                for (int x = 1; x <= ConsoleWindow.GetLength(0); x++)
                {
                    outputBuffer += ConsoleWindow[x-1,y-1].ToString();
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