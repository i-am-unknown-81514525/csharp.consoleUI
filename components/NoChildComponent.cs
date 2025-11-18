namespace ui.components
{

    public abstract class NoChildComponent<T> : Component<T> where T : ComponentStore
    {
        protected NoChildComponent()
        {
        }

        protected NoChildComponent(ComponentConfig config) : base(config)
        {
        }

        protected NoChildComponent(T store) : base(store)
        {
        }

        protected NoChildComponent(ComponentConfig config, T store) : base(config, store)
        {
        }

        protected override (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (false, child);
        }
    }

    public abstract class NoChildComponent : Component<EmptyStore>
    {
        protected NoChildComponent()
        {
        }

        protected NoChildComponent(ComponentConfig config) : base(config)
        {
        }
    }
}
