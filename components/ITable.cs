namespace ui.components
{
    public interface ITable : IComponent
    {
        (int x, int y) GetSize();
        void Resize((int x, int y) newSize);
        void ForceResize((int x, int y) newSize);
        void AddColumn(utils.SplitAmount amount = null);
        void AddRow(utils.SplitAmount amount = null);
        void InsertColumn(int idx, utils.SplitAmount amount = null);
        void InsertRow(int idx, utils.SplitAmount amount = null);
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
