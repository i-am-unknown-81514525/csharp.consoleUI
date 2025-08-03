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

    public abstract class Component : IComponent
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

        private bool _activeLock = false; // When onDeactive, calling setActive would have the unintend side-effect,
                                          // where _isActive would is still depend on onDeactive return result, not the calling of onActive
                                          // Or under undiscover side effect (UB)
                                          // Therefore the usage of such is restricted with apporiate error message
                                          // To ask the developer to return false instead.

        private bool _frameRecurrLock = false;

        private bool _isInit = false;

        public Component(ComponentConfig config)
        {
            Init(config);
        }

        public Component()
        {
            _isInit = false;
        }

        public bool IsInit() => _isInit;

        public void Init(ComponentConfig config)
        {
            if (IsInit()) throw new AlreadyInitException();
            activeHandler = config.activeStatusHandler;
            _isInit = true;
            foreach (IComponent component in GetMapping().Select(x => x.component))
                if (!component.IsInit())
                    component.Init(config);
        }

        protected List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> GetMapping()
        {
            return childsMapping.ToList();
        }

        public (uint x, uint y) GetAllocSize()
        {
            return allocSize;
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
                OnResize();
                return true;
            }
            return false;
        }

        public void OnFrame()
        {
            if (_frameRecurrLock) return;
            try
            {
                _frameRecurrLock = true;
                OnFrameInternal();
                if (childsMapping != null)
                    foreach (var compLoc in childsMapping)
                        compLoc.component.OnFrame();
            }
            finally
            {
                _frameRecurrLock = false;
            }
        }

        protected virtual void OnFrameInternal() { }

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

        public virtual void OnHover(ConsoleLocation location)
        {
            CheckLock();
            OnHoverInternal(location);
            foreach (var compLoc in childsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= location.x && location.x <= lx && ly <= location.y && location.y <= hy)
                {
                    _lock = true;
                    try
                    {
                        compLoc.component.OnHover(location);
                    }
                    finally
                    {
                        _lock = false;
                    }
                }
            }
        }

        protected virtual void OnHoverInternal(ConsoleLocation location) { }

        public virtual void OnClick(ConsoleLocation pressLocation)
        {
            bool isPropagate = OnClickPropagate(pressLocation);
        }

        protected bool OnClickPropagate(ConsoleLocation pressLocation)
        {
            foreach (var compLoc in childsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= pressLocation.x && pressLocation.x < hx && ly <= pressLocation.y && pressLocation.y < hy)
                {
                    compLoc.component.OnClick(new ConsoleLocation(pressLocation.x - (int)lx, pressLocation.y - (int)ly));
                    return true;
                }
            }
            return false;
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

        protected void SetChildAllocatedSize(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int? prioity = null)
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

        public bool Contains(IComponent component)
        {
            return childsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        public int IndexOf(IComponent component)
        {
            return childsMapping.Select(x => x.component).ToList().IndexOf(component);
        }

        protected virtual void OnResize() { }

        public bool AddChildComponent(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int prioity)
        {
            CheckLock();
            (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) = OnAddHandler((component, loc, prioity));
            if (!isAdd)
            {
                return false;
            }
            childsMapping.Add(data);
            component.Mount(this);
            if (activeHandler != null)
                component.Init(new ComponentConfig(activeHandler));
            SetHasUpdate();
            return true;
        }

        protected virtual (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (true, child);
        }

        protected void RemoveChildComponent(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            int idx = IndexOf(component);
            childsMapping.RemoveAt(idx);
            component.Dismount();
            SetHasUpdate();
        }

        public IComponent GetMount()
        {
            return root;
        }

        public void Mount(IComponent component)
        {
            CheckLock();
            if (component == null)
            {
                throw new InvalidOperationException("Cannot set mount to nothingness");
            }
            if (root != null)
            {
                throw new InvalidOperationException("The component is already mount, cannot modify");
            }
            if (noParent)
            {
                throw new InvalidOperationException("The component reject from being mount(usually occur to root node)");
            }
            root = component;
            OnMount();
            OnVisible();
        }

        protected virtual void OnMount()
        {

        }

        internal void OnVisible()
        {
            OnVisibleInternal();
            foreach (Component comp in this.GetMapping().Select(x => x.component))
            {
                comp.OnVisible();
            }
        }

        protected virtual void OnVisibleInternal()
        {

        }

        public bool Dismount()
        {
            if (root is null) throw new InvalidOperationException("The item is already dismounted");
            if (!root.Contains(this))
            {
                OnDismount();
                OnHide();
                root = null;
                return true;
            }
            return false;
        }

        protected virtual void OnDismount()
        {

        }

        internal void OnHide()
        {
            OnVisibleInternal();
            foreach (Component comp in this.GetMapping().Select(x => x.component))
            {
                comp.OnVisible();
            }
        }

        protected virtual void OnHideInternal() { }

        public ConsoleContent[,] Render()
        {
            CheckLock();
            UpdateAllocSize();
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
            newArr = RenderPre(newArr);
            if (newArr.GetLength(0) != allocSize.x || newArr.GetLength(1) != allocSize.y) throw new InvalidOperationException("Cannot change size of ConsoleContent array on RenderPre");
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
            newArr = RenderPost(newArr);
            if (newArr.GetLength(0) != allocSize.x || newArr.GetLength(1) != allocSize.y) throw new InvalidOperationException("Cannot change size of ConsoleContent array on RenderPost");
            return newArr;
        }

        protected virtual ConsoleContent[,] RenderPre(ConsoleContent[,] content) => content;

        protected virtual ConsoleContent[,] RenderPost(ConsoleContent[,] content) => content;


        public bool Deactive(Event deactiveEvent)
        {
            try
            {
                _activeLock = true;
                return OnDeactive(deactiveEvent);
            }
            finally
            {
                _activeLock = false;
            }
        }

        public bool IsActive()
        {
            return activeHandler.GetCurrActive() == this;
        }

        protected virtual bool OnDeactive(Event deactiveEvent)
        {
            return true;
        }

        public bool IsRequestingActive()
        {
            return _isActive;
        }

        protected bool SetActive(Event activeEvent)
        {
            if (_activeLock) throw new InvalidOperationException("Cannot setActive when running onDeactive handler, return false instead.");
            try
            {
                _isActive = true;
                _isActive = activeHandler.SetActive(this);
            }
            catch
            {
                _isActive = false;
                if (activeHandler.GetCurrActive() == this) activeHandler.SetInactive(this);
                throw;
            }
            if (_isActive)
            {
                OnActive();
            }
            return _isActive;
        }

        protected virtual void OnActive() { }

        protected void SetInactive()
        {
            if (activeHandler.GetCurrActive() != this) return;
            this._isActive = false;
            activeHandler.SetInactive(this);
            OnDeactive(null);
        }


        public bool IsRequestingDeactive()
        {
            return !_isActive;
        }

        public virtual Event ActiveRequest()
        {
            return null;
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
                        String.Join("\r\n", x.component.Debug_WriteStructure().Split('\n').Select(y => "    " + y.TrimEnd('\r')))
                    )
            );

            return content + inner;

        }

        public virtual string Debug_Info()
        {
            return "";
        }

        public (int row, int col) GetAbsolutePos((int row, int col) pos, IComponent childComp)
        {
            if (!Contains(childComp))
            {
                throw new InvalidOperationException("The childComp given is not the child of the given component");
            }
            (uint x, uint y, uint allocX, uint allocY) = GetMapping().Where(v => v.component == childComp).First().location;
            pos = (pos.row + (int)y, pos.col + (int)x);
            return GetAbsolutePos(pos);
        }

        public (int row, int col) GetAbsolutePos((int row, int col) pos)
        {
            IComponent parent = GetMount();
            if (parent is null) return pos;
            return parent.GetAbsolutePos(pos, this);
        }
    }
}
