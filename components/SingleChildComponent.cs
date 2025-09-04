using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ui.components
{

    public abstract class SingleChildComponent<T> : Component<T>, IEnumerable<IComponent> where T : ComponentStore
    {

        protected SingleChildComponent() : base()
        {
        }
        protected SingleChildComponent(ComponentConfig config) : base(config)
        {
        }

        protected SingleChildComponent(T store) : base(store)
        {
        }
        protected SingleChildComponent(ComponentConfig config, T store) : base(config, store)
        {
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        public void Add(IComponent component)
        {
            if (component.GetMount() != null && component.GetMount() != this) throw new InvalidOperationException("The component already have a parent");
            (uint allocX, uint allocY) = GetAllocSize();
            AddChildComponent(component, (0, 0, allocX, allocY), 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<IComponent> GetEnumerator()
        {
            return GetMapping().Select(x => x.component).GetEnumerator();
        }

        protected override void OnResize()
        {
            if (GetMapping().Count == 0) throw new InvalidOperationException("This component must have a child component");
            SetChildAllocatedSize(GetMapping()[0].component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
            SetHasUpdate();
        }

        public IComponent GetInner()
        {
            if (GetMapping().Count == 0) return null;
            return GetMapping().Select(x => x.component).First();
        }

        public override string AsLatex()
        {
            if (GetMapping().Count == 1)
            {
                return GetMapping()[0].component.AsLatex();
            }
            else
            {
                return "[EMPTY]";
            }
        }
    }
}
