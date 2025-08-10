using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;

namespace ui.components
{

    public class Table : SingleChildComponent
    {

        private HorizontalGroupComponent _horizontal = new HorizontalGroupComponent();
        private List<VerticalGroupComponent> _verticalGroups = new List<VerticalGroupComponent>();
        private (int x, int y) size = (1, 1);

        public Table() : base()
        {
            Add(_horizontal);
            for (int y = 0; y < size.y; y++)
            {
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), 1));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add(vComp);
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
                VerticalGroupComponent vComp = new VerticalGroupComponent();
                for (int x = 0; x < size.x; x++)
                {
                    vComp.Add((new Container(), 1));
                }
                _verticalGroups.Add(vComp);
                _horizontal.Add(vComp);
            }
        }

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
                    IComponent comp = _verticalGroups[newSize.x];
                    _horizontal.RemoveChildComponent(comp);
                    _verticalGroups.RemoveAt(newSize.x);
                }
            }
            if (newSize.y < size.y)
            {
                foreach (VerticalGroupComponent vert in _verticalGroups)
                {
                    List<IComponent> compList = vert.GetMapping().Select(x => x.component).ToList();
                    while (compList.Count > newSize.y)
                    {
                        IComponent comp = compList[newSize.y];
                        vert.RemoveChildComponent(comp);
                        compList.RemoveAt(newSize.x);
                    }
                }
            }
            Resize(newSize);
        }

        public void InsertColumn(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx > size.x) idx = size.x;
            VerticalGroupComponent vert = new VerticalGroupComponent();
            for (int y = 0; y < size.y; y++)
            {
                vert.Add((new Container(), 1));
            }
            _verticalGroups.Insert(idx, vert);
            _horizontal.Insert(idx, vert);
            size = (size.x + 1, size.y);
        }

        public void InsertRow(int idx)
        {
            if (idx < 0) throw new ArgumentOutOfRangeException("idx must be greater or equal to 0");
            if (idx > size.y) idx = size.y;
            for (int x = 0; x < size.x; x++)
            {
                _verticalGroups[x].Insert(idx, (new Container(), 1));
            }
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
            size = (size.x, size.y - 1);
        }
    }
}
