using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ui.core;
using static ui.core.ConsoleHandler;

namespace ui.test
{
    internal class StdoutInputHandler : InputHandler
    {
        internal override void Handle(RootInputHandler root)
        {
            string value = Buffer.Aggregate("", (string prev, byte curr) => prev + (char)curr);
            Console.Write(value);
        }

        internal override LockStatus Validate()
        {
            return LockStatus.NoLock;
        }
    }

    internal class RndLockInputHandler : InputHandler
    {
        internal override void Handle(RootInputHandler root)
        {
            if (this.GetLockStatus() == LockStatus.NoLock)
            {
                return;
            }
            Console.Write($"\nLock {this.GetLockStatus()} -> No Lock\n");
            this.SetLockStatus(LockStatus.NoLock);
            root.LockChangeAnnounce(this);
        }

        internal override LockStatus Validate()
        {
            Random rnd = new Random();
            return new LockStatus[3] { LockStatus.NoLock, LockStatus.SharedLock, LockStatus.ExclusiveLock }[rnd.Next(0, 3)]; 
        }
    }

    public static class Test
    {
        public static void Setup()
        {
            Global.InputHandler.Add(new RndLockInputHandler());
            Global.InputHandler.Add(new StdoutInputHandler());
            Global.InputHandler.Add(new StdoutInputHandler());
            ConsoleHandler.ConsoleIntermediateHandler.Setup();
            while (true)
            {
                byte result = ConsoleHandler.ConsoleIntermediateHandler.Read();
                if (result == 3)
                {
                    ConsoleHandler.ConsoleIntermediateHandler.Reset();
                    return;
                }
                Global.InputHandler.Dispatch(result);
            }
        }
    }
}
