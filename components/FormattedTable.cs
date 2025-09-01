using System;
using ui.utils;

namespace ui.components
{
    public class FormattedTable : VirtualTable<LatexTable>
    {
        internal override (int x, int y) size { get; set; } = (1, 1);

        internal override LatexTable InnerConstructor()
        {
            LatexTable table = new LatexTable((1, 1));
            size = (1, 1);
            return table;
        }


        public FormattedTable() : base()
        {

        }

        public FormattedTable((int x, int y) size)
        {
            Resize(size);
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            int insert_idx = (idx * 2 - 1);
            if (insert_idx < 0)
            {
                inner.InsertVerticalBarCol(0);
                inner.InsertColumn(0, amount);
            }
            else
            {
                inner.InsertColumn(insert_idx, amount);
                inner.InsertVerticalBarCol(insert_idx);
            }
            size = (size.x + 1, size.y);
        }

        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            if (size.y == 1)
            {
                inner.AddHorizontalBarRow();
            }
            inner.InsertRow(idx + 1, amount);
            size = (size.x, size.y + 1);
        }

        public override void RemoveColumn(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.x) idx = size.x - 1;
            if (size.x == 1) throw new InvalidOperationException("Table cannot be empty");
            int remove_idx = (idx * 2 - 1);
            if (remove_idx < 0)
            {
                inner.RemoveColumn(0);
                inner.RemoveColumn(0);
            }
            else
            {
                inner.RemoveColumn(remove_idx);
                inner.RemoveColumn(remove_idx);
            }
            size = (size.x - 1, size.y);
        }

        public override void RemoveRow(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.y) idx = size.y - 1;
            if (size.y == 1) throw new InvalidOperationException("Table cannot be empty");
            if (idx == 0)
            {
                inner.RemoveRow(0);
                inner.RemoveRow(0); // horizontal bar
                inner.InsertHorizontalBarRow(1);
            }
            else
            {
                int remove_idx = idx + 1;
                inner.RemoveRow(remove_idx);
            }
            size = (size.x, size.y - 1);
            if (size.y == 1)
            {
                inner.RemoveRow(1); // the horizontal bar removed when only 1 row remaining
            }
        }

        public override IComponent this[int x, int y]
        {
            get => inner[x * 2, y == 0 ? 0 : y + 1];
            set
            {
                inner[x * 2, y == 0 ? 0 : y + 1] = value;
            }
        }
    }
}
