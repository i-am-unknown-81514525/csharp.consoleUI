using System;
using System.Runtime.CompilerServices;

namespace ui.math
{
    public class Fraction
    {
        public readonly long numerator, denominator;

        public Fraction(long numerator, long denominator)
        {
            long value = MathUtils.factorize(numerator, denominator);
            this.numerator = numerator / value;
            this.denominator = denominator / value;
        }

        public Fraction(long value)
        {
            this.numerator = value;
            this.denominator = 1;
        }

        public static implicit operator double(Fraction fraction)
        {
            return (double)fraction.numerator / (double)fraction.denominator;
        }

        public static implicit operator float(Fraction fraction)
        {
            return (float)((double)fraction.numerator / (double)fraction.denominator);
        }

        public Fraction simplify()
        {
            long value = MathUtils.factorize(numerator, denominator);
            return new Fraction(numerator / value, denominator / value);
        }

        public Fraction Add(Fraction other)
        {
            // 3/4 + 5/8 = (3*8+5*4)/8*4, = 44/32 = 11/8 
            long v1 = MathUtils.factorize(this.denominator, other.denominator);
            long denominator = this.denominator / v1 * other.denominator;
            long leftNum = denominator / this.denominator * this.numerator;
            long rightNum = denominator / other.denominator * other.numerator;
            long numerator = leftNum + rightNum;
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
            long v1 = MathUtils.factorize(this.numerator, other.denominator);
            long v2 = MathUtils.factorize(other.numerator, this.denominator);
            Fraction left = new Fraction(this.numerator / v1, this.denominator / v2);
            Fraction right = new Fraction(other.numerator / v2, other.denominator / v1);
            long numerator = left.numerator * right.numerator;
            long denominator = left.denominator * right.denominator;
            return new Fraction(numerator, denominator);
        }

        public Fraction Divide(Fraction other)
        {
            return Multiply(other.Invert());
        }

        public bool isLong()
        {
            long v1 = MathUtils.factorize(numerator, denominator);
            return denominator == v1 || denominator == -v1 || numerator == 0;
        }

        public bool TryLong(out long value)
        {
            value = 0;
            if (!isLong())
                return false;
            long v1 = MathUtils.factorize(numerator, denominator);
            long num = numerator / v1;
            long deno = denominator / v1;
            value = num * deno; // deno for sign
            return true;
        }

        public bool isInteger()
        {
            bool stats = TryLong(out long value);
            if (!stats)
                return false;
            return value >= int.MinValue && value <= int.MaxValue;
        }

        public bool TryInt(out int value)
        {
            value = 0;
            if (!isInteger())
                return false;
            TryLong(out long v1);
            value = (int)v1;
            return true;
        }

        public static Fraction operator +(Fraction left, Fraction right) => left.Add(right);
        public static Fraction operator +(Fraction left, long right) => left + new Fraction(right);
        public static Fraction operator +(long left, Fraction right) => new Fraction(left) + right;
        public static Fraction operator -(Fraction left, Fraction right) => left.Subtract(right);
        public static Fraction operator -(Fraction left, long right) => left - new Fraction(right);
        public static Fraction operator -(long left, Fraction right) => new Fraction(left) - right;
        public static Fraction operator *(Fraction left, Fraction right) => left.Multiply(right);
        public static Fraction operator *(Fraction left, long right) => left * new Fraction(right);
        public static Fraction operator *(long left, Fraction right) => new Fraction(left) * right;
        public static Fraction operator /(Fraction left, Fraction right) => left.Divide(right);
        public static Fraction operator /(Fraction left, long right) => left / new Fraction(right);
        public static Fraction operator /(long left, Fraction right) => new Fraction(left) / right;

        public static bool operator ==(Fraction left, Fraction right)
        {
            Fraction simLeft = left.simplify();
            Fraction simRight = right.simplify();
            return simLeft.numerator == simRight.numerator && simLeft.denominator == simRight.denominator;
        }

        public static bool operator !=(Fraction left, Fraction right) => !(left == right);

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
    }
}