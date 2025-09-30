using System;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using ui.utils;
using ui.LatexExt;
using System.Text;

namespace ui.math
{
    public struct Fraction : IComparable<Fraction>, ILatex
    {
        public readonly BigInteger numerator, denominator;

        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            if (denominator < 0) { numerator = -numerator; denominator = -denominator; }
            BigInteger value = MathUtils.factorize(numerator, denominator);
            this.numerator = numerator / value;
            this.denominator = denominator / value;
        }

        public Fraction(long numerator, long denominator)
        {
            (this.numerator, this.denominator) = new Fraction((BigInteger)numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // long value = MathUtils.factorize(numerator, denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(BigInteger numerator, long denominator)
        {
            (this.numerator, this.denominator) = new Fraction((BigInteger)numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // BigInteger value = MathUtils.factorize(numerator, (BigInteger)denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(long numerator, BigInteger denominator)
        {
            (this.numerator, this.denominator) = new Fraction(numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // BigInteger value = MathUtils.factorize((BigInteger)numerator, denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(BigInteger value)
        {
            numerator = value;
            denominator = 1;
        }

        public Fraction(long value)
        {
            numerator = value;
            denominator = 1;
        }

        public Fraction(double value)
        {
            // -5.12 -> -5/1, -0.12 -> -10/2, -0.24 -> -20/4, -0.48
            // -40/8, -0.96 -> -80/16, -1.92 => -81/16, -0.92, -162/32, -1.84 => -163/32, -0.84
            // -> -326/64, -1.68 => -327/64, -0.68 -> -654/128, -1.36 => -655/128, -0.36 -> -1310/256, -0.72
            // -2620/512, -1.44 => -2621/512, -0.44 -> -5242/1024, -0.88 -> -10484/2048, -1.76 => -10485/2048, -0.76
            BigInteger numerator = (BigInteger)value;
            BigInteger denominator = 1;
            double remain = value % 1;
            int it = 0;
            while (remain != 0 && it < Config.Frac_DoubleInterpretPrecision)
            {
                remain *= 2;
                numerator <<= 1;
                denominator <<= 1;
                if (remain >= 1)
                {
                    remain -= 1;
                    numerator += 1;
                }
                if (remain <= -1)
                {
                    remain += 1;
                    numerator -= 1;
                }
                it += 1;
            }
            (this.numerator, this.denominator) = new Fraction(numerator, denominator);
        }

        public Fraction(string value)
        {
            Fraction f = Fraction.Parse(value);
            denominator = f.denominator;
            numerator = f.numerator;
        }

        public static implicit operator double(Fraction fraction)
        {
            return (double)fraction.numerator / (double)fraction.denominator;
        }

        public static implicit operator float(Fraction fraction)
        {
            return (float)((double)fraction.numerator / (double)fraction.denominator);
        }

        public static implicit operator (BigInteger numerator, BigInteger denominator)(Fraction fraction) => fraction.asTuple();

        public static implicit operator Fraction(uint value) => new Fraction((long)value);
        // Operator conversion precedence fix for the compiler, with AI advise used, fixed SplitHandler.Update (totalFrac > 1 || i == totalPrioityTier - 1)
        // Google Gemini 2.5 Flash via Gemini app, Accessed 24 July 2025, https://g.co/gemini/share/30770d6d34e9

        public static implicit operator Fraction(long value) => new Fraction(value);
        public static implicit operator Fraction(ulong value) => new Fraction((BigInteger)value);
        public static implicit operator Fraction(BigInteger value) => new Fraction(value);

        public void Deconstruct(out BigInteger numerator, out BigInteger denominator)
        {
            numerator = this.numerator;
            denominator = this.denominator;
        }

        public Fraction Simplify()
        {

            BigInteger value = MathUtils.factorize(numerator, denominator);
            if (numerator < 0 && denominator < 0)
            {
                value *= -1;
            }
            return new Fraction(numerator / value, denominator / value);
        }

        public Fraction Add(Fraction other)
        {
            // 3/4 + 5/8 = (3*8+5*4)/8*4, = 44/32 = 11/8
            BigInteger v1 = MathUtils.factorize(this.denominator, other.denominator);
            BigInteger denominator = this.denominator / v1 * other.denominator;
            BigInteger leftNum = denominator / this.denominator * this.numerator;
            BigInteger rightNum = denominator / other.denominator * other.numerator;
            BigInteger numerator = leftNum + rightNum;
            return new Fraction(numerator, denominator);
        }

        public Fraction asOpposeSign()
        {
            // Overflow safe
            if (denominator == long.MinValue && numerator == long.MinValue)
                return new Fraction(1, 1);
            else if (denominator == long.MinValue)
                return new Fraction(numerator, -denominator);
            return new Fraction(-numerator, denominator);
        }

        public Fraction Invert()
        {
            // Overflow safe
            return new Fraction(denominator, numerator);
        }

        public Fraction Subtract(Fraction other)
        {
            return Add(other.asOpposeSign());
        }

        public Fraction Multiply(Fraction other)
        {
            BigInteger v1 = MathUtils.factorize(this.numerator, other.denominator);
            BigInteger v2 = MathUtils.factorize(other.numerator, this.denominator);
            Fraction left = new Fraction(this.numerator / v1, this.denominator / v2);
            Fraction right = new Fraction(other.numerator / v2, other.denominator / v1);
            BigInteger numerator = left.numerator * right.numerator;
            BigInteger denominator = left.denominator * right.denominator;
            return new Fraction(numerator, denominator);
        }

        public Fraction Divide(Fraction other)
        {
            return Multiply(other.Invert());
        }

        public bool isBigInteger()
        {
            BigInteger v1 = MathUtils.factorize(numerator, denominator);
            return denominator == v1 || denominator == -v1 || numerator == 0;
        }

        public bool TryBigInteger(out BigInteger value)
        {
            value = 0;
            if (!isBigInteger())
                return false;
            BigInteger v1 = MathUtils.factorize(numerator, denominator);
            BigInteger num = numerator / v1;
            BigInteger deno = denominator / v1;
            value = num * deno; // deno for sign
            return true;
        }

        public bool isLong()
        {
            bool stats = TryBigInteger(out BigInteger value);
            if (!stats)
                return false;
            return value >= long.MinValue && value <= long.MaxValue;
        }

        public bool TryLong(out long value)
        {
            value = 0;
            if (!isLong())
                return false;
            TryBigInteger(out BigInteger v1);
            value = (long)v1;
            return true;
        }

        public bool isInteger()
        {
            bool stats = TryBigInteger(out BigInteger value);
            if (!stats)
                return false;
            return value >= int.MinValue && value <= int.MaxValue;
        }

        public bool TryInt(out int value)
        {
            value = 0;
            if (!isInteger())
                return false;
            TryBigInteger(out BigInteger v1);
            value = (int)v1;
            return true;
        }

        public BigInteger GetFloor()
        {
            return numerator / denominator;
        }

        public static Fraction operator +(Fraction left, Fraction right) => left.Add(right);
        public static Fraction operator +(Fraction left, long right) => left + new Fraction(right);
        public static Fraction operator +(long left, Fraction right) => new Fraction(left) + right;
        public static Fraction operator +(Fraction right) => right;
        public static Fraction operator -(Fraction left, Fraction right) => left.Subtract(right);
        public static Fraction operator -(Fraction left, long right) => left - new Fraction(right);
        public static Fraction operator -(long left, Fraction right) => new Fraction(left) - right;
        public static Fraction operator -(Fraction right) => right.asOpposeSign();
        public static Fraction operator *(Fraction left, Fraction right) => left.Multiply(right);
        public static Fraction operator *(Fraction left, long right) => left * new Fraction(right);
        public static Fraction operator *(long left, Fraction right) => new Fraction(left) * right;
        public static Fraction operator /(Fraction left, Fraction right) => left.Divide(right);
        public static Fraction operator /(Fraction left, long right) => left / new Fraction(right);
        public static Fraction operator /(long left, Fraction right) => new Fraction(left) / right;

        public static bool operator ==(Fraction left, Fraction right)
        {
            // if (right is null) return false;
            Fraction simLeft = left.Simplify();
            Fraction simRight = right.Simplify();
            return simLeft.numerator == simRight.numerator && simLeft.denominator == simRight.denominator;
        }

        public static bool operator !=(Fraction left, Fraction right) => !(left == right);
        public static bool operator <(Fraction left, Fraction right) => left.numerator * right.denominator < right.numerator * left.denominator;
        public static bool operator <=(Fraction left, Fraction right) => left < right || left == right;
        public static bool operator >(Fraction left, Fraction right) => !(left <= right);
        public static bool operator >=(Fraction left, Fraction right) => !(left < right);

        public override bool Equals(object obj)
        {
            if (!(obj is Fraction)) return false;
            return this == (Fraction)obj;
        }

        public override int GetHashCode()
        {
            int v = 2147483647;
            int c1 = (int)(numerator / v % v);
            int c2 = (int)(numerator % v);
            int c3 = (int)(denominator / v % v);
            int c4 = (int)(denominator % v);
            return c1 ^ c2 ^ c3 ^ c4;
        }

        public int CompareTo(Fraction right)
        {
            // if (this is null && right is null) return 0;
            // if (this is null) return -1;
            // if (right is null) return 1;
            if (this < right)
            {
                return -1;
            }
            if (this == right)
            {
                return 0;
            }
            return 1;
        }

        public override string ToString()
        {
            if (denominator == 1)
            {
                return $"{numerator.ToString()}";
            }
            return $"{numerator.ToString()}/{denominator.ToString()}";
        }

        public static Fraction Parse(string value)
        {
            return FracConverter.Parse(value);
        }

        public static bool TryParse(string value, out Fraction frac)
        {
            frac = new Fraction(0, 1);
            if (!RegexChecker.IsFracOrNum(value)) return false;
            frac = Fraction.Parse(value);
            return true;
        }

        public (BigInteger numerator, BigInteger denominator) asTuple() => (numerator, denominator);

        public string AsLatex() => denominator != 1 ? $"$\\frac{{{numerator}}}{{{denominator}}}$" : $"${numerator}$";

        public Fraction Abs() => this >= 0 ? this : -this;

        public (BigInteger integer, BigInteger length_0, BigInteger remaining) RepresentSigFig(BigInteger amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot have negative amount of significant figure");
            }
            bool have_initial = this.Abs() >= 1;
            BigInteger integer = numerator / denominator;
            if (this < 0)
            {
                integer--;
            }
            Fraction remaining = this - integer;
            if (remaining < 0)
            {
                throw new InvalidOperationException("Fraction:RepresentSigFig idk how it give negative remaining");
            }
            if (amount < integer.Abs().ToString().Length || remaining == 0)
            {
                if (remaining.Abs() > new Fraction(1, 2))
                {
                    integer += (remaining.numerator * 2) / remaining.denominator;
                }
                return (integer, 0, 0);
            }
            amount -= integer.Abs().ToString().Length - 2;
            BigInteger length_0 = -1;
            while (remaining < 1)
            {
                length_0++;
                remaining *= 10;
                if (have_initial)
                {
                    amount--;
                }
                if (amount < 0)
                {
                    return (integer, length_0, 0);
                }
            }
            BigInteger left_over = 0;
            while (amount >= 0)
            {
                if (remaining > 1)
                {
                    BigInteger curr = remaining.numerator / remaining.denominator;
                    remaining -= curr;
                    left_over = left_over * 10 + curr;
                }
                remaining *= 10;
                amount--;
            }
            return (integer, length_0, left_over);
        }

        public (BigInteger integer, BigInteger length_0, BigInteger remaining) RepresentDp(BigInteger amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot have negative amount of significant figure");
            }
            BigInteger integer = numerator / denominator;
            if (this < 0)
            {
                integer--;
            }
            Fraction remaining = this - integer;
            if (remaining < 0)
            {
                throw new InvalidOperationException("Fraction:RepresentSigFig idk how it give negative remaining");
            }
            if (remaining == 0)
            {
                if (remaining.Abs() > new Fraction(1, 2))
                {
                    integer += (remaining.numerator * 2) / remaining.denominator;
                }
                return (integer, 0, 0);
            }
            BigInteger length_0 = -1;
            while (remaining < 1)
            {
                length_0++;
                remaining *= 10;
                amount--;
                if (amount < 0)
                {
                    return (integer, length_0, 0);
                }
            }
            BigInteger left_over = 0;
            while (amount >= 0)
            {
                if (remaining > 1)
                {
                    BigInteger curr = remaining.numerator / remaining.denominator;
                    remaining -= curr;
                    left_over = left_over * 10 + curr;
                }
                remaining *= 10;
                amount--;
            }
            return (integer, length_0, left_over);
        }

        public string ReprString(int length = 17)
        {
            if (length < 5)
            {
                throw new InvalidOperationException("Length must be at least 5");
            }
            StringBuilder builder = new StringBuilder(30);
            (BigInteger integer, BigInteger length_0, BigInteger remaining) value = RepresentSigFig(length);
            if (value.integer == 0 && value.remaining != 0)
            {
                string content = value.remaining.ToString();
                builder.Append(content[0]);
                if (content.Length > 1)
                {
                    builder.Append(".");
                    builder.Append(content.Substring(1, content.Length - 1));
                }
                builder.Append("e");
                builder.Append((-value.length_0 - 1).ToString());
                return builder.ToString();
            }
            if (
                (value.integer.Abs().ToString().Length + value.length_0 + value.remaining.ToString().Length +
                (value.remaining == 0 ? -1 : 1)) < length
            )
            {
                builder.Append(value.integer.ToString());
                if (value.remaining > 0)
                {
                    builder.Append(".");
                    builder.Append(new string('0', (int)value.length_0));
                    builder.Append(value.remaining.ToString());
                }
                return builder.ToString();
            }
            else if (
                (value.integer.Abs().ToString().Length + value.length_0 +
                (value.remaining == 0 ? -1 : 1)) < length
            )
            {
                builder.Append(value.integer.ToString());
                if (value.remaining > 0)
                {
                    builder.Append(".");
                    builder.Append(new string('0', (int)value.length_0));
                    int left = length - (int)(value.integer.Abs().ToString().Length + value.length_0);
                    builder.Append((value.remaining / MathUtils.Pow(10, value.remaining.ToString().Length - left)).ToString());
                }
                return builder.ToString();
            }
            else if (
                value.integer.Abs().ToString().Length < length
            )
            {
                return value.integer.Abs().ToString();
            }
            else
            {
                bool is_neg = value.integer < 0;
                string content = value.integer.Abs().ToString();
                BigInteger pow = value.integer.Abs().ToString().Length - 1;
                builder.Append(is_neg ? "-" : "");
                builder.Append(content[0]);
                if (content.Length > 1)
                {
                    builder.Append(".");
                    builder.Append(content.Substring(1, Math.Min(length, content.Length) - 1));
                }
                builder.Append("e");
                builder.Append(pow.ToString());
                return builder.ToString();
            }
        }
    }

    public static class FractionExtension
    {
        public static Fraction Sum(this IEnumerable<Fraction> fracs)
        {
            Fraction totalFrac = new Fraction(0);
            foreach (Fraction frac in fracs)
            {
                totalFrac += frac;
            }
            return totalFrac;
        }

        public static Fraction Sum<T>(this IEnumerable<T> items, Func<T, Fraction> selector)
        {
            Fraction totalFrac = new Fraction(0);
            foreach (T item in items)
            {
                totalFrac += selector(item);
            }
            return totalFrac;
        }
    }
}
