using System;
using System.Collections.Generic;
using System.Linq;
using ui.utils;

namespace ui.components
{
    public class RegexTable : Container, ITable
    {
        private List<int> _horizontalBarRow = new List<int>();
        private List<int> _verticalBarCol = new List<int>();
        private Table inner = new Table((1, 1));

        public RegexTable() : base()
        {
            Add(inner);
        }

        public (int x, int y) GetSize() => inner.GetSize();

        public void InsertRow(int idx, ui.utils.SplitAmount amount = null)
        {
            inner.InsertRow(idx, amount);
            _horizontalBarRow = _horizontalBarRow.Select(x => x > idx ? x + 1 : x).ToList();
            for (int x = 0; x < GetSize().x; x++)
            {
                if (_verticalBarCol.Contains(x))
                {
                    inner[(x, idx)] = new TextLabel("│");
                }
            }
        }

        public void RemoveRow(int idx)
        {
            if (_horizontalBarRow.Contains(idx)) _horizontalBarRow.Remove(idx);
            inner.RemoveRow(idx);
            _horizontalBarRow = _horizontalBarRow.Select(x => x > idx ? x - 1 : x).ToList();
        }

        private void InsertRawColumn(int idx, SplitAmount amount = null) => inner.InsertColumn(idx, amount);

        private void AddRawColumn(SplitAmount amount = null) => InsertRawColumn(inner.GetSize().x);

        public void InsertColumn(int idx, ui.utils.SplitAmount amount = null)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater than 0");
            // int idx_intersect, idx_normal;
            // int idx_normal;
            if (idx >= GetSize().x)
            {
                // Add barrier then add data column at the end (not insert)
                // AddRawColumn(1);
                // idx_intersect = inner.GetSize().x - 1;
                inner.AddColumn(amount);
                // idx_normal = inner.GetSize().x - 1;
            }
            else
            {
                // Barrier then the data column on new_idx = idx*2
                // InsertRawColumn(idx * 2, 1);
                inner.InsertColumn(idx * 2, amount);
                // idx_normal = idx * 2;
                // idx_intersect = idx_normal + 1;
            }
            foreach (int y in _horizontalBarRow)
            {
                inner[(idx, y)] = new TextLabel("─");
                // inner[(idx_intersect, y)] = new HorizontalBar('┼');
            }
        }

        public void AddColumn(SplitAmount amount = null) => InsertColumn(GetSize().x, amount);
        public void AddRow(SplitAmount amount = null) => InsertRow(GetSize().y, amount);

        public void Resize((int x, int y) newSize)
        {
            if (newSize.x < GetSize().x || newSize.y < GetSize().y)
            {
                throw new InvalidOperationException("By default, resize to smaller size is not possible. Use ForceResize instead");
            }
            for (int x = GetSize().x; x < newSize.x; x++)
            {
                AddColumn();
            }
            for (int y = GetSize().y; y < newSize.x; y++)
            {
                AddRow();
            }
        }

        public void ForceResize((int x, int y) newSize)
        {
            if (newSize.x < 1 || newSize.y < 1)
            {
                throw new InvalidOperationException("A table must at least have 1 by 1 cell");
            }
            if (newSize.x < GetSize().x)
            {
                for (int x = newSize.x; x > GetSize().y; x--)
                {
                    RemoveColumn(x);
                }
            }
            if (newSize.y < GetSize().y)
            {
                for (int y = newSize.y; y > GetSize().y; y--)
                {
                    RemoveRow(y);
                }
            }
            Resize(newSize);
        }

        private void InsertHorizontalBarRow(int idx)
        {
            InsertRow(idx, 1);
            int y = idx;
            for (int x = 0; x < inner.GetSize().x; x++)
            {
                if (_verticalBarCol.Contains(x))
                {
                    inner[(x, y)] = new TextLabel("┼");
                }
                else
                {
                    inner[(x, y)] = new TextLabel("│");
                }
            }
            _horizontalBarRow.Add(idx);
        }

        private void AddHorizontalBarRow() => InsertHorizontalBarRow(inner.GetSize().y);

        private void InsertVerticalBarCol(int idx)
        {
            InsertRow(idx, 1);
            int y = idx;
            for (int x = 0; x < inner.GetSize().x; x++)
            {
                if (_horizontalBarRow.Contains(x))
                {
                    inner[(x, y)] = new TextLabel("┼");
                }
                else
                {
                    inner[(x, y)] = new TextLabel("─");
                }
            }
            _horizontalBarRow.Add(idx);
        }

        private void AddVerticalBarCol() => InsertVerticalBarCol(inner.GetSize().x);

        public RegexTable((int x, int y) size) : base()
        {
            Add(inner);
            for (int x = 1; x < size.x; x++)
            {
                AddColumn();
            }
            for (int y = 0; y < size.y; y++)
            {
                AddRow();
            }
        }

        public (int x, int y) GetLocaltionMapping((int x, int y) loc)
        {
            return (loc.x * 2, loc.y == 0 ? loc.y : loc.y + 1);
        }


        public void RemoveColumn(int idx)
        {
            inner.RemoveColumn(idx);
            if (_verticalBarCol.Contains(idx))
            {
                _verticalBarCol.Remove(idx);
            }
            _verticalBarCol = _verticalBarCol.Select(x => x > idx ? x - 1 : x).ToList();
        }

        public IComponent this[int x, int y]
        {
            get => inner[x, y];
            set
            {
                if (_horizontalBarRow.Contains(y) || _verticalBarCol.Contains(x))
                {
                    throw new InvalidOperationException("Cannot overwrite bar/row");
                }
                inner[x, y] = value;
            }
        }

        public IComponent this[(int x, int y) loc]
        {
            get => this[loc.x, loc.y];
            set => this[loc.x, loc.y] = value;
        }

    }
}
