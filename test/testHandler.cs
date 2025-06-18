using System;
using System.Linq;
using ui.core;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    // ReSharper disable once UnusedMember.Global
    internal class StdoutInputHandler : InputHandler
    {
        internal override void Handle(RootInputHandler root)
        {
            string value = "";
            foreach (var b in Buffer) value += (char)b;
            Console.Write(value);
        }

        internal override LockStatus Validate()
        {
            return LockStatus.NoLock;
        }
    }

    // ReSharper disable once UnusedMember.Global
    internal class RndLockInputHandler : InputHandler
    {
        internal override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() == LockStatus.NoLock)
            {
                return;
            }
            Console.Write($"\nLock {GetLockStatus()} -> No Lock\n");
            SetLockStatus(LockStatus.NoLock);
            root.LockChangeAnnounce(this);
        }

        internal override LockStatus Validate()
        {
            
            // ReSharper disable once RedundantExplicitArraySize
            return new LockStatus[3]
            {
                LockStatus.NoLock,
                LockStatus.SharedLock,
                LockStatus.ExclusiveLock
            }[Test.Rnd.Next(0, 3)];
        }
    }

    // ReSharper disable once InconsistentNaming
    internal class ANSIStdoutInputHandler : ANSIInputHandler
    {
        internal override void Handle(RootInputHandler root)
        {
            base.Handle(root);
            if (isANSI) // Lock Reset => Valid ANSI
            {
                string content = string.Concat(
                    ANSIvalue.Select(
                        x=>x==(byte)'\x1b'?
                            "[\\x1b]":
                            ((char)x).ToString()
                    )
                );
                Console.WriteLine(content);
            }
            // ReSharper disable once RedundantJumpStatement
            return;
        }
    }

    // ReSharper disable once InconsistentNaming
    internal class ASCIIIntStdouInputHandler : InputHandler
    {
        internal override LockStatus Validate()
        {
            return LockStatus.NoLock;
        }

        internal override void Handle(RootInputHandler root)
        {
            if (Buffer.Count == 0)
            {
                return;
            }
            Console.Write($"{(int)Buffer[0]},");
        }
    }

    public static class Test
    {
        public static Random Rnd = new Random();
        public static void Setup()
        {
            // Global.InputHandler.Add(new RndLockInputHandler());
            Global.InputHandler.Add(new ANSIStdoutInputHandler());
            Global.InputHandler.Add(new ASCIIIntStdouInputHandler());
            // Global.InputHandler.Add(new StdoutInputHandler());
            // Global.InputHandler.Add(new StdoutInputHandler());
            ConsoleIntermediateHandler.Setup();
            ConsoleIntermediateHandler.ANSISetup();
            try
            {
                while (true)
                {
                    byte result = ConsoleIntermediateHandler.Read();
                    if (result == 3)
                    {
                        ConsoleIntermediateHandler.Reset();
                        return;
                    }
                    Global.InputHandler.Dispatch(result);
                }
            } catch (Exception)
            {
                ConsoleIntermediateHandler.Reset();
                throw;
            }
        }
    }
}
