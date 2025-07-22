using ui.core;

namespace ui.components
{
    public interface IInteractable
    {
        void onClick(ConsoleLocation pressLocation);
        void onHover(ConsoleLocation location);

    }
}