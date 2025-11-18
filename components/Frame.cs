using ui.components.chainExt;
using ui.math;
using ui.utils;
using ui.core;

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

        private readonly IComponent _frameInner;
        private readonly Container _titleContainer = new Container();
        private readonly HorizontalGroupComponent _outerGroupComponent = null;
        private readonly Container _innerContainer = new Container();

        private const bool ToggleCanNoFrame = true; // future changeable config

        public Frame(GroupComponentConfig? titlegroupConfig = null) : base()
        {
            if (titlegroupConfig is null)
                titlegroupConfig = (new TextLabel(""), 0);
            GroupComponentConfig config = (GroupComponentConfig)titlegroupConfig;
            _innerContainer.Add(inner);
            _title = config;
            _titleContainer.Add(config.Component);
            Add(
                _frameInner = new VerticalGroupComponent() {
                    (new HorizontalGroupComponent() {
                        (new TextLabel("┌─"), 2),
                        (_outerGroupComponent = new HorizontalGroupComponent() {
                            (_titleContainer, config.SplitAmount),
                            (new HorizontalBar('─'), (new Fraction(1, 1), 2))
                        }, new Fraction(1, 1)),
                        (new TextLabel("─┐"), 2)
                    }, 1),
                    (
                        new HorizontalGroupComponent() {
                            (new VerticalBar('│'), 1),
                            (new VerticalBar(' '), 1),
                            (_innerContainer, new Fraction(1, 1)),
                            (new VerticalBar(' '), 1),
                            (new VerticalBar('│'), 1)
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
            IComponent original = title.Component;
            SplitAmount oriAmount = title.SplitAmount;
            if (comp.Component is null) return;
            if (original is null || original != comp.Component)
            {
                if (!(original is null))
                    _titleContainer.RemoveChildComponent(original);
                _titleContainer.Add(comp.Component);
                _titleContainer.SetHasUpdate();
            }
            if (oriAmount != comp.SplitAmount)
            {
                _outerGroupComponent.UpdateSplitConfig(_titleContainer, comp.SplitAmount);
                _outerGroupComponent.SetHasUpdate();
            }
            comp.Component.SetHasUpdate();
            _title = (comp.Component, comp.SplitAmount);
            SetHasUpdate();
        }

        public void SwitchInner(IComponent comp)
        {
            IComponent original = inner;
            if (comp is null) return;
            if (original is null || original != comp)
            {
                if (!(original is null))
                    _innerContainer.RemoveChildComponent(original);
                _innerContainer.Add(comp);
                _innerContainer.SetHasUpdate();
            }
            _inner = comp;
            comp.SetHasUpdate();
            SetHasUpdate();
        }

        protected override void OnResize()
        {
            if ((GetAllocSize().x < 3 || GetAllocSize().y < 3) && this.GetInner() == _frameInner && ToggleCanNoFrame)
            {
                this.RemoveChildComponent(_frameInner);
                _innerContainer.RemoveChildComponent(inner);
                this.Add(inner);
                SetHasUpdate();
            }
            else if ((GetAllocSize().x >= 3 && GetAllocSize().y >= 3 || !ToggleCanNoFrame) && this.GetInner() == inner)
            {
                this.RemoveChildComponent(inner);
                this.Add(_frameInner);
                _innerContainer.Add(inner);
                SetHasUpdate();
            }
            base.OnResize();
        }
    }
}
