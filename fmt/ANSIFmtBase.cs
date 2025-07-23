namespace ui.fmt
{
    public abstract class ANSIFmtBase
    {
        public readonly string fmtString;


        public ANSIFmtBase(string fmtString)
        {
            this.fmtString = fmtString;
        }

        public override string ToString() => fmtString;
    }
}