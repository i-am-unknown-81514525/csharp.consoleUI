using ui.core;
namespace ui.input {
    internal class NornalANSISkipHandler : ANSIInputHandler
    {

        public override bool Handle(byte[] buf)
        {
            return buf[1] == '[';
        }
    }
}