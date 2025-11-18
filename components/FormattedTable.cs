using System;
using ui.utils;

namespace ui.components
{
    public class FormattedTable : VirtualTable<LatexTable>
    {


        protected override (int x, int y) size { get; set; } = (1, 1);

        protected override LatexTable InnerConstructor()
        {
            LatexTable table = new LatexTable((1, 1));
            size = (1, 1);
            return table;
        }


        public FormattedTable()
        {

        }

        public FormattedTable((int x, int y) size)
        {
            Resize(size);
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            int insertIdx = (idx * 2 - 1);
            if (insertIdx < 0)
            {
                Inner.InsertVerticalBarCol(0);
                Inner.InsertColumn(0, amount);
            }
            else
            {
                Inner.InsertColumn(insertIdx, amount);
                Inner.InsertVerticalBarCol(insertIdx);
            }
            size = (size.x + 1, size.y);
            SetHasUpdate();
        }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            if (size.y == 1)
            {
                Inner.InsertHorizontalBarRow(1);
            }
            if (idx == 0)
            {
                Inner.RemoveRow(1);
                Inner.InsertRow(idx, amount);
                Inner.InsertHorizontalBarRow(1);
            }
            else
            {
                Inner.InsertRow(idx + 1, amount);
            }
            size = (size.x, size.y + 1);
        }

        public override void RemoveColumn(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException($"idx must be greater or equal to 0 (given: {idx})");
            if (idx >= size.x) idx = size.x - 1;
            if (size.x == 1) throw new InvalidOperationException("Table cannot be empty");
            int removeIdx = (idx * 2 - 1);
            if (removeIdx < 0)
            {
                Inner.RemoveColumn(0);
                Inner.RemoveColumn(0);
            }
            else
            {
                Inner.RemoveColumn(removeIdx);
                Inner.RemoveColumn(removeIdx);
            }
            size = (size.x - 1, size.y);
            SetHasUpdate();
        }

        public override void RemoveRow(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException($"idx must be greater or equal to 0 (given:{idx})");
            if (idx >= size.y) idx = size.y - 1;
            if (size.y == 1) throw new InvalidOperationException("Table cannot be empty");
            if (idx == 0)
            {
                Inner.RemoveRow(0);
                Inner.RemoveRow(0); // horizontal bar
                Inner.InsertHorizontalBarRow(1);
            }
            else
            {
                int removeIdx = idx + 1;
                Inner.RemoveRow(removeIdx);
            }
            size = (size.x, size.y - 1);
            if (size.y == 1)
            {
                Inner.RemoveRow(1); // the horizontal bar removed when only 1 row remaining
            }
            SetHasUpdate();
        }

        public override IComponent this[int x, int y]
        {
            get => Inner[x * 2, y == 0 ? 0 : y + 1];
            set
            {
                Inner[x * 2, y == 0 ? 0 : y + 1] = value;
            }
        }

        public override (int x, int y) GetSize()
        {
            (int x, int y) = Inner.GetSize();
            if (y > 1)
            {
                y--;
            }
            return ((x + 1) / 2, y);
        }
    }
}
