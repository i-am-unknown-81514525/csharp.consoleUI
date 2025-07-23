using ui.core;

namespace ui.events {
    public class ClickEvent : Event
    {
        public ConsoleLocation location { get; }
        public ClickEvent(ConsoleLocation location)
        {
            this.location = location;
        }
    
    }
}