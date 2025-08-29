using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;
using ui.utils;
using System.Text;

namespace ui.components
{

    public class Table : SingleChildComponent, ITable
    {

        private HorizontalGroupComponent _horizontal = new HorizontalGroupComponent();
        private List<VerticalGroupComponent> _verticalGroups = new List<VerticalGroupComponent>();
        private List<SplitAmount> _vSize = new List<SplitAmount>();
        private (int x, int y) size = (0, 0);

        public Table(SplitAmount vSplit = null, SplitAmount hSplit = null) : base()
        {
            Add(_horizontal);
            AddRow(vSplit);
            AddColumn(hSplit);
        }

        public Table((int x, int y) size) : base()
        {
            Add(_horizontal);
            if (size.x < 1 || size.y < 1)
            {
                throw new InvalidOperationException("A table must at least have 1 by 1 cell");
            }
            for (int x = 0; x < size.y; x++)
            {
                AddColumn();
            }
            for (int y = 0; y < size.y; y++)
            {
                AddRow();
            }
        }

        public (int x, int y) GetSize() => size;

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

        public void AddColumn(SplitAmount amount = null) => InsertColumn(size.x, amount);
        public void AddRow(SplitAmount amount = null) => InsertRow(size.y, amount);

        public void InsertColumn(int idx, SplitAmount amount = null)
        {
            if (amount is null) amount = new Fraction(1, 1);
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx > size.x) idx = size.x;
            VerticalGroupComponent vert = new VerticalGroupComponent();
            for (int y = 0; y < size.y; y++)
            {
                vert.Add((new Container(new Padding()), _vSize[y]));
            }
            _verticalGroups.Insert(idx, vert);
            _horizontal.Insert(idx, (vert, amount));
            size = (size.x + 1, size.y);
        }

        public void InsertRow(int idx, SplitAmount amount = null)
        {
            if (amount is null) amount = 1;
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx > size.y) idx = size.y;
            for (int x = 0; x < size.x; x++)
            {
                _verticalGroups[x].Insert(idx, (new Container(new Padding()), amount));
            }
            _vSize.Insert(idx, amount);
            size = (size.x, size.y + 1);
        }

        public void RemoveColumn(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.x) idx = size.x - 1;
            if (size.x == 1) throw new InvalidOperationException("Table cannot be empty");
            VerticalGroupComponent vert = _verticalGroups[idx];
            _verticalGroups.RemoveAt(idx);
            _horizontal.RemoveChildComponent(vert);
            size = (size.x - 1, size.y);
        }

        public void RemoveRow(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.y) idx = size.y - 1;
            if (size.y == 1) throw new InvalidOperationException("Table cannot be empty");
            for (int x = 0; x < size.x; x++)
            {
                VerticalGroupComponent vert = _verticalGroups[x];
                vert.RemoveChildComponent(vert.GetMapping().Select(m => m.component).ToArray()[idx]);
            }
            _vSize.RemoveAt(idx);
            size = (size.x, size.y - 1);
        }



        public IComponent this[int x, int y]
        {
            get => ((Container)_verticalGroups[x].GetMapping().Select(m => m.component).ToArray()[y]).GetInner();
            set
            {
                Container container = ((Container)_verticalGroups[x].GetMapping().Select(m => m.component).ToArray()[y]);
                if (object.ReferenceEquals(container.GetInner(), value))
                {
                    return;
                }
                container.RemoveChildComponent(container.GetInner());
                container.Add(value);
            }
        }

        public IComponent this[(int x, int y) loc]
        {
            get => this[loc.x, loc.y];
            set => this[loc.x, loc.y] = value;
        }

        public override string AsLatex()
        {
            StringBuilder builder = new StringBuilder();
            for (int y = 0; y < GetSize().y; y++)
            {
                for (int x = 0; x < GetSize().x; x++)
                {
                    builder.Append($"{this[x, y].AsLatex()} ");
                }
                builder.Append("\\\\");
            }
            return builder.ToString();
        }
    }
}
