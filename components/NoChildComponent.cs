namespace ui.components
{

    public abstract class NoChildComponent : Component
    {
        protected NoChildComponent() : base()
        {
        }

        protected NoChildComponent(ComponentConfig config) : base(config)
        {
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (false, child);
        }
    }
}
