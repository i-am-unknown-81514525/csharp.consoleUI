using System;

namespace ui.core
{
    public struct ConsoleLocation
    {

        public readonly int X; // x
        public readonly int Y; // y

        public ConsoleLocation(int x, int y)
        {
            if (x < 0 || y < 0) throw new InvalidOperationException("Cannot set negative size");
            this.Y = y;
            this.X = x;
        }

        public bool Validate() => X > 0 && Y > 0; // The difference is this function check for if it can be displayed

        public static bool operator ==(ConsoleLocation left, ConsoleLocation right) => left.X == right.X && left.Y == right.Y;
        public static bool operator !=(ConsoleLocation left, ConsoleLocation right) => !(left == right);
        public override bool Equals(object obj)
        {
            if (!(obj is ConsoleLocation)) return false;
            return this == (ConsoleLocation)obj;
        }

        public override int GetHashCode() => (X << 16) + Y;
    }
}