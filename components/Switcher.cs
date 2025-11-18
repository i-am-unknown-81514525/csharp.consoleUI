using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.core;

namespace ui.components
{
    public class Switcher<TS, T> : Component<TS>, IEnumerable<IComponent> where TS : ComponentStore where T : Switcher<TS, T>
    {
        List<IComponent> _compList = new List<IComponent>();
        private int _idx = 0;
        private IComponent _curr = null;


        public Switcher() : base() { }

        public Switcher(ComponentConfig config) : base(config) { }

        public Switcher(TS store) : base(store) { }

        public Switcher(ComponentConfig config, TS store) : base(config, store) { }

        public Switcher(IEnumerable<IComponent> components) : base()
        {
            _compList = components.ToList();
        }

        public Switcher(IEnumerable<IComponent> components, ComponentConfig config) : base(config)
        {
            _compList = components.ToList();
        }

        public void Add(IComponent comp)
        {
            _compList.Add(comp);
        }

        public void AddMulti(IEnumerable<IComponent> comps)
        {
            foreach (IComponent comp in comps)
                Add(comp);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (child.Item1 == _curr, child);
        }

        protected override ConsoleContent[,] RenderPre(ConsoleContent[,] content)
        {
            if (_compList.Count == 0)
            {
                return content;
            }
            IComponent newComp = _compList[_idx];
            if (newComp == _curr)
            {
                return content;
            }
            if (!(_curr is null))
            {
                RemoveChildComponent(_curr);
            }
            _curr = newComp;
            (uint allocX, uint allocY) = GetAllocSize();
            AddChildComponent(_curr, (0, 0, allocX, allocY), 1);
            return base.RenderPre(content);
        }

        public void SwitchTo(int idx)
        {
            if (idx >= _compList.Count || idx < 0)
            {
                throw new ArgumentOutOfRangeException("idx must between 0 and compList.Count - 1");
            }
            this._idx = idx;
            SetHasUpdate();
        }

        public int GetCurrIdx() => _idx;

        public IComponent GetCurr() => _curr;

        IEnumerator<IComponent> IEnumerable<IComponent>.GetEnumerator()
        {
            return _compList.ToList().GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void OnResize()
        {
            (uint allocX, uint allocY) = GetAllocSize();
            if (!(_curr is null))
                SetChildAllocatedSize(_curr, (0, 0, allocX, allocY), 1);
        }

        public override string Debug_Info()
        {
            return $"[{_compList.Count} total child components, Active Index: {_idx}]";
        }
    }

    public class Switcher<TS> : Switcher<TS, Switcher<TS>> where TS : ComponentStore { }

    public class Switcher : Switcher<EmptyStore, Switcher> { }
}
