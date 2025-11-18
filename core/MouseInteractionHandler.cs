using System;
using ui.utils;

namespace ui.core
{
    public abstract class MouseInteractionHandler : AnsiInputHandler
    {
        public readonly int OpCode;
        public readonly bool IsAllOpCode;

        public MouseInteractionHandler()
        {
            IsAllOpCode = true;
            OpCode = 0;
        }

        public MouseInteractionHandler(int opCode)
        {
            IsAllOpCode = false;
            this.OpCode = opCode;
        }

        public abstract void OnActive(int opCode, ConsoleLocation loc);
        public abstract void OnInactive(int opCode, ConsoleLocation loc);

        public void Handle((int opCode, int col, int row, bool isActive) data)
        {
            Action<int, ConsoleLocation> fn;
            if (data.isActive) fn = OnActive;
            else fn = OnInactive;
            fn(data.opCode, AnsiConverter.ToConsoleLocation(data.row, data.col));
        }

        public override bool Handle(byte[] buf)
        {
            string content = buf.AsByteBuffer().AsString();
            if (!RegexChecker.IsMouseActivityAnsiSeq(content))
            {
                return false;
            }
            (int opCode, int col, int row, bool isActive) data = RegexChecker.RetrieveMouseActicity(content);
            if (!IsAllOpCode && data.opCode != OpCode)
            {
                return false;
            }
            Handle(data);
            return true;
        }
    }
}