using System;
using ui.core;

namespace ui.components
{
    public class Container : SingleChildComponent
    {
        public Container(Component component) : base()
        {
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        public Container(Component component, ActiveStatusHandler activeStatusHandler) : base(new ComponentConfig(activeStatusHandler))
        {
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        public Container(Component component, ComponentConfig config) : base(config)
        {
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        protected override void OnResize()
        {
            if (GetMapping().Count == 0) throw new InvalidOperationException("An App must have a child component");
            SetChildAllocatedSize(GetMapping()[0].component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }
    }
}
