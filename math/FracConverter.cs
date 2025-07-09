using System;
using ui.utils;

namespace ui.math
{
    public class FractionFormatException : FormatException
    {
        public FractionFormatException() : base("Invalid format string at ui.math.Fraction") { }
        public FractionFormatException(string value) : base($"Invalid format string of {integer} at ui.math.Fraction") { }
    }

    public static class FracConverter
    {
        public Fraction ParseInteger(string integer)
        {
            if (!RegexChecker.IsInteger(integer))
                throw FractionFormatException(integer);
            return new Fraction(BigInteger.Parse(integer), 1);
        }

        public Fraction ParseDecimal(string decimalValue)
        {
            if (!RegexChecker.IsDecimal(decimalValue))
                throw FractionFormatException(decimalValue);
            string left = decimalValue.Split('.')[0];
            string right = decimalValue.Split('.')[1];
            int rCount = right.Length;
            return new Fraction(BigInteger.Parse(left + right), BigInteger.Parse("1"+new string('0', rCount)));
        }
    }
}