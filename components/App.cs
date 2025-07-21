using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ui.components
{
    public class App : BaseComponent, IEnumerable<BaseComponent>
    {
        internal OverlayApp overlay = null; // Future

        public App(BaseComponent component)
        {
            noParent = true;
            Add(component);
        }

        public IEnumerator<BaseComponent> GetEnumerator()
        {
            return GetMapping().Select(x=>x.component).GetEnumerator();
        }

        public void Add(BaseComponent component)
        {
            (uint allocX, uint allocY)  = GetAllocSize();
            AddChildComponent(component, (0, 0, allocX, allocY), 1);
        }

        protected override (bool isAdd, (BaseComponent, (uint, uint, uint, uint), int) data) onAddHandler((BaseComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, child);
        }

        protected override void onResize()
        {
            
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}