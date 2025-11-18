using System.Collections.Generic;
using ui.utils;

namespace ui.core
{
    public class KeyCodeTranslationHandler : AnsiInputHandler
    {
        private RootInputHandler _handler;
        public KeyCodeTranslationHandler(RootInputHandler handler)
        {
            _handler = handler;
        }

        private static readonly Dictionary<string, KeyCode> Translation = new Dictionary<string, KeyCode> {
            { "\u001b[2~", KeyCode.INSERT },
            { "\u001b[5~", KeyCode.PG_UP },
            { "\u001b[6~" , KeyCode.PG_DOWN },
            { "\u001b[H" , KeyCode.HOME },
            { "\u001b[F", KeyCode.END },
            { "\u001b[3~", KeyCode.ESC },
            { "\u001b[O", KeyCode.SPECIAL_UNFOCUS },
            { "\u001b[I", KeyCode.SPECIAL_FOCUS },
            { "\u001b[A", KeyCode.ARR_UP },
            { "\u001b[B", KeyCode.ARR_DOWN },
            { "\u001b[C", KeyCode.ARR_RIGHT },
            { "\u001b[D", KeyCode.ARR_LEFT },
            { "\u001bOA", KeyCode.ARR_UP },
            { "\u001bOB", KeyCode.ARR_DOWN },
            { "\u001bOC", KeyCode.ARR_RIGHT },
            { "\u001bOD", KeyCode.ARR_LEFT },
        };
        public override bool Handle(byte[] buf)
        {
            string conv = buf.AsByteBuffer().AsString();
            if (Translation.ContainsKey(conv))
            {
                _handler.LocalDispatch((byte)Translation[conv]);
                return true;
            }
            return false;
        }
    }
}