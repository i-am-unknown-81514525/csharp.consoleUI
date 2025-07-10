using ui.math;

namespace ui.test
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