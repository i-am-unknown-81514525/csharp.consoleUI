using System.Collections.Generic;
using ui.utils;

namespace ui.core
{
    public class KeyCodeTranslationHandler : ANSIInputHandler
    {
        private RootInputHandler handler;
        public KeyCodeTranslationHandler(RootInputHandler handler)
        {
            this.handler = handler;
        }

        private static readonly Dictionary<string, KeyCode> translation = new Dictionary<string, KeyCode> {
            { "\x1b[2~", KeyCode.INSERT },
            { "\x1b[5~", KeyCode.PG_UP },
            { "\x1b[6~" , KeyCode.PG_DOWN },
            { "\x1b[H" , KeyCode.HOME },
            { "\x1b[F", KeyCode.END },
            { "\x1b[3~", KeyCode.ESC },
            { "\x1b[O", KeyCode.SPECIAL_UNFOCUS },
            { "\x1b[I", KeyCode.SPECIAL_FOCUS },
            { "\x1b[A", KeyCode.ARR_UP },
            { "\x1b[B", KeyCode.ARR_DOWN },
            { "\x1b[C", KeyCode.ARR_RIGHT },
            { "\x1b[D", KeyCode.ARR_LEFT },
            { "\x1bOA", KeyCode.ARR_UP },
            { "\x1bOB", KeyCode.ARR_DOWN },
            { "\x1bOC", KeyCode.ARR_RIGHT },
            { "\x1bOD", KeyCode.ARR_LEFT },
        };
        public override bool Handle(byte[] buf)
        {
            string conv = buf.AsByteBuffer().AsString();
            if (translation.ContainsKey(conv))
            {
                handler.LocalDispatch((byte)translation[conv]);
                return true;
            }
            return false;
        }
    }
}