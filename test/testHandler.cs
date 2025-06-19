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

    internal class ExitHandler : InputHandler
    {
        private bool exit = false;

        internal override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() > LockStatus.NoLock)
            {
                exit = true;
                this.SetLockStatus(LockStatus.NoLock);
                root.LockChangeAnnounce(this);
            }
        }

        internal override LockStatus Validate()
        {
            if (Buffer.Count > 0 && Buffer[0] == (byte)KeyCode.INTERRUPT) return LockStatus.ExclusiveLock;
            return LockStatus.NoLock;
        }

        public bool GetExitStatus() => exit;
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

        public override bool Handle(byte[] buf)
        {
            string content = string.Concat(
                    buf.Select(
                        x => x == (byte)'\x1b' ?
                            "[\\x1b]" :
                            ((char)x).ToString()
                    )
                );
            Console.Write(content);
            return true;
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
            ExitHandler exitHandler = new ExitHandler();
            Global.InputHandler.Add(exitHandler);
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
                    Global.InputHandler.Handle();
                    if (exitHandler.GetExitStatus()) break;
                    System.Threading.Thread.Sleep(10);
                }
                ConsoleIntermediateHandler.Reset();
            }
            catch (Exception)
            {
                ConsoleIntermediateHandler.Reset();
                throw;
            }
        }
    }
}
