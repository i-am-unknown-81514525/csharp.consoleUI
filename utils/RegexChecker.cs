using System.IO;
using System.Text.RegularExpressions;

namespace ui.utils
{
    public static class RegexPattern
    {
        public const string Integer = @"^([+-]?([1-9][0-9]*)|0)$";
        public const string Decimal = @"^(([+-]?([1-9][0-9]*)\.[0-9]*[1-9])|([+-]?0\.[0-9]*[1-9]))$";
        public const string Number = @"^(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)$"; // DECIMAL OR INTEGER
        public const string Fraction = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))))$";
        public const string FractionOrNumber = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9])))|(([+-]?([1-9][0-9]*)(\.?[0-9]*[1-9])?)|([+-]?0(\.[0-9]*[1-9]))|0))$";

        public const string MouseActivityAnsiSeq = @"^\x1b\[\<(\d+);(\d+);(\d+)(M|m)$";
        public static readonly Regex CompiledMouseActivityAnsiSeq = new Regex(MouseActivityAnsiSeq, RegexOptions.Compiled);

    }

    public static class RegexChecker
    {
        public static bool IsInteger(string value)
        {
            Regex regex = new Regex(RegexPattern.Integer, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsDecimal(string value)
        {
            Regex regex = new Regex(RegexPattern.Decimal, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsNumber(string value)
        {
            Regex regex = new Regex(RegexPattern.Number, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsFraction(string value)
        {
            Regex regex = new Regex(RegexPattern.Fraction, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsFracOrNum(string value)
        {
            Regex regex = new Regex(RegexPattern.FractionOrNumber, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static bool IsMouseActivityAnsiSeq(string value)
        {
            Regex regex = RegexPattern.CompiledMouseActivityAnsiSeq; // new Regex(RegexPattern.MOUSE_ACTIVITY_ANSI_SEQ, RegexOptions.None);
            return regex.IsMatch(value);
        }

        public static (int opCode, int col, int row, bool isActive) RetrieveMouseActicity(string value)
        {
            if (!IsMouseActivityAnsiSeq(value)) throw new InvalidDataException("Not a valid ANSI sequence for mouse activity");
            Regex regex = RegexPattern.CompiledMouseActivityAnsiSeq;  //new Regex(RegexPattern.MOUSE_ACTIVITY_ANSI_SEQ, RegexOptions.None);
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