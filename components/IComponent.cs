using ui.core;

namespace ui.components
{
    public interface IComponent : IInteractable
    {
        (uint x, uint y) GetAllocSize();
        void onFrame();
        bool GetHasUpdate();
        IComponent getParent();
        void setParent(IComponent component);
        (uint x, uint y) GetChildAllocatedSize(IComponent component);
        bool Contains(IComponent component);
        int IndexOf(IComponent component);
        bool UpdateAllocSize();
        bool AddChildComponent(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int prioity);
        ConsoleContent[,] Render();
    }
}