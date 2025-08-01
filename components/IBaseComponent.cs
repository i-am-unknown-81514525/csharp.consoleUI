using ui.core;

namespace ui.components
{
    public interface IBaseComponent
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
        bool isInit();
        void Init(ComponentConfig config);
        (int row, int col) getAbsolutePos((int row, int col) pos, IComponent childComp);
        (int row, int col) getAbsolutePos((int row, int col) pos);
    }
}
