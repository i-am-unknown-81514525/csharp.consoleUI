using ui.core;

namespace ui.events
{
    public class TypeEvent : Event
    {
        public char value { get; }
        public TypeEvent(char value)
        {
            this.value = value;
        }

    }
}
