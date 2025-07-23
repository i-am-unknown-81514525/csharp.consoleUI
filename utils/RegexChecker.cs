using System.IO;
using System.Text.RegularExpressions;

namespace ui.utils
{
    public static class RegexPattern
    {
        public const string INTEGER = @"^([+-]?([1-9][0-9]*)|0)$";
        public const string DECIMAL = @"^(([+-]?([1-9][0-9]*)\.[0-9]*[1-9])|([+-]?0\.[0-9]*[1-9]))$";
        public const string NUMBER = @"^(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)$"; // DECIMAL OR INTEGER
        public const string FRACTION = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))))$";
        public const string FRACTION_OR_NUMBER = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9])))|(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0))$";

        public const string MOUSE_ACTIVITY_ANSI_SEQ = @"^\x1b\[\<(\d+);(\d+);(\d+)(M|m)$";
        public static readonly Regex Compiled_Mouse_Activity_ANSI_Seq = new Regex(MOUSE_ACTIVITY_ANSI_SEQ, RegexOptions.Compiled);

    }

    public static class RegexChecker
    {
        public static bool IsInteger(string value)
        {
            Regex regex = new Regex(RegexPattern.INTEGER, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsDecimal(string value)
        {
            Regex regex = new Regex(RegexPattern.DECIMAL, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsNumber(string value)
        {
            Regex regex = new Regex(RegexPattern.NUMBER, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsFraction(string value)
        {
            Regex regex = new Regex(RegexPattern.FRACTION, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsFracOrNum(string value)
        {
            Regex regex = new Regex(RegexPattern.FRACTION_OR_NUMBER, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsMouseActivityANSISeq(string value)
        {
            Regex regex = RegexPattern.Compiled_Mouse_Activity_ANSI_Seq; // new Regex(RegexPattern.MOUSE_ACTIVITY_ANSI_SEQ, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static (int opCode, int col, int row, bool isActive) RetrieveMouseActicity(string value)
        {
            if (!IsMouseActivityANSISeq(value)) throw new InvalidDataException("Not a valid ANSI sequence for mouse activity");
            Regex regex = RegexPattern.Compiled_Mouse_Activity_ANSI_Seq;  //new Regex(RegexPattern.MOUSE_ACTIVITY_ANSI_SEQ, RegexOptions.None);
            Match match = regex.Match(value);
            return (
                int.Parse(match.Groups[1].Value),
                int.Parse(match.Groups[2].Value),
                int.Parse(match.Groups[3].Value),
                match.Groups[4].Value.Equals("M")
            );
        }
    }
}