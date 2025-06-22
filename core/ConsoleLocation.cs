using System;

namespace ui.core
{
    public struct ConsoleLocation
    {

        public readonly int x; // x
        public readonly int y; // y

        public ConsoleLocation(int x, int y)
        {
            if (x < 0 || y < 0) throw new InvalidOperationException("Cannot set negative size");
            this.y = y;
            this.x = x;
        }

        public bool Validate() => x > 0 && y > 0; // The difference is this function check for if it can be displayed

        public static bool operator ==(ConsoleLocation left, ConsoleLocation right) => left.x == right.x && left.y == right.y;
        public static bool operator !=(ConsoleLocation left, ConsoleLocation right) => !(left == right);
        public override bool Equals(object obj)
        {
            if (!(obj is ConsoleLocation)) return false;
            return this == (ConsoleLocation)obj;
        }

        public override int GetHashCode() => (x << 16) + y;
    }
}