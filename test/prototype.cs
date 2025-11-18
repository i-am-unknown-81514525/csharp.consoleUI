using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ui.core;
using ui.input;
using ui.math;

// ReSharper disable All

namespace ui.test
{
    public class ValueFieldHandler : InputFieldHandler
    {
        internal bool ComponentStatus = true;
        private bool _ignoreI = false;

        internal void SetComponentStatus(bool status)
        {
            ComponentStatus = status;
        }

        internal bool GetComponentStatus() => ComponentStatus;

        protected override void OnDefault(byte value)
        {
            base.OnDefault(value);
            Prototype.Set(Content);
        }

        protected override void OnEnter()
        {
            ComponentStatus = false;
            Prototype.Next(Content);
        }

        protected override void Handle(byte value)
        {
            if (value == (byte)'i' && _ignoreI)
            {
                _ignoreI = false;
                return;
            }
            uint pre = Cursor;
            base.Handle(value);
            File.AppendAllText("log-loc", $"recv {value}, pre {pre}, cursor {Cursor}, content {Content}\n");
            Prototype.Set(Content);
            Prototype.WriteTable();
        }

        protected override LockStatus Validate()
        {
            if (GetComponentStatus() && !GetActiveStatus() && (Buffer[0] == 'i' || Buffer[0] == 'i'))
            {
                _ignoreI = true;
                SetActiveStatus(true);
                return LockStatus.EXCLUSIVE_LOCK;
            }
            return base.Validate();
        }
    }

