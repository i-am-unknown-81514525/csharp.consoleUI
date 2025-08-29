using ui.core;
using ui.LatexExt;

namespace ui.components
{
    public interface IComponent : IInteractable, IBaseComponent, ICanActive, IComponentDebug, ILatex
    {
    }
}
