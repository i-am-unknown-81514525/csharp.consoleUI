using System;

namespace ui.components
{
    public class UninitComponentException : InvalidOperationException
    {
        public UninitComponentException(string err = null) : base(err ?? "Using component from ComponentHolder that is not initialized") { }
    }

    public sealed class ComponentHolder<T> where T : IComponent
    {
        public T raw_inner { get; set; }
        public T inner
        {
            get
            {
                if (!(raw_inner is object)) // is null don't work weirdly
                {
                    throw new UninitComponentException();
                }
                return raw_inner;
            }
            set => raw_inner = value;
        }
    }
}
