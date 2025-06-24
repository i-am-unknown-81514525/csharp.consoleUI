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
        public UnpermitHierarchyChangeException(string message) : base(message) { }
        public UnpermitHierarchyChangeException() : base() { }
    }

    public class DeactiveIgnoreNotice : Exception { }

    public abstract class BaseComponent
    {
        private BaseComponent root = null;

        private (uint x, uint y) allocSize = (0, 0);
        private List<(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> childsMapping =
                new List<(BaseComponent, (uint, uint, uint, uint), int)>(); // Lower value -> earlier to render = lower prioity (being override by higher value)
                                                                            // The component writer decide itself override on other or other overide on itself by call order

        internal ConsoleContent[,] contentPlace = new ConsoleContent[0, 0];

        internal bool noParent = false;

        private bool _localHasUpdate = true;

        private bool _lock = false; // Any active change called to upper level class would enable this lock

        private bool _isActive = false;

        private bool _deactive_recurr_lock = false;

        private bool _frame_recurr_lock = false;

        public bool getIsActive() => _isActive;

        public bool getContainsActive()
        {
            return getIsActive() || childsMapping.Select(x => x.component.getContainsActive()).Any();
        }

        internal List<(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> GetMapping()
        {
            return childsMapping.ToList();
        }

        public (uint x, uint y) GetAllocSize()
        {
            return allocSize;
        }

        public void DeactiveAll()
        {
            if (_deactive_recurr_lock) return;
            _deactive_recurr_lock = true;
            try
            {
                if (getIsActive())
                {
                    _isActive = false;
                }
                else if (getContainsActive())
                {
                    foreach (BaseComponent comp in childsMapping.Select(x => x.component).ToList())
                    {
                        comp.DeactiveAll();
                    }
                }
                else
                {
                    if (root != null)
                    {
                        root.DeactiveAll();
                    }
                }
                _deactive_recurr_lock = false;
            }
            catch
            {
                _deactive_recurr_lock = false;
                throw;
            }
        }

        internal bool setActive()
        {
            try
            {
                DeactiveAll();
                _isActive = true;
                _localHasUpdate = true;
                onActive();
                return true;
            }
            catch (DeactiveIgnoreNotice)
            {
                return false;
            }
        }

        internal virtual void onActive() { }

        internal virtual void onFrame() { }

        public void onFrameExternal()
        {
            if (_frame_recurr_lock) return;
            try
            {
                _frame_recurr_lock = true;
                onFrame();
                if (childsMapping != null)
                    foreach (var compLoc in childsMapping)
                        compLoc.component.onFrameExternal();
                _frame_recurr_lock = false;
            }
            catch
            {
                _frame_recurr_lock = false;
                throw;
            }
        }

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

        public BaseComponent getParent()
        {
            return root;
        }

        public void setParent(BaseComponent component)
        {
            if (component == null)
            {
                throw new InvalidOperationException("Cannot set parent to empty");
            }
            if (root != null)
            {
                throw new InvalidOperationException("A parent already exist for the component, cannot modify");
            }
            if (noParent)
            {
                throw new InvalidOperationException("The component reject having a parent");
            }
            root = component;
        }

        internal bool ClickPropagationHandler(ConsoleLocation pressLocation)
        {
            foreach (var compLoc in childsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= pressLocation.x && pressLocation.x <= lx && ly <= pressLocation.y && pressLocation.y <= hy)
                {
                    compLoc.component.onClick(pressLocation);
                    return true;
                }
            }
            return false;
        }

        internal virtual void onHover(ConsoleLocation location) { }

        public virtual void onHoverExternal(ConsoleLocation location)
        {
            onHover(location);
            foreach (var compLoc in childsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= location.x && location.x <= lx && ly <= location.y && location.y <= hy)
                {
                    compLoc.component.onHover(location);
                }
            }
        }

        internal virtual void onClick(ConsoleLocation pressLocation)
        {
            bool isPropagate = ClickPropagationHandler(pressLocation);
            if (!isPropagate)
                setActive();
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
            return childsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        public int IndexOf(BaseComponent component)
        {
            return childsMapping.Select(x => x.component).ToList().IndexOf(component);
        }

        public bool UpdateAllocSize()
        {
            CheckLock();
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

        internal void setSize(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int? prioity = null)
        {
            if (!Contains(component))
            {
                if (prioity == null) prioity = 0;
                childsMapping.Add((component, loc, (int)prioity));
                SetHasUpdate();
            }
            else
            {
                int idx = IndexOf(component);
                if (prioity == null) prioity = childsMapping[idx].prioity;
                if (loc != childsMapping[idx].location || prioity != childsMapping[idx].prioity)
                    SetHasUpdate();
                childsMapping[idx] = (component, loc, (int)prioity);
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

        internal virtual (bool isAdd, (BaseComponent, (uint, uint, uint, uint), int) data) onAddHandler((BaseComponent, (uint, uint, uint, uint), int) child)
        {
            return (true, child);
        }

        public bool AddChildComponent(BaseComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int prioity)
        {
            CheckLock();
            (bool isAdd, (BaseComponent, (uint, uint, uint, uint), int) data) = onAddHandler((component, loc, prioity));
            if (!isAdd)
            {
                return false;
            }
            childsMapping.Add(data);
            SetHasUpdate();
            return true;
        }

        public ConsoleContent[,] RenderExternal()
        {
            CheckLock();
            if (!GetHasUpdate())
            {
                return (ConsoleContent[,])contentPlace.Clone();
            }
            _localHasUpdate = false;
            return Render();
        }

        internal virtual ConsoleContent[,] Render()
        {
            CheckLock();
            bool hasResize = UpdateAllocSize();
            if (hasResize)
            {
                onResize();
            }
            ConsoleContent[,] newArr = new ConsoleContent[allocSize.x, allocSize.y];
            _lock = true;
            try
            {
                foreach (
                    (BaseComponent component, (uint x, uint y, uint allocX, uint allocY) meta, int prioity) compLoc
                    in childsMapping.OrderBy(x => x.prioity)
                )
                {
                    newArr = CopyTo(
                        changeSize(
                            compLoc.component.RenderExternal(),
                            (compLoc.meta.allocX, compLoc.meta.allocY),
                            ConsoleContent.getDefault()
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