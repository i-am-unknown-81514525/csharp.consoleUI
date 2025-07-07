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
        internal bool componentStatus = true;

        internal void SetComponentStatus(bool status)
        {
            componentStatus = status;
        }

        internal bool GetComponentStatus() => componentStatus;

        internal override void onDefault(byte value)
        {
            base.onDefault(value);
            Prototype.Set(content);
        }

        internal override void onEnter()
        {
            componentStatus = false;
            Prototype.Next(content);
        }

        internal override void Handle(byte value)
        {
            uint pre = cursor;
            base.Handle(value);
            File.AppendAllText("log-loc", $"recv {value}, pre {pre}, cursor {cursor}, content {content}\n");
            Prototype.Set(content);
            Prototype.WriteTable();
        }

        internal override LockStatus Validate()
        {
            if (GetComponentStatus() && !GetActiveStatus() && (Buffer[0] == 'i' || Buffer[0] == 'i'))
            {
                SetActiveStatus(true);
                return LockStatus.ExclusiveLock;
            }
            return base.Validate();
        }
    }

    internal class NornalANSISkipHandler : ANSIInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return buf[1] == '[';
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
                    // File.AppendAllText("log-loc", $"{n} {y}\n");
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
                if (y == 0)
                {
                    ConsoleCanva.WriteStringOnCanva(Global.consoleCanva, $"      ", (x, y));
                    continue;
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
                    if (varLoc >= table.GetLength(0) || (varLoc >= table.GetLength(0) - 1 && constLoc == 0))
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

        public static Fraction[] getObjFrac(Fraction[,] src)
        {
            Fraction[] obj = new Fraction[src.GetLength(0) - 1];
            for (int i = 0; i < src.GetLength(0) - 1; i++)
            {
                obj[i] = src[i, 0];
            }
            return obj;
        }

        public static void Start()
        {
            Console.Write("Variable count: ");
            bool v1 = uint.TryParse(Console.ReadLine(), out varCount);
            if (varCount >= 10 || !v1 || varCount == 0)
            {
                Console.WriteLine("Variable count must be valid integer, 0 < v < 10");
                Start();
                return;
            }
            Console.Write("Constraint count: ");
            v1 = uint.TryParse(Console.ReadLine(), out constraintCount);
            if (constraintCount >= 10 || !v1 || constraintCount == 0)
            {
                Console.WriteLine("Constraint count must be valid integer, 0 < v < 10");
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
            Global.InputHandler.Add(new NornalANSISkipHandler());
            handler.SetActiveStatus(true);
            ConsoleHandler.ConsoleIntermediateHandler.Setup();
            ConsoleHandler.ConsoleIntermediateHandler.ANSISetup();
            table[table.GetLength(0) - 1, 0] = new Fraction(0);
            strTable[strTable.GetLength(0) - 1, 0] = "0";
            WriteTable();
            try
            {
                while (!isComplete)
                {
                    bool status = Global.InputHandler.Handle();
                    if (!status)
                    {
                        System.Threading.Thread.Sleep(1);
                        continue;
                    }
                    if (exitHandler.GetExitStatus()) break;
                    HandleNext();
                    Prototype.WriteTable();
                }
            }
            finally
            {
                ConsoleHandler.ConsoleIntermediateHandler.Reset();
            }
            Fraction[,] newTable = new Fraction[table.GetLength(0)+table.GetLength(1)-1, table.GetLength(1)];
            for (int c = 0; c < table.GetLength(0) - 1; c++)
            {
                newTable[c, 0] = table[c, 0].asOpposeSign();
            }
            for (int c = table.GetLength(0) - 1; c < table.GetLength(0)+table.GetLength(1)-1; c++)
            {
                newTable[c, 0] = new Fraction(0);
            }
            for (int c = 1; c < table.GetLength(1); c++)
            {
                for (int v = 0; v < table.GetLength(0) - 1; v++)
                {
                    newTable[v, c] = table[v, c];
                }
                for (int v = table.GetLength(0) - 1; v < table.GetLength(0) + table.GetLength(1) - 2; v++)
                {
                    newTable[v, c] = new Fraction(0);
                    if (v - (table.GetLength(0) - 1) == c - 1)
                    {
                        newTable[v, c] = new Fraction(1);
                    }
                }
                newTable[table.GetLength(0) + table.GetLength(1) - 2, c] = table[table.GetLength(0) - 1, c];
            }

            while (getObjFrac(newTable).Min() < new Fraction(0))
            {
                Fraction frac = getObjFrac(newTable).Min();
                int i = Array.IndexOf(getObjFrac(newTable), frac);
                (int j, Fraction rhsV, Fraction fracPivot) = Enumerable.Range(1, table.GetLength(1) - 1)
                        .Where(idx => newTable[i, idx] != new Fraction(0))
                        .Select(idx => (idx, newTable[newTable.GetLength(0) - 1, idx] / newTable[i, idx], newTable[i, idx]))
                        .Where(x => x.Item2 >= new Fraction(0))
                        .OrderBy(x => x.Item2)
                        .First();
                Console.WriteLine($"{i} ({frac.ToString()}), {j}({fracPivot.ToString()})");
                Fraction mul = fracPivot.Invert();
                for (int x = 0; x < newTable.GetLength(0); x++)
                {
                    newTable[x, j] = newTable[x, j] * mul;
                }
                for (int y = 0; y < newTable.GetLength(1); y++)
                {
                    if (y == j) continue;
                    Fraction subMul = newTable[i, y];
                    for (int x = 0; x < newTable.GetLength(0); x++)
                    {
                        newTable[x, y] -= subMul * newTable[x, j];
                    }
                }
            }
            Console.WriteLine($"P = {newTable[newTable.GetLength(0) - 1, 0].ToString()}");
            for (int x = 0; x < newTable.GetLength(0) - 1; x++)
            {
                int count1 = 0;
                Fraction frac = newTable[x, 1];
                Fraction rhs = newTable[x, newTable.GetLength(1) - 1];
                for (int y = 1; y < newTable.GetLength(1); y++)
                {
                    if (newTable[x, y] == 1)
                    {
                        count1++;
                        frac = newTable[x, y];
                        rhs = newTable[newTable.GetLength(0) - 1, y];
                    }
                    else if (newTable[x, y] == 0)
                    {

                    }
                    else
                    {
                        count1 += 2;
                    }
                }
                string name = $"x_{x}";
                if (x >= table.GetLength(0) - 1)
                {
                    name = $"s_{x - (table.GetLength(0) - 1)}";
                }
                if (count1 == 1)
                {
                    Console.WriteLine($"{name} = {rhs.ToString()}");
                }
                else if (count1 == 0)
                {
                    Console.WriteLine($"{name} = ?");
                }
                else
                {
                    Console.WriteLine($"{name} = 0");
                }
            }
            Console.ReadKey();

        }

        public static void Setup()
        {
            Start();
        }
    }
}
