using System;
using System.Linq;
using ui.math;
using ui.core;
using ui.input;
using System.IO;

namespace ui.test
{
    public class ValueFieldHandler : InputFieldHandler
    {
        internal override void onDefault(byte value)
        {
            base.onDefault(value);
            Prototype.Set(content);
            File.AppendAllText("log-loc", $"recv {value}\n");
            Prototype.WriteTable();
        }

        internal override void onEnter()
        {

            Prototype.Next(content);
        }
    }

    internal class ANSISkipHandler : ANSIInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return true;
        }
    }

    public static class Prototype
    {
        public static uint varCount = 1;
        public static uint constraintCount = 1;

        public static Fraction[,] table = new Fraction[2, 2];

        public static string[,] strTable = new string[2, 2];

        public static (uint varLoc, uint constLoc) loc = (0, 0);
        public static ValueFieldHandler handler = null;

        public static bool isComplete = false;
        public static bool handleNext = false;
        public static string contentNext = "";

        public static void WriteTable()
        {
            ConsoleCanva canva = Global.consoleCanva;
            canva.EventLoopPre();
            uint width = 6 + (8 * ((uint)strTable.GetLength(0) - 1)) + 2 + 4;
            uint height = 1 + (uint)strTable.GetLength(1) - 1;
            if (canva.GetConsoleSize().Width < width || canva.GetConsoleSize().Height < height)
            {
                canva.SetSize(new ConsoleSize((int)width, (int)height));
            }
            canva.SetEmpty();
            for (int y = 0; y < height; y++)
            {
                ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, y == 0 ? "MAX P=" : "      ", (0, y));
            }
            for (int x = 6; x < 6 + (8 * ((uint)strTable.GetLength(0) - 1)); x += 8)
            {
                for (int y = 0; y < height; y++)
                {
                    int n = (x - 6) / 8;
                    // Console.Write($"{n} {y},");
                    File.AppendAllText("log-loc", $"{n} {y}\n");
                    string ansiPrefix = "";
                    string ansiPostfix = "";
                    if (((uint)n, (uint)y) == loc)
                    {
                        ansiPrefix = "\x1b[30;47m";
                        ansiPostfix = "\x1b[37;40m";
                    }
                    ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, $"+", (x, y));
                    ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, (strTable[n, y] ?? "").PadRight(3).Substring(0, 3), (x + 1, y), ansiPrefix, ansiPostfix);
                    ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, $"x_{n}", (x + 4, y));
                }
            }
            for (int y = 0; y < height; y++)
            {
                int x = 6 + (8 * (table.GetLength(0) - 1));
                string ansiPrefix = "";
                string ansiPostfix = "";
                if (((uint)table.GetLength(0) - 1, (uint)y) == loc)
                {
                    ansiPrefix = "\x1b[30;47m";
                    ansiPostfix = "\x1b[37;40m";
                }
                ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, $"<=", (x, y));
                ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, (strTable[strTable.GetLength(0) - 1, y] ?? "").PadRight(3).Substring(0, 3), (x + 2, y), ansiPrefix, ansiPostfix);
            }
            canva.EventLoopPost();
        }

        public static void Set(string value)
        {
            strTable[loc.varLoc, loc.constLoc] = value;
        }

        public static void Next(string value)
        {
            handleNext = true;
            contentNext = value;
        }

        public static void HandleNext()
        {
            if (!handleNext) return;
            string value = contentNext;
            handleNext = false;
            if (handler != null)
            {
                Global.InputHandler.Remove(handler);
                if (double.TryParse(value, out double longValue))
                {
                    Fraction frac = new Fraction(longValue);
                    table[loc.varLoc, loc.constLoc] = frac;
                    uint varLoc = loc.varLoc + 1;
                    uint constLoc = loc.constLoc;
                    if (varLoc >= table.GetLength(0))
                    {
                        varLoc = 0;
                        constLoc += 1;
                    }
                    if (constLoc >= table.GetLength(1))
                    {
                        constLoc = 0;
                        isComplete = true;
                        return;
                    }
                    loc = (varLoc, constLoc);
                }
            }
            strTable[loc.varLoc, loc.constLoc] = "";
            handler = new ValueFieldHandler();
            handler.SetActiveStatus(true);
            Global.InputHandler.Add(handler);
        }

        public static void Start()
        {
            Console.Write("Variable count: ");
            varCount = uint.Parse(Console.ReadLine());
            if (varCount >= 10)
            {
                Console.Write("Variable count must be < 10");
                Start();
                return;
            }
            Console.Write("Constraint count: ");
            constraintCount = uint.Parse(Console.ReadLine());
            if (varCount >= 10)
            {
                Console.Write("Variable count must be < 10");
                Start();
                return;
            }
            FracConfig.iterationLimit = 8;
            table = new Fraction[varCount + 1, constraintCount + 1];
            strTable = new string[varCount + 1, constraintCount + 1];
            ExitHandler exitHandler = new ExitHandler();
            Global.InputHandler.Add(exitHandler);
            KeyCodeTranslationHandler keyCodeHandler = new KeyCodeTranslationHandler(Global.InputHandler);
            Global.InputHandler.Add(keyCodeHandler);
            handler = new ValueFieldHandler();
            Global.InputHandler.Add(handler);
            Global.InputHandler.Add(new ANSISkipHandler());
            handler.SetActiveStatus(true);
            ConsoleHandler.ConsoleIntermediateHandler.Setup();
            ConsoleHandler.ConsoleIntermediateHandler.ANSISetup();
            WriteTable();
            try
            {
                while (!isComplete)
                {
                    Global.InputHandler.Handle();
                    if (exitHandler.GetExitStatus()) break;
                    HandleNext();
                    Prototype.WriteTable();
                    System.Threading.Thread.Sleep(10);
                }
            }
            finally
            {
                ConsoleHandler.ConsoleIntermediateHandler.Reset();
            }
        }

        public static void Setup()
        {
            Start();
        }
    }
}