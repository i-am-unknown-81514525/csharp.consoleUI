using ui.core;
namespace ui.input
{
    public class NornalANSISkipHandler : ANSIInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return buf[1] == '[';
        }
    }
}
