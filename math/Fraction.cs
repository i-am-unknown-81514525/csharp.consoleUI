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
        public readonly BigInteger Numerator, Denominator;

        public Fraction(BigInteger numerator, BigInteger denominator)
        {
            if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            if (denominator < 0) { numerator = -numerator; denominator = -denominator; }
            BigInteger value = MathUtils.Factorize(numerator, denominator);
            this.Numerator = numerator / value;
            this.Denominator = denominator / value;
        }

        public Fraction(long numerator, long denominator)
        {
            (this.Numerator, this.Denominator) = new Fraction((BigInteger)numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // long value = MathUtils.factorize(numerator, denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(BigInteger numerator, long denominator)
        {
            (this.Numerator, this.Denominator) = new Fraction((BigInteger)numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // BigInteger value = MathUtils.factorize(numerator, (BigInteger)denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(long numerator, BigInteger denominator)
        {
            (this.Numerator, this.Denominator) = new Fraction(numerator, (BigInteger)denominator);
            // if (denominator == 0) throw new DivideByZeroException("Denominator cannot be 0");
            // BigInteger value = MathUtils.factorize((BigInteger)numerator, denominator);
            // this.numerator = numerator / value;
            // this.denominator = denominator / value;
        }

        public Fraction(BigInteger value)
        {
            Numerator = value;
            Denominator = 1;
        }

        public Fraction(long value)
        {
            Numerator = value;
            Denominator = 1;
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
            while (remain != 0 && it < Config.FracDoubleInterpretPrecision)
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
            (this.Numerator, this.Denominator) = new Fraction(numerator, denominator);
        }

        public Fraction(string value)
        {
            Fraction f = Fraction.Parse(value);
            Denominator = f.Denominator;
            Numerator = f.Numerator;
        }

        public static implicit operator double(Fraction fraction)
        {
            return (double)fraction.Numerator / (double)fraction.Denominator;
        }

        public static implicit operator float(Fraction fraction)
        {
            return (float)((double)fraction.Numerator / (double)fraction.Denominator);
        }

        public static implicit operator (BigInteger numerator, BigInteger denominator)(Fraction fraction) => fraction.AsTuple();

        public static implicit operator Fraction(uint value) => new Fraction((long)value);
        // Operator conversion precedence fix for the compiler, with AI advise used, fixed SplitHandler.Update (totalFrac > 1 || i == totalPrioityTier - 1)
        // Google Gemini 2.5 Flash via Gemini app, Accessed 24 July 2025, https://g.co/gemini/share/30770d6d34e9

        public static implicit operator Fraction(long value) => new Fraction(value);
        public static implicit operator Fraction(ulong value) => new Fraction((BigInteger)value);
        public static implicit operator Fraction(BigInteger value) => new Fraction(value);

        public void Deconstruct(out BigInteger numerator, out BigInteger denominator)
        {
            numerator = this.Numerator;
            denominator = this.Denominator;
        }

        public Fraction Simplify()
        {

            BigInteger value = MathUtils.Factorize(Numerator, Denominator);
            if (Numerator < 0 && Denominator < 0)
            {
                value *= -1;
            }
            return new Fraction(Numerator / value, Denominator / value);
        }

        public Fraction Add(Fraction other)
        {
            // 3/4 + 5/8 = (3*8+5*4)/8*4, = 44/32 = 11/8
            BigInteger v1 = MathUtils.Factorize(this.Denominator, other.Denominator);
            BigInteger denominator = this.Denominator / v1 * other.Denominator;
            BigInteger leftNum = denominator / this.Denominator * this.Numerator;
            BigInteger rightNum = denominator / other.Denominator * other.Numerator;
            BigInteger numerator = leftNum + rightNum;
            return new Fraction(numerator, denominator);
        }

        public Fraction AsOpposeSign()
        {
            // Overflow safe
            if (Denominator == long.MinValue && Numerator == long.MinValue)
                return new Fraction(1, 1);
            else if (Denominator == long.MinValue)
                return new Fraction(Numerator, -Denominator);
            return new Fraction(-Numerator, Denominator);
        }

        public Fraction Invert()
        {
            // Overflow safe
            return new Fraction(Denominator, Numerator);
        }

        public Fraction Subtract(Fraction other)
        {
            return Add(other.AsOpposeSign());
        }

        public Fraction Multiply(Fraction other)
        {
            BigInteger v1 = MathUtils.Factorize(this.Numerator, other.Denominator);
            BigInteger v2 = MathUtils.Factorize(other.Numerator, this.Denominator);
            Fraction left = new Fraction(this.Numerator / v1, this.Denominator / v2);
            Fraction right = new Fraction(other.Numerator / v2, other.Denominator / v1);
            BigInteger numerator = left.Numerator * right.Numerator;
            BigInteger denominator = left.Denominator * right.Denominator;
            return new Fraction(numerator, denominator);
        }

        public Fraction Divide(Fraction other)
        {
            return Multiply(other.Invert());
        }

        public bool IsBigInteger()
        {
            BigInteger v1 = MathUtils.Factorize(Numerator, Denominator);
            return Denominator == v1 || Denominator == -v1 || Numerator == 0;
        }

        public bool TryBigInteger(out BigInteger value)
        {
            value = 0;
            if (!IsBigInteger())
                return false;
            BigInteger v1 = MathUtils.Factorize(Numerator, Denominator);
            BigInteger num = Numerator / v1;
            BigInteger deno = Denominator / v1;
            value = num * deno; // deno for sign
            return true;
        }

        public bool IsLong()
        {
            bool stats = TryBigInteger(out BigInteger value);
            if (!stats)
                return false;
            return value >= long.MinValue && value <= long.MaxValue;
        }

        public bool TryLong(out long value)
        {
            value = 0;
            if (!IsLong())
                return false;
            TryBigInteger(out BigInteger v1);
            value = (long)v1;
            return true;
        }

        public bool IsInteger()
        {
            bool stats = TryBigInteger(out BigInteger value);
            if (!stats)
                return false;
            return value >= int.MinValue && value <= int.MaxValue;
        }

        public bool TryInt(out int value)
        {
            value = 0;
            if (!IsInteger())
                return false;
            TryBigInteger(out BigInteger v1);
            value = (int)v1;
            return true;
        }

        public BigInteger GetFloor()
        {
            return Numerator / Denominator;
        }

        public static Fraction operator +(Fraction left, Fraction right) => left.Add(right);
        public static Fraction operator +(Fraction left, long right) => left + new Fraction(right);
        public static Fraction operator +(long left, Fraction right) => new Fraction(left) + right;
        public static Fraction operator +(Fraction right) => right;
        public static Fraction operator -(Fraction left, Fraction right) => left.Subtract(right);
        public static Fraction operator -(Fraction left, long right) => left - new Fraction(right);
        public static Fraction operator -(long left, Fraction right) => new Fraction(left) - right;
        public static Fraction operator -(Fraction right) => right.AsOpposeSign();
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
            return simLeft.Numerator == simRight.Numerator && simLeft.Denominator == simRight.Denominator;
        }

        public static bool operator !=(Fraction left, Fraction right) => !(left == right);
        public static bool operator <(Fraction left, Fraction right) => left.Numerator * right.Denominator < right.Numerator * left.Denominator;
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
            int c1 = (int)(Numerator / v % v);
            int c2 = (int)(Numerator % v);
            int c3 = (int)(Denominator / v % v);
            int c4 = (int)(Denominator % v);
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
            if (Denominator == 1)
            {
                return $"{Numerator.ToString()}";
            }
            return $"{Numerator.ToString()}/{Denominator.ToString()}";
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

        public (BigInteger numerator, BigInteger denominator) AsTuple() => (Numerator, Denominator);

        public string AsLatex() => Denominator != 1 ? $"$\\frac{{{Numerator}}}{{{Denominator}}}$" : $"${Numerator}$";

        public Fraction Abs() => this >= 0 ? this : -this;

        public (BigInteger integer, BigInteger length_0, BigInteger remaining) RepresentSigFig(BigInteger amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot have negative amount of significant figure");
            }
            bool haveInitial = this.Abs() >= 1;
            BigInteger integer = Numerator / Denominator;
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
                    integer += remaining.Numerator * 2 / remaining.Denominator;
                }
                return (integer, 0, 0);
            }
            amount -= integer.Abs().ToString().Length - 2;
            BigInteger length0 = -1;
            while (remaining < 1 && remaining != 0)
            {
                length0++;
                remaining *= 10;
                if (haveInitial)
                {
                    amount--;
                }
                if (amount < 0)
                {
                    return (integer, length0, 0);
                }
            }
            BigInteger leftOver = 0;
            while (amount >= 0)
            {
                if (remaining > 1)
                {
                    BigInteger curr = remaining.Numerator / remaining.Denominator;
                    remaining -= curr;
                    leftOver = leftOver * 10 + curr;
                }
                remaining *= 10;
                amount--;
            }
            return (integer, length0, leftOver);
        }

        public (BigInteger integer, BigInteger length_0, BigInteger remaining) RepresentDp(BigInteger amount)
        {
            if (amount < 0)
            {
                throw new InvalidOperationException("Cannot have negative amount of significant figure");
            }
            BigInteger integer = Numerator / Denominator;
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
                    integer += remaining.Numerator * 2 / remaining.Denominator;
                }
                return (integer, 0, 0);
            }
            BigInteger length0 = -1;
            while (remaining < 1 && remaining != 0)
            {
                length0++;
                remaining *= 10;
                amount--;
                if (amount < 0)
                {
                    return (integer, length0, 0);
                }
            }
            BigInteger leftOver = 0;
            while (amount >= 0)
            {
                if (remaining > 1)
                {
                    BigInteger curr = remaining.Numerator / remaining.Denominator;
                    remaining -= curr;
                    leftOver = leftOver * 10 + curr;
                }
                remaining *= 10;
                amount--;
            }
            return (integer, length0, leftOver);
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
                bool isNeg = value.integer < 0;
                string content = value.integer.Abs().ToString();
                BigInteger pow = value.integer.Abs().ToString().Length - 1;
                builder.Append(isNeg ? "-" : "");
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
