using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.utils;
using ui.core;

namespace ui.components
{
    public class App : SingleChildComponent
    {
        protected OverlayApp overlay = null; // Future

        public App(Component component) : base(new ComponentConfig(new ActiveStatusHandler()))
        {
            noParent = true;
            Add(component);
            SetChildAllocatedSize(component, (0, 0, GetAllocSize().x, GetAllocSize().y), 1);
        }

        protected App(Component component, ActiveStatusHandler activeStatusHandler) : base(new ComponentConfig(activeStatusHandler))
        {
            noParent = true;
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
