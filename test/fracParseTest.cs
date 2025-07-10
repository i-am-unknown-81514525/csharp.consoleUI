using ui.math;

namespace ui.test
{
    public class FracParseTest
    {
        public static void Setup()
        {
            while (true)
            {
                Console.Write("Fraction: ");
                if (Fraction.TryParse(Console.ReadLine(), out Fraction frac))
                {
                    Console.WriteLine(frac.ToString());
                }
            }
        }
    }
}