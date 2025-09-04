using System;
using System.Collections.Generic;
using System.Linq;
using ui.utils;

namespace ui.components
{
    public class LatexTable : VirtualTable<Table>
    {
        private List<int> _horizontalBarRow = new List<int>();
        private List<int> _verticalBarCol = new List<int>();

        protected override Table InnerConstructor()
        {
            return new Table((1, 1));
        }

        public LatexTable() : base()
        {
        }

        public override void InsertRow(int idx, ui.utils.SplitAmount amount = null)
        {
            if (idx > size.y) idx = size.y;
            inner.InsertRow(idx, amount);
            _horizontalBarRow = _horizontalBarRow.Select(x => x > idx ? x + 1 : x).ToList();
            for (int x = 0; x < GetSize().x; x++)
            {
                if (_verticalBarCol.Contains(x))
                {
                    inner[(x, idx)] = new VerticalBar('│');
                }
            }
            SetHasUpdate();
        }

        public override void RemoveRow(int idx)
        {
            if (_horizontalBarRow.Contains(idx)) _horizontalBarRow.Remove(idx);
            inner.RemoveRow(idx);
            _horizontalBarRow = _horizontalBarRow.Select(x => x > idx ? x - 1 : x).ToList();
            SetHasUpdate();
        }


        public override void InsertColumn(int idx, ui.utils.SplitAmount amount = null)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater than 0");
            if (idx > size.x) idx = size.x;
            // int idx_intersect, idx_normal;
            // int idx_normal;
            // if (idx >= GetSize().x)
            // {
            //     // Add barrier then add data column at the end (not insert)
            //     // AddRawColumn(1);
            //     // idx_intersect = inner.GetSize().x - 1;
            //     inner.AddColumn(amount);
            //     // idx_normal = inner.GetSize().x - 1;
            // }
            // else
            // {
            //     // Barrier then the data column on new_idx = idx*2
            //     // InsertRawColumn(idx * 2, 1);
            //     // inner.InsertColumn(idx, amount);
            //     // idx_normal = idx * 2;
            //     // idx_intersect = idx_normal + 1;
            // }
            inner.InsertColumn(idx, amount);
            _verticalBarCol = _verticalBarCol.Select(y => y > idx ? y + 1 : y).ToList();

            foreach (int y in _horizontalBarRow)
            {
                DEBUG.DebugStore.AppendLine($"Write horizontal to {y}");
                inner[(idx, y)] = new HorizontalBar('─');
                // inner[(idx_intersect, y)] = new HorizontalBar('┼');
            }
            SetHasUpdate();
        }

        public void InsertHorizontalBarRow(int idx)
        {
            if (idx > size.y) idx = size.y;
            InsertRow(idx, 1);
            int y = idx;
            DEBUG.DebugStore.AppendLine($"Write horizontal to {y} from bar insert");
            for (int x = 0; x < inner.GetSize().x; x++)
            {
                if (_verticalBarCol.Contains(x))
                {
                    inner[(x, y)] = new TextLabel("┼");
                }
                else
                {
                    inner[(x, y)] = new HorizontalBar('─');
                    // inner[(x, y)] = new VerticalBar('│');
                }
            }
            _horizontalBarRow.Add(idx);
        }

        public void AddHorizontalBarRow() => InsertHorizontalBarRow(inner.GetSize().y);

        public void InsertVerticalBarCol(int idx)
        {
            // int curr_idx = idx;
            // int curr = size.x;
            if (idx > size.x) idx = size.x;
            InsertColumn(idx, 1);
            int x = idx;
            // if (x >= inner.GetSize().x)
            // {
            //     throw new InvalidOperationException($"DEBUG: x={x}, inner.GetSize().x={inner.GetSize().x} prev,size.x={curr} size.x={size.x} prev,idx={idx}");
            // }
            for (int y = 0; y < inner.GetSize().y; y++)
            {
                if (_horizontalBarRow.Contains(y))
                {
                    inner[(x, y)] = new TextLabel("┼");
                }
                else
                {
                    // inner[(x, y)] = new HorizontalBar('─');
                    inner[(x, y)] = new VerticalBar('│');
                }
            }
            _verticalBarCol.Add(idx);
        }

        public void AddVerticalBarCol() => InsertVerticalBarCol(inner.GetSize().x);

        public LatexTable((int x, int y) size) : base()
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


        public override void RemoveColumn(int idx)
        {
            inner.RemoveColumn(idx);
            if (_verticalBarCol.Contains(idx))
            {
                _verticalBarCol.Remove(idx);
            }
            _verticalBarCol = _verticalBarCol.Select(x => x > idx ? x - 1 : x).ToList();
            SetHasUpdate();
        }

        public override IComponent this[int x, int y]
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

        public override string AsLatex()
        {
            string table_config = Enumerable.Range(0, GetSize().y).Select(x => _verticalBarCol.Contains(x) ? '|' : 'c').AsByteBuffer().AsString();
            int[] arrangement = Enumerable.Range(0, GetSize().y).Where(i => table_config[i] == 'c').ToArray();
            List<string> contents = new List<string>();
            for (int y = 0; y < GetSize().y; y++)
            {
                string content = "";
                if (_horizontalBarRow.Contains(y))
                {
                    content = "\\hline";
                }
                else
                {
                    content = String.Join(" & ", arrangement.Select(x => this[x, y])) + "\\\\";
                }
                contents.Append(content);
            }
            return $"\\begin{{tabular}}{{ {arrangement} }}\n{String.Join("\n", contents)}\n\\end{{tabular}}";
        }

        public override string Debug_Info()
        {
            return $"horizontal_row={String.Join(", ", _horizontalBarRow)} vertical_column={String.Join(", ", _verticalBarCol)}";
        }
    }
}
