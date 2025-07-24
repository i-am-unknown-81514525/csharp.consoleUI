using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.utils;
using ui.math;

namespace ui.components
{
    public enum Direction
    {
        VERTICAL = 0,
        HORIZONTAL = 1,
    }

    public struct GroupComponentConfig
    {
        public IComponent component;
        public SplitAmount splitAmount;

        public GroupComponentConfig(IComponent component, SplitAmount splitAmount)
        {
            this.component = component;
            this.splitAmount = splitAmount;
        }

        public static implicit operator GroupComponentConfig(
            (IComponent component, SplitAmount splitAmount) config
        ) => new GroupComponentConfig(config.component, config.splitAmount);

        public static implicit operator GroupComponentConfig(Component component) => new GroupComponentConfig(component, (Fraction)1);
    
    }

    public abstract class GroupComponent : Component, IEnumerable<GroupComponentConfig>
    {
        protected SplitHandler splitHandler = new SplitHandler(120); // Need to be update on initial
        protected Direction direction;
        protected Dictionary<IComponent, SplitConfig> splitMapping = new Dictionary<IComponent, SplitConfig>();

        protected GroupComponent() : base()
        {
        }

        public void Add(GroupComponentConfig componentConfig)
        {
            IComponent component = componentConfig.component;
            if (component.getParent() != null && component.getParent() != this) throw new InvalidOperationException("The component already have a parent");
            (uint allocX, uint allocY) = GetAllocSize();
            bool isSuccess = AddChildComponent(component, (0, 0, allocX, allocY), 1);
            if (isSuccess)
            {
                SplitConfig config = splitHandler.AddSplit(componentConfig.splitAmount);
                splitMapping[component] = config;
            }
        }

        public new bool Contains(IComponent component)
        {
            return childsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        protected new void Remove(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            int idx = IndexOf(component);
            if (idx != -1)
                childsMapping.RemoveAt(idx);
            SplitConfig splitConfig = splitMapping[component];
            splitMapping.Remove(component);
            splitHandler.Remove(splitConfig);
        }

        protected void SyncMapping()
        {
            foreach (var split in splitMapping)
            {
                if (!childsMapping.Select(x => x.component).Contains(split.Key))
                {
                    childsMapping.Add((split.Key, (0, 0, 0, 0), 1));
                }
            }
            int curr_idx = childsMapping.Count;
            for (int idx = 0; idx < curr_idx; idx++)
            {
                if (!splitMapping.ContainsKey(childsMapping[idx].component))
                {
                    childsMapping.RemoveAt(idx);
                    idx--;
                    curr_idx--;
                }
            }
        }

        public void UpdateSize()
        {
            splitHandler.Update();
            SyncMapping();
            // childsMapping = new List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)>();
            uint curr = 0;
            int count = childsMapping.Count;
            for (int idx = 0; idx < count; idx++)
            {
                (IComponent component, (uint x, uint y, uint allocX, uint allocY) _, int prioity) = childsMapping[idx];
                SplitConfig config = splitMapping[component];
                uint size = (uint)splitHandler.GetSize(config);
                if (direction == Direction.VERTICAL)
                {
                    childsMapping[idx] = (component, (0, curr, GetAllocSize().x, size), prioity);
                }
                else
                {
                    childsMapping[idx] = (component, (curr, 0, size, GetAllocSize().y), prioity);
                }
                curr += size;
                component.UpdateAllocSize();
            }

        }

        protected override void onResize()
        {
            if (splitHandler == null) return;
            if (direction == Direction.VERTICAL)
            {
                splitHandler.SetTotalSize((int)GetAllocSize().y);
            }
            else
            {
                splitHandler.SetTotalSize((int)GetAllocSize().x);
            }
            splitHandler.Update();
            UpdateSize();
            SetHasUpdate();
        }

        IEnumerator<GroupComponentConfig> IEnumerable<GroupComponentConfig>.GetEnumerator()
        {
            return new List<GroupComponentConfig>().GetEnumerator(); // as the detail config is not stored directly, and no reason to be read such way.
        }

        public IEnumerator GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}