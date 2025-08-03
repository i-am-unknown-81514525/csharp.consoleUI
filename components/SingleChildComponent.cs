using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ui.components
{

    public abstract class SingleChildComponent : Component, IEnumerable<IComponent>
    {

        protected SingleChildComponent() : base()
        {
        }
        protected SingleChildComponent(ComponentConfig config) : base(config)
        {
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        public void Add(Component component)
        {
            if (component.GetParent() != null && component.GetParent() != this) throw new InvalidOperationException("The component already have a parent");
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
    }
}
