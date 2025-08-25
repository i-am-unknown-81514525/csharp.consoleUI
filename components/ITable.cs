namespace ui.components
{
    public interface ITable : IComponent
    {
        (int x, int y) GetSize();
        void Resize((int x, int y) newSize);
        void ForceResize((int x, int y) newSize);
        void AddColumn(ui.utils.SplitAmount amount = null);
        void AddRow(ui.utils.SplitAmount amount = null);
        void InsertColumn(int idx, ui.utils.SplitAmount amount = null);
        void InsertRow(int idx, ui.utils.SplitAmount amount = null);
        void RemoveColumn(int idx);
        void RemoveRow(int idx);
        IComponent this[int x, int y]
        {
            get;
            set;
        }
        IComponent this[(int x, int y) loc]
        {
            get;
            set;
        }
    }
}
