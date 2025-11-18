

namespace ui.core
{
    public abstract class AnsiInputHandler
    {
        public abstract bool Handle(byte[] buf);
    }
}