    internal class NornalAnsiSkipHandler : AnsiInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return buf[1] == '[';
        }
    }

    public static class Prototype
    {
        public static uint VarCount = 1;
        public static uint ConstraintCount = 1;

        public static Fraction[,] Table = new Fraction[2, 2];

        public static string[,] StrTable = new string[2, 2];

        public static (uint varLoc, uint constLoc) Loc = (0, 0);
        public static ValueFieldHandler Handler = null;

        public static bool IsComplete = false;
        public static bool handleNext = false;
        public static string ContentNext = "";


        public static void WriteTable()
        {
            ConsoleCanva canva = Global.ConsoleCanva;
            canva.EventLoopPre();
            uint width = 6 + (16 * ((uint)StrTable.GetLength(0) - 1)) + 2 + 4;
            uint height = 1 + (uint)StrTable.GetLength(1) - 1;
            if (canva.GetConsoleSize().Width < width || canva.GetConsoleSize().Height < height)
            {
                canva.SetSize(new ConsoleSize((int)width, (int)height));
            }
            canva.SetEmpty();
            for (int y = 0; y < height; y++)
            {
                ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, y == 0 ? "MAX P=" : "      ", (0, y));
            }
            for (int x = 6; x < 6 + (16 * ((uint)StrTable.GetLength(0) - 1)); x += 16)
            {
                for (int y = 0; y < height; y++)
                {
                    int n = (x - 6) / 16;
                    // Console.Write($"{n} {y},");
                    // File.AppendAllText("log-loc", $"{n} {y}\n");
                    string ansiPrefix = "";
                    string ansiPostfix = "";
                    if (((uint)n, (uint)y) == Loc)
                    {
                        ansiPrefix = "\x1b[30;47m";
                        ansiPostfix = "\x1b[37;40m";
                    }
                    ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, $"+", (x, y));
                    ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, (StrTable[n, y] ?? "").PadRight(11).Substring(0, 11), (x + 1, y), ansiPrefix, ansiPostfix);
                    ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, $"x_{n}", (x + 12, y));
                }
            }
            for (int y = 0; y < height; y++)
            {
                int x = 6 + (16 * (Table.GetLength(0) - 1));
                string ansiPrefix = "";
                string ansiPostfix = "";
                if (((uint)Table.GetLength(0) - 1, (uint)y) == Loc)
                {
                    ansiPrefix = "\x1b[30;47m";
                    ansiPostfix = "\x1b[37;40m";
                }
                if (y == 0)
                {
                    ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, $"      ", (x, y));
                    continue;
                }
                ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, $"<=", (x, y));
                ConsoleCanva.WriteStringOnCanva(Global.ConsoleCanva, (StrTable[StrTable.GetLength(0) - 1, y] ?? "").PadRight(11).Substring(0, 11), (x + 2, y), ansiPrefix, ansiPostfix);
            }
            canva.EventLoopPost();
        }

        public static void Set(string value)
        {
            StrTable[Loc.varLoc, Loc.constLoc] = value;
        }

        public static void Next(string value)
        {
            handleNext = true;
            ContentNext = value;
        }

        public static void HandleNext()
        {
            if (!handleNext) return;
            string value = ContentNext;
            handleNext = false;
            if (Handler != null)
            {
                Global.InputHandler.Remove(Handler);
                if (Fraction.TryParse(value, out Fraction frac))
                {
                    Table[Loc.varLoc, Loc.constLoc] = frac;
                    uint varLoc = Loc.varLoc + 1;
                    uint constLoc = Loc.constLoc;
                    if (varLoc >= Table.GetLength(0) || (varLoc >= Table.GetLength(0) - 1 && constLoc == 0))
                    {
                        varLoc = 0;
                        constLoc += 1;
                    }
                    if (constLoc >= Table.GetLength(1))
                    {
                        constLoc = 0;
                        IsComplete = true;
                        return;
                    }
                    Loc = (varLoc, constLoc);
                }
            }
            StrTable[Loc.varLoc, Loc.constLoc] = "";
            Handler = new ValueFieldHandler();
            Handler.SetActiveStatus(true);
            Global.InputHandler.Add(Handler);
        }

        public static Fraction[] GetObjFrac(Fraction[,] src)
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
            bool v1 = uint.TryParse(Console.ReadLine(), out VarCount);
            if (VarCount >= 10 || !v1 || VarCount == 0)
            {
                Console.WriteLine("Variable count must be valid integer, 0 < v < 10");
                Start();
                return;
            }
            Console.Write("Constraint count: ");
            v1 = uint.TryParse(Console.ReadLine(), out ConstraintCount);
            if (ConstraintCount >= 10 || !v1 || ConstraintCount == 0)
            {
                Console.WriteLine("Constraint count must be valid integer, 0 < v < 10");
                Start();
                return;
            }
            Table = new Fraction[VarCount + 1, ConstraintCount + 1];
            StrTable = new string[VarCount + 1, ConstraintCount + 1];
            ExitHandler exitHandler = new ExitHandler();
            Global.InputHandler.Add(exitHandler);
            KeyCodeTranslationHandler keyCodeHandler = new KeyCodeTranslationHandler(Global.InputHandler);
            Global.InputHandler.Add(keyCodeHandler);
            Handler = new ValueFieldHandler();
            Global.InputHandler.Add(Handler);
            Global.InputHandler.Add(new NornalAnsiSkipHandler());
            Handler.SetActiveStatus(true);
            ConsoleHandler.ConsoleIntermediateHandler.Setup();
            ConsoleHandler.ConsoleIntermediateHandler.AnsiSetup();
            Table[Table.GetLength(0) - 1, 0] = new Fraction(0);
            StrTable[StrTable.GetLength(0) - 1, 0] = "0";
            WriteTable();
            try
            {
                while (!IsComplete)
                {
                    bool status = Global.InputHandler.Handle();
                    if (!status)
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    if (exitHandler.GetExitStatus())
                    {
                        return;
                    }
                    HandleNext();
                    WriteTable();
                }
            }
            finally
            {
                ConsoleHandler.ConsoleIntermediateHandler.Reset();
            }
            Fraction[,] newTable = new Fraction[Table.GetLength(0) + Table.GetLength(1) - 1, Table.GetLength(1)];
            for (int c = 0; c < Table.GetLength(0) - 1; c++)
            {
                newTable[c, 0] = Table[c, 0].AsOpposeSign();
            }
            for (int c = Table.GetLength(0) - 1; c < Table.GetLength(0) + Table.GetLength(1) - 1; c++)
            {
                newTable[c, 0] = new Fraction(0);
            }
            for (int c = 1; c < Table.GetLength(1); c++)
            {
                for (int v = 0; v < Table.GetLength(0) - 1; v++)
                {
                    newTable[v, c] = Table[v, c];
                }
                for (int v = Table.GetLength(0) - 1; v < Table.GetLength(0) + Table.GetLength(1) - 2; v++)
                {
                    newTable[v, c] = new Fraction(0);
                    if (v - (Table.GetLength(0) - 1) == c - 1)
                    {
                        newTable[v, c] = new Fraction(1);
                    }
                }
                newTable[Table.GetLength(0) + Table.GetLength(1) - 2, c] = Table[Table.GetLength(0) - 1, c];
            }
            for (int dj = 0; dj < newTable.GetLength(1); dj++)
            {
                for (int di = 0; di < newTable.GetLength(0); di++)
                {
                    Console.Write($"{newTable[di, dj]} ");
                }
                Console.Write("\n");
            }
            while (GetObjFrac(newTable).Min() < new Fraction(0))
            {
                Fraction frac = GetObjFrac(newTable).Min();
                int i = Array.IndexOf(GetObjFrac(newTable), frac);
                (int j, Fraction rhsV, Fraction fracPivot)[] selections = Enumerable.Range(1, Table.GetLength(1) - 1)
                    .Where(idx => newTable[i, idx] > new Fraction(0))
                    .Select(idx => (idx, newTable[newTable.GetLength(0) - 1, idx] / newTable[i, idx], newTable[i, idx])) // idx, RHS/value
                    .Where(x => x.Item2 >= new Fraction(0))
                    .OrderBy(x => x.Item2).ToArray();
                if (selections.Length == 0)
                {
                    string name = $"x_{i}";
                    if (i >= Table.GetLength(0) - 1)
                    {
                        name = $"s_{i - (Table.GetLength(0) - 1)}";
                    }
                    Console.WriteLine($"{name} is unbounded");
                    Console.WriteLine($"P = {newTable[newTable.GetLength(0) - 1, 0].ToString()}+inf");
                    List<int> rowsUsed1 = new List<int>();
                    for (int x = 0; x < newTable.GetLength(0) - 1; x++)
                    {
                        if (x == i) continue;
                        string name1 = $"x_{x}";
                        if (x >= Table.GetLength(0) - 1)
                        {
                            name1 = $"s_{x - (Table.GetLength(0) - 1)}";
                        }
                        int[] count1 = Enumerable.Range(1, newTable.GetLength(1) - 1).Where(y => newTable[x, y] == 1).ToArray();
                        int[] other = Enumerable.Range(1, newTable.GetLength(1) - 1).Where(y => newTable[x, y] != 0 && newTable[x, y] != 1).ToArray();
                        if (other.Count() == 0 && count1.Count() == 1)
                        {
                            int idx = count1[0];
                            if (!rowsUsed1.Contains(idx))
                            {
                                Fraction rhs = newTable[newTable.GetLength(0) - 1, idx];
                                rowsUsed1.Append(idx);
                                Console.WriteLine($"{name1} = {rhs.ToString()}");
                                continue;
                            }
                        }
                        Console.WriteLine($"{name1} = 0");
                    }
                    return;
                }
                (int j, Fraction rhsV, Fraction fracPivot) = selections.First();
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
                for (int dj = 0; dj < newTable.GetLength(1); dj++)
                {
                    for (int di = 0; di < newTable.GetLength(0); di++)
                    {
                        Console.Write($"{newTable[di, dj]} ");
                    }
                    Console.Write("\n");
                }
            }
            Console.WriteLine($"P = {newTable[newTable.GetLength(0) - 1, 0].ToString()}");
            // int[] oneFrom = new int[newTable.GetLength(0)].Select(q => -1).ToArray();
            List<int> rowsUsed = new List<int>();
            for (int x = 0; x < newTable.GetLength(0) - 1; x++)
            {
                string name = $"x_{x}";
                if (x >= Table.GetLength(0) - 1)
                {
                    name = $"s_{x - (Table.GetLength(0) - 1)}";
                }
                int[] count1 = Enumerable.Range(1, newTable.GetLength(1) - 1).Where(y => newTable[x, y] == 1).ToArray();
                int[] other = Enumerable.Range(1, newTable.GetLength(1) - 1).Where(y => newTable[x, y] != 0 && newTable[x, y] != 1).ToArray();
                if (other.Count() == 0 && count1.Count() == 1)
                {
                    int idx = count1[0];
                    if (!rowsUsed.Contains(idx))
                    {
                        Fraction rhs = newTable[newTable.GetLength(0) - 1, idx];
                        rowsUsed.Append(idx);
                        Console.WriteLine($"{name} = {rhs.ToString()}");
                        continue;
                    }
                }
                Console.WriteLine($"{name} = 0");
            }
            Console.ReadKey();

        }

        public static void Setup()
        {
            Start();
        }
    }
}
