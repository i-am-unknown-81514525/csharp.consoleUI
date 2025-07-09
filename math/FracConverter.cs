using System;
using System.Nunerics;
using ui;
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
        public static Fraction ParseInteger(string integer)
        {
            if (!RegexChecker.IsInteger(integer))
                throw new FractionFormatException(integer);
            return new Fraction(BigInteger.Parse(integer), 1);
        }

        public static Fraction ParseDecimal(string decimalValue)
        {
            if (!RegexChecker.IsDecimal(decimalValue))
                throw new FractionFormatException(decimalValue);
            string left = decimalValue.Split('.')[0];
            string right = decimalValue.Split('.')[1];
            int rCount = right.Length;
            return new Fraction(BigInteger.Parse(left + right), BigInteger.Parse("1" + new string('0', rCount)));
        }

        public static Fraction ParseNumber(string number)
        {
            if (DEBUG.FracConverter_AlternativeLowerScopeCheck && !RegexChecker.IsNumber(number) && (RegexChecker.IsDecimal(number) || RegexChecker.IsInteger(number)))
            {
                throw new NotSupportedException($"Regex Error: {number} is not being consider as number but have been consider as one of decimal or integer");
            }
            if (!RegexChecker.IsNumber(number))
                throw new FractionFormatException(number);
            if (RegexChecker.IsDecimal(number)) return ParseDecimal(number);
            if (RegexChecker.IsInteger(number)) return ParseInteger(number);
            throw new NotSupportedException($"Regex Error: {number} is being consider as number but have not been consider as either of decimal or integer");
        }

        public static Fraction ParseFraction(string fraction)
        {
            if (!RegexChecker.IsFraction(fraction))
                throw new FractionFormatException(fraction);
            string[] lr = fraction.Split('/');
            string left = lr[0];
            string right = lr[1];
            return ParseNumber(left) / ParseNumber(right);
        }

        public static Fraction Parse(string value)
        {
            if (DEBUG.FracConverter_AlternativeLowerScopeCheck && !RegexChecker.IsNumber(number) && (RegexChecker.IsFraction(number) || RegexChecker.IsNumber(number)))
            {
                throw new NotSupportedException($"Regex Error: {number} is not being consider as parsable but have been consider as one of fraction or number");
            }
            if (!RegexChecker.IsFracOrNum(value))
                throw new FractionFormatException(value);
            if (RegexChecker.IsFraction(number)) return ParseFraction(number);
            if (RegexChecker.IsNumber(number)) return ParseNumber(number);
            throw new NotSupportedException($"Regex Error: {number} is being consider as parsable but have not been consider as either of fraction or number");
            
        }
    }
}