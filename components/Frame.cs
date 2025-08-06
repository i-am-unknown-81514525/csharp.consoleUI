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

        private readonly IComponent frameInner;
        private readonly Container titleContainer = new Container();
        private readonly HorizontalGroupComponent outerGroupComponent = null;
        private readonly Container innerContainer = new Container();

        private const bool toggleCanNoFrame = true; // future changeable config

        public Frame(GroupComponentConfig? titlegroupConfig = null) : base()
        {
            if (titlegroupConfig is null)
                titlegroupConfig = (new TextLabel(""), 0);
            GroupComponentConfig config = (GroupComponentConfig)titlegroupConfig;
            innerContainer.Add(inner);
            _title = config;
            titleContainer.Add(config.component);
            Add(
                frameInner = new VerticalGroupComponent() {
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
            IComponent original = title.component;
            SplitAmount ori_amount = title.splitAmount;
            if (comp.component is null) return;
            if (original is null || original != comp.component)
            {
                if (!(original is null))
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
            if (comp is null) return;
            if (original is null || original != comp)
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

        protected override void OnResize()
        {
            if ((GetAllocSize().x < 4 || GetAllocSize().y < 4) && this.GetInner() == frameInner && toggleCanNoFrame)
            {
                this.RemoveChildComponent(frameInner);
                innerContainer.RemoveChildComponent(inner);
                this.Add(inner);
                SetHasUpdate();
            }
            else if ((GetAllocSize().x >= 4 && GetAllocSize().y >= 4 || !toggleCanNoFrame) && this.GetInner() == inner)
            {
                this.RemoveChildComponent(inner);
                this.Add(frameInner);
                innerContainer.Add(inner);
                SetHasUpdate();
            }
            base.OnResize();
        }

        // protected override ConsoleContent[,] RenderPost(ConsoleContent[,] content)
        // {
        //     if ((GetAllocSize().x < 4 || GetAllocSize().y < 4) && this.GetInner() == frameInner && toggleCanNoFrame)
        //     {
        //         this.RemoveChildComponent(frameInner);
        //         innerContainer.RemoveChildComponent(inner);
        //         this.Add(inner);
        //         SetHasUpdate();
        //         ConsoleContent[,] result = Render();
        //         return result;
        //     }
        //     else if ((GetAllocSize().x >= 4 || GetAllocSize().y >= 4 || !toggleCanNoFrame) && this.GetInner() == inner)
        //     {
        //         this.RemoveChildComponent(inner);
        //         this.Add(frameInner);
        //         innerContainer.Add(inner);
        //         SetHasUpdate();
        //         ConsoleContent[,] result = Render();
        //         return result;
        //     }
        //     return content;
        // }
    }
}
