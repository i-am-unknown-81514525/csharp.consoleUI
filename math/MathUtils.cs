using System;
using System.Collections.Generic;
using System.Numerics;

namespace ui.math
{
    public static class MathUtils {
        // public static long factorize(long num1, long num2)
        // {
        //     long minV = Math.Min(Math.Abs(num1), Math.Abs(num2));
        //     long highestPossibleFactor = (long)Math.Floor(Math.Sqrt(minV));
        //     long[] primes = genSieveOp(highestPossibleFactor);
        //     long factor = 1;
        //     foreach (long prime in primes)
        //     {
        //         while (num1 % prime == 0 && num2 % prime == 0)
        //         {
        //             factor *= prime;
        //             num1 /= prime;
        //             num2 /= prime;
        //         }
        //     }
        //     return factor;

        // }

        public static BigInteger Abs(this BigInteger v)
        {
            if (v >= 0)
            {
                return v;
            }
            return -v;
        }

        public static long factorize(long num1, long num2)
        {
            num1 = Math.Abs(num1);
            num2 = Math.Abs(num2);
            while (num2 != 0)
            {
                long temp = num2;
                num2 = num1 % num2;
                num1 = temp;
            }

            return num1;
        }

        public static BigInteger factorize(BigInteger num1, BigInteger num2)
        {
            if (num1 < 0) num1 = -num1;
            if (num2 < 0) num2 = -num2;
            while (num2 != 0)
            {
                BigInteger temp = num2;
                num2 = num1 % num2;
                num1 = temp;
            }

            return num1;
        }

        public static long lMultiple(long num1, long num2) => (num1 / factorize(num1, num2)) * num2;
        

        public static long[] genSieveOp(long n)
        {
            if (n < 10)
            {
                long[] presetTo10 = new long[] { 2, 3, 5, 7 };
                List<long> output = new List<long>();
                for (long i = 0; i < presetTo10.Length; i++)
                {
                    if (presetTo10[i] <= n)
                    {
                        output.Add(presetTo10[i]);
                    }
                }
                return output.ToArray();
            }

            long sqrtv = (long)Math.Floor(Math.Sqrt(n));
            long[] primes = genSieveOp(sqrtv);
            long start = ((sqrtv + 1) / 2) * 2 + 1; // e.g. 5 -> 7, 6 -> 7
            long length = (n - start) / 2 + 1;
            long prevPrimeLength = primes.Length;
            bool[] sieve = new bool[length]; // 7 -> 10: 7 9, 7 -> 11: 7 9 11
            long iter_n = n + 1;
            long iter_n_minus_start = iter_n - start;
            for (long i = 1; i < prevPrimeLength; i++) // Start at 1 to ignore the first prime: 2 as all multiple of 2 have avoided by design
            {
                long r_i = primes[i];
                long init_v = r_i * r_i - start;
                long r2_i = 2 * r_i;
                if (init_v < 0)
                {
                    init_v = (long)Math.Ceiling((decimal)start / r_i) * r_i - start;
                }
                if (init_v % 2 == 1)
                {
                    init_v += r_i;
                }
                for (long j = init_v; j < iter_n_minus_start; j += r2_i)
                {
                    long prime_i = j >> 1;
                    sieve[prime_i] = true;
                }
            }

            long size = n;
            if (n > 64679)
            {
                size = n / 10; // Pre-calculated value where when searching for first n value (where n > 64679), it will have less than or equal to n/10 amount
            }

            if (n > 9560081)
            {
                size = n / 15;
            }

            if (n > 189969229)
            {
                size = n / 18;
            }
            long arrayLength = primes.Length;
            Array.Resize(ref primes, (int)size);

            for (long i = 0; i < length; i++)
            {
                if (!sieve[i])
                {
                    primes[arrayLength] = (start + (i << 1));
                    arrayLength++;
                }
            }
            Array.Resize(ref primes, (int)arrayLength);
            return primes;
        }
    }
}