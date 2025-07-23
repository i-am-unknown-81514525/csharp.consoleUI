using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ui.utils;

namespace ui.components
{
    public enum Direction
    {
        VERTICAL = 0,
        HORIZONTAL = 1,
    }

    public abstract class GroupComponent : Component
    {
        protected SplitHandler splitHandler;
        protected Direction direction;

        protected GroupComponent(ComponentConfig config) : base(config)
        {
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
            SetHasUpdate();
        }
    }
}