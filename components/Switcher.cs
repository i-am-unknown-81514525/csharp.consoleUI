using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.core;

namespace ui.components
{
    public class Switcher : Component, IEnumerable<IComponent>
    {
        List<IComponent> compList = new List<IComponent>();
        private int idx = 0;
        private IComponent curr = null;


        public Switcher() : base() { }

        public Switcher(ComponentConfig config) : base(config) { }

        public Switcher(IEnumerable<IComponent> components) : base()
        {
            compList = components.ToList();
        }

        public Switcher(IEnumerable<IComponent> components, ComponentConfig config) : base(config)
        {
            compList = components.ToList();
        }

        public void Add(IComponent comp)
        {
            compList.Add(comp);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (child.Item1 == curr, child);
        }

        protected override ConsoleContent[,] RenderPre(ConsoleContent[,] content)
        {
            if (compList.Count == 0)
            {
                return content;
            }
            IComponent newComp = compList[idx];
            if (newComp == curr)
            {
                return content;
            }
            if (!(curr is null))
            {
                RemoveChildComponent(curr);
            }
            curr = newComp;
            (uint allocX, uint allocY) = GetAllocSize();
            AddChildComponent(curr, (0, 0, allocX, allocY), 1);
            return base.RenderPre(content);
        }

        public void SwitchTo(int idx)
        {
            if (idx >= compList.Count || idx < 0)
            {
                throw new ArgumentOutOfRangeException("idx must between 0 and compList.Count - 1");
            }
            this.idx = idx;
            SetHasUpdate();
        }

        public int GetCurrIdx() => idx;

        public IComponent GetCurr() => curr;

        IEnumerator<IComponent> IEnumerable<IComponent>.GetEnumerator()
        {
            return compList.ToList().GetEnumerator();
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }

        protected override void OnResize()
        {
            (uint allocX, uint allocY) = GetAllocSize();
            if (!(curr is null))
                SetChildAllocatedSize(curr, (0, 0, allocX, allocY), 1);
        }
    }
}
