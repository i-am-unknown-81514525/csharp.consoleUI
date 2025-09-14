namespace ui.components
{
    public struct ComponentHolder<T> where T : IComponent
    {
        public T comp { get; set; }
    }
}
