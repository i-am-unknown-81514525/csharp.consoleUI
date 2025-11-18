using System;
using System.Linq;
using ui.core;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    // ReSharper disable once UnusedMember.Global
    internal class StdoutInputHandler : InputHandler
    {
        protected override void Handle(RootInputHandler root)
        {
            string value = "";
            foreach (var b in Buffer) value += (char)b;
            Console.Write(value);
        }

        protected override LockStatus Validate()
        {
            return LockStatus.NO_LOCK;
        }
    }

    internal class ExitHandler : InputHandler
    {
        private bool _exit = false;

        protected override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() > LockStatus.NO_LOCK)
            {
                _exit = true;
                this.SetLockStatus(LockStatus.NO_LOCK);
                root.LockChangeAnnounce(this);
            }
        }

        protected override LockStatus Validate()
        {
            if (Buffer.Count > 0 && Buffer[0] == (byte)KeyCode.INTERRUPT) return LockStatus.EXCLUSIVE_LOCK;
            return LockStatus.NO_LOCK;
        }

        public bool GetExitStatus() => _exit;
    }

    internal class InputTriggerHandler : InputHandler
    {
        private bool _enableInput = false;

        protected override void Handle(RootInputHandler root)
        {
            _enableInput = false;
            if (GetLockStatus() > LockStatus.NO_LOCK)
            {
                _enableInput = true;
                this.SetLockStatus(LockStatus.NO_LOCK);
                root.LockChangeAnnounce(this);
            }
        }

        protected override LockStatus Validate()
        {
            if (Buffer.Count > 0 && Buffer[0] == (byte)'i') return LockStatus.SHARED_LOCK;
            return LockStatus.NO_LOCK;
        }

        public bool GetTriggerStatus() => _enableInput;
    }

    // ReSharper disable once UnusedMember.Global
    internal class RndLockInputHandler : InputHandler
    {
        protected override void Handle(RootInputHandler root)
        {
            if (GetLockStatus() == LockStatus.NO_LOCK)
            {
                return;
            }
            Console.Write($"\nLock {GetLockStatus()} -> No Lock\n");
            SetLockStatus(LockStatus.NO_LOCK);
            root.LockChangeAnnounce(this);
        }

        protected override LockStatus Validate()
        {

            // ReSharper disable once RedundantExplicitArraySize
            return new LockStatus[3]
            {
                LockStatus.NO_LOCK,
                LockStatus.SHARED_LOCK,
                LockStatus.EXCLUSIVE_LOCK
            }[Test.Rnd.Next(0, 3)];
        }
    }

    // ReSharper disable once InconsistentNaming
    internal class ANSIStdoutInputHandler : AnsiInputHandler
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
        protected override LockStatus Validate()
        {
            return LockStatus.NO_LOCK;
        }

        protected override void Handle(RootInputHandler root)
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
            ConsoleIntermediateHandler.AnsiSetup();
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
