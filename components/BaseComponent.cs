using System;
using System.Linq;
using System.Collections.Generic;
using ui.core;
using ui.utils;
using static ui.utils.Array2DHandler;

namespace ui.components
{

    public class UnpermitHierarchyChangeException : InvalidOperationException
    {
        public UnpermitHierarchyChangeException(string message) : base(message) {}
        public UnpermitHierarchyChangeException() : base() {}
    }

    public abstract class BaseComponent
    {
        private BaseComponent root;

        private (uint x, uint y) allocSize;
        private List<(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) location)> childsMapping =
                new List<(BaseComponent, (uint, uint, uint, uint))>();

        internal ConsoleContent[,] contentPlace = new ConsoleContent[0, 0];

        private bool _localHasUpdate = false;

        private bool _lock = false; // Any active change called to upper level class would enable this lock

        public bool GetHasUpdate()
        {
            return _localHasUpdate || childsMapping.Any(x => x.component.GetHasUpdate());
        }

        internal void SetHasUpdate()
        {
            _localHasUpdate = true;
        }

        private void CheckLock()
        {
            if (_lock)
            {
                throw new UnpermitHierarchyChangeException("Cannot call parent change function from child");
            }
        }

        public (uint x, uint y) GetChildAllocatedSize(BaseComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The parent doesn't have this direct child");
            }
            (uint _, uint _2, uint allocX, uint allocY) = childsMapping.Where(x => x.component == component).First().location;
            return (allocX, allocY);
        }

        public bool Contains(BaseComponent component)
        {
            return childsMapping.Select(x=>x.component).Count(x=>x==component) > 0;
        }

        public int IndexOf(BaseComponent component)
        {
            return childsMapping.Select(x=>x.component).ToList().IndexOf(component);
        }

        public bool UpdateAllocSize()
        {
            (uint x, uint y) old = allocSize;
            if (root == null)
            {
                ConsoleSize size = Global.consoleCanva.GetConsoleSize();
                allocSize = ((uint)size.Width, (uint)size.Height);
            }
            else
            {
                allocSize = root.GetChildAllocatedSize(this);
            }
            if (old != allocSize)
            {
                SetHasUpdate();
                return true;
            }
            return false;
        }

        internal abstract void onResize();

        internal void setSize(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) loc)
        {
            if (!Contains(component))
            {
                childsMapping.Add((component, loc));
                SetHasUpdate();
            }
            else
            {
                int idx = IndexOf(component);
                if (loc != childsMapping[idx].location)
                    SetHasUpdate();
                childsMapping[idx] = (component, loc);
            }
        }

        internal void Remove(BaseComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            int idx = IndexOf(component);
            childsMapping.RemoveAt(idx);
        }

        internal virtual (bool isAdd, (BaseComponent, (uint, uint, uint, uint)) data) onAddHandler((BaseComponent, (uint, uint, uint, uint)) child)
        {
            return (true, child);
        }

        public bool AddChildComponent(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) loc)
        {
            (bool isAdd, (BaseComponent, (uint, uint, uint, uint)) data) = onAddHandler((component, loc));
            if (!isAdd)
            {
                return false;
            }
            childsMapping.Add(data);
            SetHasUpdate();
            return true;
        }

        public virtual ConsoleContent[,] Render()
        {
            CheckLock();
            bool hasResize = UpdateAllocSize();
            if (hasResize)
            {
                onResize();
            }
            if (!GetHasUpdate())
            {
                return (ConsoleContent[,])contentPlace.Clone();
            }
            ConsoleContent[,] newArr = new ConsoleContent[allocSize.x, allocSize.y];
            _lock = true;
            try
            {
                foreach ((BaseComponent component, (uint x, uint y, uint allocX, uint allocY) meta) compLoc in childsMapping)
                {
                    newArr = CopyTo(
                        changeSize(
                            compLoc.component.Render(),
                            (compLoc.meta.allocX, compLoc.meta.allocY),
                            new ConsoleContent
                            {
                                content = " ",
                                ansiPrefix = "",
                                ansiPostfix = ""
                            }
                        ),
                        newArr,
                        (compLoc.meta.x, compLoc.meta.y))
                    ;
                }
                _lock = false;
                return newArr;
            }
            catch
            {
                _lock = false;
                throw;
            }
        }

    }
}