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
                    Console.WriteLine(frac.RepresentSigFig(13));
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