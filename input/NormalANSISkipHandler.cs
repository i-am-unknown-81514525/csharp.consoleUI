using ui.core;

namespace ui.input
{
    public class NornalAnsiSkipHandler : AnsiInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return buf[1] == '[';
        }
    }
}
