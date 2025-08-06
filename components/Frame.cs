using ui.components.chainExt;
using ui.math;
using ui.utils;

namespace ui.components
{
    public class Frame : Container
    {

        //Reactive of title with type IComponent, Trigger SetHasUpdate();
        private GroupComponentConfig _title;
        public GroupComponentConfig title
        {
            get => _title;
            set
            {
                SwitchTitle(value);
                SetHasUpdate();
            }
        }

        //Reactive of inner with type IComponent and default value: `new TextLabel("")`, Trigger: SetHasUpdate();
        private IComponent _inner = new TextLabel("");
        public IComponent inner { get => _inner; set { SwitchInner(value); SetHasUpdate(); } }

        private readonly Container titleContainer = new Container();
        private readonly HorizontalGroupComponent outerGroupComponent = null;
        private readonly Container innerContainer = new Container();

        public Frame(GroupComponentConfig? titlegroupConfig = null) : base()
        {
            if (titlegroupConfig is null)
                titlegroupConfig = (new TextLabel(""), 0);
            GroupComponentConfig config = (GroupComponentConfig)titlegroupConfig;
            titleContainer.Add(config.component);
            innerContainer.Add(inner);
            Add(
                new VerticalGroupComponent() {
                    (new HorizontalGroupComponent() {
                        (new TextLabel("┌─"), 2),
                        (outerGroupComponent = new HorizontalGroupComponent() {
                            (titleContainer, config.splitAmount),
                            (new HorizontalBar('─'), (new Fraction(1, 1), 2))
                        }, new Fraction(1, 1)),
                        (new TextLabel("─┐"), 2)
                    }, 1),
                    (
                        new HorizontalGroupComponent() {
                            (new VerticalBar('│'), 1),
                            (new VerticalBar(' '), 1),
                            (innerContainer, new Fraction(1, 1)),
                            (new VerticalBar('│'), 2)
                        },
                        new Fraction(1, 1)
                    ),
                    (
                        new HorizontalGroupComponent() {
                            (new TextLabel("└─"), 2),
                            (new HorizontalBar('─'), new Fraction(1, 1)),
                            (new TextLabel("─┘"), 2)
                        },
                        1
                    )
                }
            );
        }

        public void SwitchTitle(GroupComponentConfig comp)
        {
            IComponent original = title.component;
            SplitAmount ori_amount = title.splitAmount;
            if (!(original is null) && !(original == comp.component))
            {
                titleContainer.RemoveChildComponent(original);
                titleContainer.Add(comp.component);
                titleContainer.SetHasUpdate();
            }
            if (ori_amount != comp.splitAmount)
            {
                outerGroupComponent.UpdateSplitConfig(titleContainer, comp.splitAmount);
                outerGroupComponent.SetHasUpdate();
            }
            comp.component.SetHasUpdate();
            _title = (comp.component, comp.splitAmount);
            SetHasUpdate();
        }

        public void SwitchInner(IComponent comp)
        {
            IComponent original = inner;
            if (!(original is null) && !(original == comp))
            {
                if (!(original is null))
                    innerContainer.RemoveChildComponent(original);
                innerContainer.Add(comp);
                innerContainer.SetHasUpdate();
            }
            _inner = comp;
            comp.SetHasUpdate();
            SetHasUpdate();
        }
    }
}
