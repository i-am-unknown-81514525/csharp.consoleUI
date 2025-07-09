using System.Text.RegularExpressions;

namespace ui.utils
{
    public static class RegexPattern
    {
        public const string INTEGER = @"^([+-]?([1-9][0-9]*)|0)$";
        public const string DECIMAL = @"^(([+-]?([1-9][0-9]*)\.[0-9]+)|([+-]?0\.[0-9]+))$";
        public const string NUMBER = @"^(([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0)$"; // DECIMAL OR INTEGER
        public const string FRACTION = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0))$";
        public const string FRACTION_OR_NUMBER = @"^((([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0)\/(([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0)|(([+-]?([1-9][0-9]*)(\.?[0-9]+)?)|([+-]?0(\.[0-9]+))|0))$";

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
    }
}