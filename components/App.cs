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

        public App(Component component) : base(new ComponentConfig(new SplitHandler(1).AddSplit(new math.Fraction(1), 1), new ActiveStatusHandler()))
        {
            noParent = true;
            Add(component);
        }

        protected App(Component component, SplitConfig splitConfig, ActiveStatusHandler activeStatusHandler) : base(new ComponentConfig(splitConfig, activeStatusHandler))
        {
            noParent = true;
            Add(component);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) onAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        protected override void onResize()
        {
            if (GetMapping().Count == 0) throw new InvalidOperationException("An App must have a child component");
            (IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity) value = childsMapping[0];
            value.location.allocX = GetAllocSize().x;
            value.location.allocY = GetAllocSize().y;
            childsMapping[0] = value;
        }
    }
}