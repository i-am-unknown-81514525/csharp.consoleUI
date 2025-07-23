using System;
using System.Linq;
using System.Collections.Generic;
using ui.core;
using ui.math;
using ui.utils;
using static ui.utils.Array2DHandler;
using System.Collections;

namespace ui.components
{
    // using ComponentLocation = (IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity);

    public class UnpermitHierarchyChangeException : InvalidOperationException
    {
        public UnpermitHierarchyChangeException(string message) : base(message) { }
        public UnpermitHierarchyChangeException() : base() { }
    }

    public class AlreadyInitException : InvalidOperationException { }

    public abstract class Component : IComponent, IEnumerable<IComponent>
    {
        protected ActiveStatusHandler activeHandler;
        private IComponent root = null;

        private (uint x, uint y) allocSize = (0, 0);
        protected List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> childsMapping =
                new List<(IComponent, (uint, uint, uint, uint), int)>(); // Lower value -> earlier to render = lower prioity (being override by higher value)
                                                                         // The component writer decide itself override on other or other overide on itself by call order

        protected ConsoleContent[,] contentPlace = new ConsoleContent[0, 0];

        protected bool noParent = false;

        private bool _localHasUpdate = true;

        private bool _lock = false; // Any active change called to upper level class would enable this lock

        private bool _isActive = false;

        private bool _active_lock = false; // When onDeactive, calling setActive would have the unintend side-effect, 
                                           // where _isActive would is still depend on onDeactive return result, not the calling of onActive
                                           // Or under undiscover side effect (UB)
                                           // Therefore the usage of such is restricted with apporiate error message
                                           // To ask the developer to return false instead.

        private bool _frame_recurr_lock = false;

        private bool _isInit = false;

        public Component(ComponentConfig config)
        {
            Init(config);
        }

        public Component()
        {
            _isInit = false;
        }

        public bool isInit() => _isInit;

        public void Init(ComponentConfig config)
        {
            if (isInit()) throw new AlreadyInitException();
            activeHandler = config.activeStatusHandler;
            _isInit = true;
        }

        protected List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> GetMapping()
        {
            return childsMapping.ToList();
        }

        public (uint x, uint y) GetAllocSize()
        {
            return allocSize;
        }

        protected virtual void onFrameInternal() { }

        public void onFrame()
        {
            if (_frame_recurr_lock) return;
            try
            {
                _frame_recurr_lock = true;
                onFrameInternal();
                if (childsMapping != null)
                    foreach (var compLoc in childsMapping)
                        compLoc.component.onFrame();
            }
            finally
            {
                _frame_recurr_lock = false;
            }
        }

        public bool GetHasUpdate()
        {
            return _localHasUpdate || childsMapping.Any(x => x.component.GetHasUpdate());
        }

        protected void SetHasUpdate()
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

        public IComponent getParent()
        {
            return root;
        }

        public void setParent(IComponent component)
        {
            CheckLock();
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


        protected virtual void onHoverInternal(ConsoleLocation location) { }

        public virtual void onHover(ConsoleLocation location)
        {
            CheckLock();
            onHoverInternal(location);
            foreach (var compLoc in childsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= location.x && location.x <= lx && ly <= location.y && location.y <= hy)
                {
                    _lock = true;
                    try
                    {
                        compLoc.component.onHover(location);
                    }
                    finally
                    {
                        _lock = false;
                    }
                }
            }
        }
        protected bool onClickPropagate(ConsoleLocation pressLocation)
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

        public virtual void onClick(ConsoleLocation pressLocation)
        {
            bool isPropagate = onClickPropagate(pressLocation);
        }

        public (uint x, uint y) GetChildAllocatedSize(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The parent doesn't have this direct child");
            }
            (uint _, uint _2, uint allocX, uint allocY) = childsMapping.Where(x => x.component == component).First().location;
            return (allocX, allocY);
        }

        public bool Contains(IComponent component)
        {
            return childsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        public int IndexOf(IComponent component)
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

        protected virtual void onResize() { }

        protected void setSize(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int? prioity = null)
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

        protected void Remove(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            int idx = IndexOf(component);
            childsMapping.RemoveAt(idx);
        }

        protected virtual (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) onAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (true, child);
        }

