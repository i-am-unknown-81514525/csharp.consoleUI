using System;
using System.Text;

namespace ui.core
{
    public struct ConsoleContent
    {
        public string content { get; set; }
        public string ansiPrefix { get; set; }
        public string ansiPostfix { get; set; }
        public bool isContent { get; set; }
        public new string ToString() => (ansiPrefix ?? "") + (content ?? " ") + (ansiPostfix ?? "");

        public StringBuilder AppendToStringBuilder(StringBuilder builder)
        {
            builder.Append(ansiPrefix ?? "");
            builder.Append(content ?? "");
            builder.Append(ansiPostfix ?? "");
            return builder;
        }

        public static ConsoleContent getDefault()
        {
            return new ConsoleContent
            {
                content = " ",
                ansiPrefix = "",
                ansiPostfix = "",
                isContent = false
            };
        }
    }

    public class ConsoleCanva
    {
        private ConsoleSize _size = new ConsoleSize(60, 40);

        private ConsoleSize _minSize = new ConsoleSize(60, 40);
        public ConsoleContent[,] ConsoleWindow = new ConsoleContent[120, 80];

        private ConsoleContent[,] previous = null;
        public (int row, int col)? cursorPosition = null;

        protected void applyToNew((int width, int height) size)
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
                    newWindow[x, y] = ConsoleContent.getDefault();
                }
            }
            for (int x = previous.GetLength(0); x < newWindow.GetLength(0); x++)
            {
                for (int y = 0; y < newWindow.GetLength(1); y++)
                {
                    newWindow[x, y] = ConsoleContent.getDefault();
                }
            }
            ConsoleWindow = newWindow;
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

        public void SetEmpty()
        {
            for (int x = 0; x < ConsoleWindow.GetLength(0); x++)
            {
                for (int y = 0; y < ConsoleWindow.GetLength(1); y++)
                {
                    ConsoleWindow[x, y] = ConsoleContent.getDefault();
                }
            }
        }

        public void EventLoopPre() // Any handling required before the higher level abstraction
        {
            _size = new ConsoleSize(Console.BufferWidth, Console.BufferHeight);
            previous = ConsoleWindow;
            applyToNew((Console.BufferWidth, Console.BufferHeight));
        }

        public string GetContent()
        {
            StringBuilder builder = new StringBuilder(65536);
            string prefix = (
                //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?1049h") + // Enable alternative buffer
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?25l") + // Hide cursor
                                                                           // ConsoleHandler.ConsoleIntermediateHandler.ToANSI("2J") + // Clear screen and move cursor to top left (Window ANSI.sys)
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour                                                                                                                                           //ConsoleHandler.ConsoleIntermediateHandler.ToANSI("3J") + // Clear screen and delete all lines saved in the scrollback buffer (xterm alive)
                                                                         // ConsoleHandler.ConsoleIntermediateHandler.ToANSI("1;39m") + // Set colour to default (according to ANSI)
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0;0H") + // Move cursor to 0,0 (top left)
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("37;40m") // Set default colour
            );
            builder.Append(prefix);
            string postfix = (
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0m") + // Reset colour
                ConsoleHandler.ConsoleIntermediateHandler.ToANSI("0;0H") // Move cursor to 0,0 (top left)
            );
            if (!(cursorPosition is null))
            {
                (int row, int col) v = cursorPosition.GetValueOrDefault();
                postfix += ConsoleHandler.ConsoleIntermediateHandler.ToANSI($"{v.row};{v.col}H");
                postfix += ConsoleHandler.ConsoleIntermediateHandler.ToANSI("?25h"); // Show cursor
            }
            // string outputBuffer = "";
            for (int y = 1; y <= ConsoleWindow.GetLength(1); y++)
            {
                builder.Append(ConsoleHandler.ConsoleIntermediateHandler.ToANSI($"{y};0H"));
                for (int x = 1; x <= ConsoleWindow.GetLength(0); x++)
                {
                    ConsoleWindow[x - 1, y - 1].AppendToStringBuilder(builder);
                    // builder.Append(ConsoleWindow[x - 1, y - 1].ToString());
                }
            }
            builder.Append(postfix);
            return builder.ToString();
        }

        public ConsoleSize GetConsoleSize()
        {
            return this._size;
        }

        public void EventLoopPost()
        {
            Console.Write(GetContent());
        }

        public static ConsoleCanva OverwriteOnCanva(ConsoleCanva canva, ConsoleContent[,] data, (int x, int y) topLeft, bool force = true)
        { // When force, it would write to the content that fit instead of raising error

            int minX = topLeft.x;
            int minY = topLeft.y;
            int maxX = topLeft.x + data.GetLength(0);
            int maxY = topLeft.y + data.GetLength(1);
            if (force)
            {
                if (maxX > canva.ConsoleWindow.GetLength(0))
                {
                    maxX = canva.ConsoleWindow.GetLength(0);
                }
                if (maxY > canva.ConsoleWindow.GetLength(1))
                {
                    maxY = canva.ConsoleWindow.GetLength(1);
                }
                if (minX < 0) minX = 0;
                if (minY < 0) minY = 0;
            }
            else
            {
                if (maxX > canva.ConsoleWindow.GetLength(0))
                {
                    throw new InvalidOperationException($"To write on canva, with offsetX={topLeft.x} and sizeX={data.GetLength(0)} => maxX={maxX} but limX={canva.ConsoleWindow.GetLength(0)}");
                }
                if (maxY > canva.ConsoleWindow.GetLength(1))
                {
                    throw new InvalidOperationException($"To write on canva, with offsetX={topLeft.y} and sizeX={data.GetLength(1)} => maxX={maxY} but limX={canva.ConsoleWindow.GetLength(1)}");
                }
                if (minX < 0) throw new InvalidOperationException($"To write on canva, minX={minX} but limX=0");
                if (minY < 0) throw new InvalidOperationException($"To write on canva, minY={minY} but limY=0");
            }

            for (int x = minX; x < maxX; x++)
            {
                for (int y = minY; y < maxY; y++)
                {
                    if (data[x - minX, y - minY].isContent)
                    {
                        ConsoleContent content = data[x - minX, y - minY];

                        canva.ConsoleWindow[x, y] = content;
                    }
                }
            }

            return canva;
        }

        public static ConsoleCanva WriteStringOnCanva(ConsoleCanva canva, string text, (int x, int y) topLeft, string ansiPrefix = "", string ansiPostfix = "")
        {
            if (string.IsNullOrEmpty(text)) return canva;
            return OverwriteOnCanva(canva, getContentArr(text, ansiPrefix, ansiPostfix), topLeft, true);
        }

        public static ConsoleContent[,] getContentArr(string text, string ansiPrefix = "", string ansiPostfix = "")
        {
            if (string.IsNullOrEmpty(text)) text = "";
            ConsoleContent[,] data = new ConsoleContent[text.Length, 1];
            for (int i = 0; i < text.Length; i++)
            {
                data[i, 0] = new ConsoleContent()
                {
                    content = text[i].ToString(),
                    ansiPrefix = ansiPrefix,
                    ansiPostfix = ansiPostfix,
                    isContent = true
                };
            }

            return data;
        }

        public static ConsoleContent[] getContentArr1D(string text, string ansiPrefix = "", string ansiPostfix = "")
        {
            if (string.IsNullOrEmpty(text)) text = "";
            ConsoleContent[] data = new ConsoleContent[text.Length];
            for (int i = 0; i < text.Length; i++)
            {
                data[i] = new ConsoleContent()
                {
                    content = text[i].ToString(),
                    ansiPrefix = ansiPrefix,
                    ansiPostfix = ansiPostfix,
                    isContent = true
                };
            }

            return data;
        }
    }
}
