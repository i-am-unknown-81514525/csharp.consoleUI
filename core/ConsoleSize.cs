using System;

namespace ui.core
{
    public struct ConsoleSize
    {

        public readonly int Width; // x
        public readonly int Height; // y

        public ConsoleSize(int width, int height)
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

        public override int GetHashCode() => (Height << 16) + Width;
    }
}