        public bool AddChildComponent(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int prioity)
        {
            CheckLock();
            (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) = onAddHandler((component, loc, prioity));
            if (!isAdd)
            {
                return false;
            }
            childsMapping.Add(data);
            component.setParent(this);
            component.Init(new ComponentConfig(activeHandler));
            SetHasUpdate();
            return true;
        }

        public ConsoleContent[,] Render()
        {
            CheckLock();
            bool hasResize = UpdateAllocSize();
            if (hasResize)
            {
                SetHasUpdate();
                onResize();
            }
            if (!GetHasUpdate())
            {
                return (ConsoleContent[,])contentPlace.Clone();
            }
            _localHasUpdate = false;
            ConsoleContent[,] content = RenderInternal();
            contentPlace = content;
            return content;
        }

        protected virtual ConsoleContent[,] RenderInternal()
        {
            CheckLock();
            ConsoleContent[,] newArr = new ConsoleContent[allocSize.x, allocSize.y];
            RenderPre(newArr);
            _lock = true;
            try
            {
                foreach (
                    (IComponent component, (uint x, uint y, uint allocX, uint allocY) meta, int prioity) compLoc
                    in childsMapping.OrderBy(x => x.prioity)
                )
                {
                    newArr = CopyTo(
                        changeSize(
                            compLoc.component.Render(),
                            (compLoc.meta.allocX, compLoc.meta.allocY),
                            ConsoleContent.getDefault()
                        ),
                        newArr,
                        (compLoc.meta.x, compLoc.meta.y)
                    );
                }
            }
            finally
            {
                _lock = false;
            }
            RenderPost(newArr);
            return newArr;
        }

        protected virtual ConsoleContent[,] RenderPre(ConsoleContent[,] content) => content;

        protected virtual ConsoleContent[,] RenderPost(ConsoleContent[,] content) => content;


        public bool Deactive(Event deactiveEvent)
        {
            try
            {
                _active_lock = true;
                return onDeactive(deactiveEvent);
            }
            finally
            {
                _active_lock = false;
            }
        }

        protected virtual bool onDeactive(Event deactiveEvent)
        {
            return true;
        }

        public bool isRequestingActive()
        {
            return _isActive;
        }

        protected bool setActive(Event activeEvent)
        {
            if (_active_lock) throw new InvalidOperationException("Cannot setActive when running onDeactive handler, return false instead.");
            try
            {
                _isActive = true;
                _isActive = activeHandler.setActive(this);
            }
            catch
            {
                _isActive = false;
                if (activeHandler.getCurrActive() == this) activeHandler.setInactive(this);
                throw;
            }
            if (_isActive)
            {
                onActive();
            }
            return _isActive;
        }

        protected virtual void onActive() { }

        protected void setInactive()
        {
            if (activeHandler.getCurrActive() != this) return;
            activeHandler.setInactive(this);
        }


        public bool isRequestingDeactive()
        {
            return !_isActive;
        }

        public virtual Event ActiveRequest()
        {
            return null;
        }

        public IEnumerator<IComponent> GetEnumerator()
        {
            return GetMapping().Select(x => x.component).GetEnumerator();
        }

        public void Add(Component component)
        {
            if (component.getParent() != null && component.getParent() != this) throw new InvalidOperationException("The component already have a parent");
            (uint allocX, uint allocY) = GetAllocSize();
            AddChildComponent(component, (0, 0, allocX, allocY), 1);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Debug_WriteStructure()
        {
            string clsName = GetType().Name;
            string content = "";
            if (root == null)
                content = $"{clsName} (allocX={GetAllocSize().x}, allocY={GetAllocSize().y}) - {Debug_Info() ?? ""}\r\n";
            string inner = String.Join(
                "\n",
                childsMapping
                    .Select(x => x)
                    .Select(
                        x =>
                        $"    {x.component.GetType().Name} (x={x.location.x}, y={x.location.y}, allocX={x.location.allocX}, allocY={x.location.allocY}) - {x.component.Debug_Info() ?? ""}\r\n" +
                        String.Join("\r\n", x.component.Debug_WriteStructure().Split('\n').Select(y=>"    " + y.TrimEnd('\r')))
                    )
            );

            return content + inner;

        }

        public virtual string Debug_Info()
        {
            return "";
        }
    }
}