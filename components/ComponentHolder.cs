using System;

namespace ui.components
{
    public class UninitComponentException : InvalidOperationException
    {
        public UninitComponentException(string err = null) : base(err ?? "Using component from ComponentHolder that is not initialized") { }
    }

    public sealed class ComponentHolder<T> where T : IComponent
    {
        public T rawInner { get; set; }
        public T inner
        {
            get
            {
                if (!(rawInner is object)) // is null don't work weirdly
                {
                    throw new UninitComponentException();
                }
                return rawInner;
            }
            set => rawInner = value;
        }
    }
}
