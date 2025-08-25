using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;
using ui.utils;

namespace ui.components
{

    public class Table : SingleChildComponent, ITable
    {

        private HorizontalGroupComponent _horizontal = new HorizontalGroupComponent();
        private List<VerticalGroupComponent> _verticalGroups = new List<VerticalGroupComponent>();
        private List<SplitAmount> _vSize = new List<SplitAmount>();
        private (int x, int y) size = (1, 1);

        public Table() : base()
        {
            Add(_horizontal);
            for (int y = 0; y < size.y; y++)
            {

                _vSize.Append(1);
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), 1));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add(vComp);
            }
        }

        public Table(SplitAmount vSplit, SplitAmount hSplit) : base()
        {
            Add(_horizontal);
            for (int y = 0; y < size.y; y++)
            {

                _vSize.Append(vSplit);
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), vSplit));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add((vComp, hSplit));
            }
        }

        public Table((int x, int y) size) : base()
        {
            Add(_horizontal);
            if (size.x < 1 || size.y < 1)
            {
                throw new InvalidOperationException("A table must at least have 1 by 1 cell");
            }
            for (int y = 0; y < size.y; y++)
            {
                _vSize.Append(1);
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), 1));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add(vComp);
            }
        }

        public (int x, int y) GetSize() => size;

        public void Resize((int x, int y) newSize)
        {
            if (newSize.x < size.x || newSize.y < size.y)
            {
                throw new InvalidOperationException("By default, resize to smaller size is not possible. Use ForceResize instead");
            }
            foreach (VerticalGroupComponent comp in _verticalGroups)
            {
                for (int x = size.x; x < newSize.x; x++)
                {
                    comp.Add((new Container(), 1));
                }
            }
            for (int y = size.y; y < newSize.y; y++)
            {
                _vSize.Append(1);
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), 1));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add(vComp);
            }
        }

        public void ForceResize((int x, int y) newSize)
        {
            if (newSize.x < 1 || newSize.y < 1)
            {
                throw new InvalidOperationException("A table must at least have 1 by 1 cell");
            }
            if (newSize.x < size.x)
            {
                while (_verticalGroups.Count > newSize.x)
                {
                    RemoveColumn(newSize.x);
                }
            }
            if (newSize.y < size.y)
            {
                for (int y = newSize.y; y > size.y; y--)
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
                vert.Add((new Container(), _vSize[y]));
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
                _verticalGroups[x].Insert(idx, (new Container(), amount));
            }
            _vSize.Insert(idx, amount);
            size = (size.x, size.y + 1);
        }

        public void RemoveColumn(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.x) idx = size.x - 1;
            VerticalGroupComponent vert = _verticalGroups[idx];
            _verticalGroups.RemoveAt(idx);
            _horizontal.RemoveChildComponent(vert);
            size = (size.x - 1, size.y);
        }

        public void RemoveRow(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx >= size.y) idx = size.y - 1;
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
    }
}
