using System;
using ui.math;

namespace ui.test
{
    public static class FracParseTest
    {
        public static void Setup()
        {
            while (true)
            {
                Console.Write("Fraction: ");
                if (Fraction.TryParse(Console.ReadLine(), out Fraction frac))
                {
                    Console.Write("Success: ");
                    Console.WriteLine(frac.ToString());
                    Console.WriteLine(frac.RepresentSigFig(17)); // 1000000000000001/10000000000000000 ???
                    Console.WriteLine(frac.ReprString());
                }
                else
                {
                    Console.WriteLine("Failed");
                }
            }
        }
    }
}