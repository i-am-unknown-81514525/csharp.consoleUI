using ui.core;

namespace ui.components
{
    public interface IInteractable
    {
        void OnClick(ConsoleLocation pressLocation);
        void OnHover(ConsoleLocation location);

    }
}
