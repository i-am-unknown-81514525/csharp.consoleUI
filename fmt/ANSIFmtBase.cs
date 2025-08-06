using System;

namespace ui.fmt
{
    public abstract class ANSIFmtBase : IComparable<ANSIFmtBase>
    {
        public readonly string fmtString;


        public ANSIFmtBase(string fmtString)
        {
            this.fmtString = fmtString;
        }

        public override string ToString() => fmtString;

        public int CompareTo(ANSIFmtBase other)
        {
            return this.fmtString.CompareTo(other.fmtString);
        }

        public static bool operator ==(ANSIFmtBase left, ANSIFmtBase right) => left.fmtString == right.fmtString;
        public static bool operator !=(ANSIFmtBase left, ANSIFmtBase right) => !(left == right);
        public override bool Equals(object obj)
        {
            if (!(obj is ANSIFmtBase)) return false;
            return this == (ANSIFmtBase)obj;
        }

        public override int GetHashCode()
        {
            return this.fmtString.GetHashCode();
        }
    }
}
