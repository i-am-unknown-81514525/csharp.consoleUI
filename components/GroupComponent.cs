using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.core;
using ui.math;
using ui.utils;

namespace ui.components
{
    public enum Direction
    {
        VERTICAL = 0,
        HORIZONTAL = 1,
    }

    public struct GroupComponentConfig
    {
        public readonly IComponent Component;
        public readonly SplitAmount SplitAmount;

        public GroupComponentConfig(IComponent component, SplitAmount splitAmount)
        {
            Component = component;
            SplitAmount = splitAmount;
        }

        public static implicit operator GroupComponentConfig(
            (IComponent component, SplitAmount splitAmount) config
        ) => new GroupComponentConfig(config.component, config.splitAmount);

        public static implicit operator GroupComponentConfig(BaseComponent component) => new GroupComponentConfig(component, (Fraction)1);

    }

    public abstract class GroupComponent<T> : Component<T>, IEnumerable<GroupComponentConfig> where T : ComponentStore
    {
        protected SplitHandler SplitHandler = new SplitHandler(120); // Need to be update on initial
        protected Direction Direction;
        protected Dictionary<IComponent, SplitConfig> SplitMapping = new Dictionary<IComponent, SplitConfig>();

        protected GroupComponent()
        {
        }

        protected GroupComponent(T store) : base(store)
        {
        }

        protected GroupComponent(ComponentConfig config) : base(config)
        {
        }

        protected GroupComponent(ComponentConfig config, T store) : base(config, store)
        {
        }

        public void Add(GroupComponentConfig componentConfig)
        {
            IComponent component = componentConfig.Component;
            if (component.GetMount() != null && component.GetMount() != this) throw new InvalidOperationException("The component already have a parent");
            (uint allocX, uint allocY) = GetAllocSize();
            bool isSuccess = AddChildComponent(component, (0, 0, allocX, allocY), 1);
            if (isSuccess)
            {
                SplitConfig config = SplitHandler.AddSplit(componentConfig.SplitAmount);
                SplitMapping[component] = config;
            }
            UpdateSize();
            SetHasUpdate();
        }

        public void Insert(int idx, GroupComponentConfig componentConfig)
        {
            IComponent component = componentConfig.Component;
            if (component.GetMount() != null && component.GetMount() != this) throw new InvalidOperationException("The component already have a parent");
            (uint allocX, uint allocY) = GetAllocSize();
            bool isSuccess = InsertChildComponent(idx, component, (0, 0, allocX, allocY), 1);
            if (isSuccess)
            {
                SplitConfig config = SplitHandler.AddSplit(componentConfig.SplitAmount);
                SplitMapping[component] = config;
            }
            UpdateSize();
            SetHasUpdate();
        }

        public void UpdateSplitConfig(IComponent component, SplitAmount amount)
        {
            SplitConfig config = SplitMapping[component];
            SplitHandler.Remove(config);
            SplitConfig newConfig = SplitHandler.AddSplit(amount);
            SplitMapping[component] = newConfig;
            UpdateSize();
            SetHasUpdate();
        }

        public new bool Contains(IComponent component)
        {
            return ChildsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        public new void RemoveChildComponent(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            base.RemoveChildComponent(component);
            SplitConfig splitConfig = SplitMapping[component];
            SplitMapping.Remove(component);
            SplitHandler.Remove(splitConfig);
            UpdateSize();
        }

        protected void SyncMapping()
        {
            foreach (var split in SplitMapping)
            {
                if (!ChildsMapping.Select(x => x.component).Contains(split.Key))
                {
                    // childsMapping.Add((split.Key, (0, 0, 0, 0), 1));
                    AddChildComponent(split.Key, (0, 0, 0, 0), 1);
                }
            }
            int currIdx = ChildsMapping.Count;
            for (int idx = 0; idx < currIdx; idx++)
            {
                if (!SplitMapping.ContainsKey(ChildsMapping[idx].component))
                {
                    RemoveChildComponent(ChildsMapping[idx].component);
                    // childsMapping.RemoveAt(idx);
                    idx--;
                    currIdx--;
                }
            }
        }

        public void UpdateSize()
        {
            SplitHandler.Update();
            SyncMapping();
            // childsMapping = new List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)>();
            uint curr = 0;
            int count = ChildsMapping.Count;
            for (int idx = 0; idx < count; idx++)
            {
                (IComponent component, (uint x, uint y, uint allocX, uint allocY) _, int prioity) = ChildsMapping[idx];
                SplitConfig config = SplitMapping[component];
                System.Diagnostics.Debug.Assert(SplitHandler != null, nameof(SplitHandler) + " != null");
                uint size = (uint)SplitHandler.GetSize(config);
                if (Direction == Direction.VERTICAL)
                {
                    ChildsMapping[idx] = (component, (0, curr, GetAllocSize().x, size), prioity);
                }
                else
                {
                    ChildsMapping[idx] = (component, (curr, 0, size, GetAllocSize().y), prioity);
                }
                curr += size;
                component.UpdateAllocSize();
            }

        }

        protected override void OnResize()
        {
            if (SplitHandler == null) return;
            if (Direction == Direction.VERTICAL)
            {
                SplitHandler.SetTotalSize((int)GetAllocSize().y);
            }
            else
            {
                SplitHandler.SetTotalSize((int)GetAllocSize().x);
            }
            SplitHandler.Update();
            UpdateSize();
            SetHasUpdate();
        }

        IEnumerator<GroupComponentConfig> IEnumerable<GroupComponentConfig>.GetEnumerator()
        {
            return new List<GroupComponentConfig>().GetEnumerator(); // as the detail config is not stored directly, and no reason to be read such way.
        }

        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable<GroupComponentConfig>)this).GetEnumerator();
        }

        protected override ConsoleContent[,] RenderPre(ConsoleContent[,] content)
        {
            for (int x = 0; x < GetAllocSize().x; x++)
            {
                for (int y = 0; y < GetAllocSize().y; y++)
                {
                    content[x, y] = new ConsoleContent
                    {
                        content = " ",
                        ansiPrefix = "",
                        ansiPostfix = "",
                        isContent = true
                    };
                }
            }
            return content;
        }
    }

    public abstract class GroupComponent : GroupComponent<EmptyStore>
    {
    }
}
