using System;

namespace ui.fmt
{
    public abstract class AnsiFmtBase : IComparable<AnsiFmtBase>
    {
        public readonly string FmtString;


        public AnsiFmtBase(string fmtString)
        {
            this.FmtString = fmtString;
        }

        public override string ToString() => FmtString;

        public int CompareTo(AnsiFmtBase other)
        {
            return this.FmtString.CompareTo(other.FmtString);
        }

        public static bool operator ==(AnsiFmtBase left, AnsiFmtBase right) => left.FmtString == right.FmtString;
        public static bool operator !=(AnsiFmtBase left, AnsiFmtBase right) => !(left == right);
        public override bool Equals(object obj)
        {
            if (!(obj is AnsiFmtBase)) return false;
            return this == (AnsiFmtBase)obj;
        }

        public override int GetHashCode()
        {
            return this.FmtString.GetHashCode();
        }
    }
}
