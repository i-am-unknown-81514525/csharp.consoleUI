namespace ui.components
{
    public class Container<TS> : SingleChildComponent<TS> where TS : ComponentStore
    {

        public Container()
        {
        }

        public Container(IComponent component)
        {
            Add(component);
        }

        public Container(ComponentConfig config) : base(config) { }


        public Container(IComponent component, ComponentConfig config) : base(config)
        {
            Add(component);
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (GetMapping().Count == 0, (child.Item1, (0, 0, GetAllocSize().x, GetAllocSize().y), 1));
        }

        public void Set(IComponent comp)
        {
            if (!(GetInner() is null))
            {
                RemoveChildComponent(GetInner());
            }
            Add(comp);
        }
    }

    public class Container : Container<EmptyStore>
    {
        public Container()
        { }
        public Container(IComponent component) : base(component) { }
        public Container(ComponentConfig config) : base(config) { }
        public Container(IComponent component, ComponentConfig config) : base(component, config) { }
    }
}
