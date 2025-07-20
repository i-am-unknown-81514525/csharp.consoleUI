using System;
using ui.utils;

namespace ui.core
{
    public abstract class MouseInteractionHandler : ANSIInputHandler
    {
        public readonly int opCode;
        public readonly bool isAllOpCode;

        public MouseInteractionHandler()
        {
            isAllOpCode = true;
            opCode = 0;
        }

        public MouseInteractionHandler(int opCode)
        {
            isAllOpCode = false;
            this.opCode = opCode;
        }

        public abstract void onActive(int opCode, int col, int row);
        public abstract void onInactive(int opCode, int col, int row);

        public void Handle((int opCode, int col, int row, bool isActive) data)
        {
            Action<int, int, int> fn;
            if (data.isActive) fn = onActive;
            else fn = onInactive;
            fn(data.opCode, data.col, data.row);
        }

        public override bool Handle(byte[] buf)
        {
            string content = buf.AsByteBuffer().AsString();
            if (!RegexChecker.IsMouseActivityANSISeq(content))
            {
                return false;
            }
            (int opCode, int col, int row, bool isActive) data = RegexChecker.RetrieveMouseActicity(content);
            if (!isAllOpCode && data.opCode != opCode)
            {
                return false;
            }
            Handle(data);
            return true;
        }
    }
}