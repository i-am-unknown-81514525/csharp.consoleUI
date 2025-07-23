namespace ui.components
{

    public abstract class NoChildComponent : Component
    {

        protected NoChildComponent(ComponentConfig config) : base(config)
        {
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) onAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (false, child);
        }
    }
}