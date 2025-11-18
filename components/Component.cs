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


    public abstract class BaseComponent : IComponent
    {
        protected ActiveStatusHandler ActiveHandler;
        private IComponent _root = null;

        private (uint x, uint y) _allocSize = (0, 0);
        protected List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> ChildsMapping =
                new List<(IComponent, (uint, uint, uint, uint), int)>(); // Lower value -> earlier to render = lower prioity (being override by higher value)
                                                                         // The component writer decide itself override on other or other overide on itself by call order

        protected ConsoleContent[,] ContentPlace = new ConsoleContent[0, 0];

        protected bool NoParent = false;

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

        private bool _rerender = false;

        public BaseComponent(ComponentConfig config)
        {
            Init(config);
        }

        public BaseComponent()
        {
            _isInit = false;
        }

        public bool IsInit() => _isInit;

        public void Init(ComponentConfig config)
        {
            if (IsInit()) throw new AlreadyInitException();
            ActiveHandler = config.ActiveStatusHandler;
            _isInit = true;
            foreach (IComponent component in GetMapping().Select(x => x.component))
                if (!component.IsInit())
                    component.Init(config);
        }

        public void UnInit()
        {
            if (GetMount() is null || GetMount().IsInit() == false)
                _isInit = false;
            foreach (IComponent component in GetMapping().Select(x => x.component))
                component.UnInit();
        }

        public List<(IComponent component, (uint x, uint y, uint allocX, uint allocY) location, int prioity)> GetMapping()
        {
            return ChildsMapping.ToList();
        }

        public (uint x, uint y) GetAllocSize()
        {
            return _allocSize;
        }

        public bool UpdateAllocSize()
        {
            CheckLock();
            (uint x, uint y) old = _allocSize;
            if (_root == null)
            {
                ConsoleSize size = Global.ConsoleCanva.GetConsoleSize();
                _allocSize = ((uint)size.Width, (uint)size.Height);
            }
            else
            {
                _allocSize = _root.GetChildAllocatedSize(this);
            }
            if (old != _allocSize)
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
                if (ChildsMapping != null)
                    foreach (var compLoc in ChildsMapping)
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
            return _localHasUpdate || ChildsMapping.Any(x => x.component.GetHasUpdate());
        }

        public void SetHasUpdate()
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
            foreach (var compLoc in ChildsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= location.X && location.X <= lx && ly <= location.Y && location.Y <= hy)
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
            foreach (var compLoc in ChildsMapping.OrderByDescending(x => x.prioity))
            {
                (uint lx, uint ly) = (compLoc.location.x, compLoc.location.y);
                (uint hx, uint hy) = (lx + compLoc.location.allocX, ly + compLoc.location.allocY);
                if (lx <= pressLocation.X && pressLocation.X < hx && ly <= pressLocation.Y && pressLocation.Y < hy)
                {
                    compLoc.component.OnClick(new ConsoleLocation(pressLocation.X - (int)lx, pressLocation.Y - (int)ly));
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
            (uint _, uint _2, uint allocX, uint allocY) = ChildsMapping.Where(x => x.component == component).First().location;
            return (allocX, allocY);
        }

        protected void SetChildAllocatedSize(IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int? prioity = null)
        {
            if (!Contains(component))
            {
                if (prioity == null) prioity = 0;
                ChildsMapping.Add((component, loc, (int)prioity));
                SetHasUpdate();
            }
            else
            {
                int idx = IndexOf(component);
                if (prioity == null) prioity = ChildsMapping[idx].prioity;
                if (loc != ChildsMapping[idx].location || prioity != ChildsMapping[idx].prioity)
                    SetHasUpdate();
                ChildsMapping[idx] = (component, loc, (int)prioity);
            }
        }

        public bool Contains(IComponent component)
        {
            return ChildsMapping.Select(x => x.component).Count(x => x == component) > 0;
        }

        public int IndexOf(IComponent component)
        {
            return ChildsMapping.Select(x => x.component).ToList().IndexOf(component);
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
            ChildsMapping.Add(data);
            component.Mount(this);
            if (ActiveHandler != null)
                component.Init(new ComponentConfig(ActiveHandler));
            ReRender();
            SetHasUpdate();
            return true;
        }

        public bool InsertChildComponent(int idx, IComponent component, (uint x, uint y, uint allocX, uint allocY) loc, int prioity)
        {
            CheckLock();
            (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) = OnAddHandler((component, loc, prioity));
            if (!isAdd)
            {
                return false;
            }
            ChildsMapping.Insert(idx, data);
            component.Mount(this);
            if (ActiveHandler != null)
                component.Init(new ComponentConfig(ActiveHandler));
            ReRender();
            SetHasUpdate();
            return true;
        }

        protected virtual (bool isAdd, (IComponent, (uint, uint, uint, uint), int) data) OnAddHandler((IComponent, (uint, uint, uint, uint), int) child)
        {
            return (true, child);
        }

        public void RemoveChildComponent(IComponent component)
        {
            if (!Contains(component))
            {
                throw new InvalidOperationException("The component is not the direct child of the current component and cannot be removed");
            }
            int idx = IndexOf(component);
            ChildsMapping.RemoveAt(idx);
            component.Dismount();
            component.UnInit();
            ReRender();
            SetHasUpdate();
        }

        public IComponent GetMount()
        {
            return _root;
        }

        public void Mount(IComponent component)
        {
            CheckLock();
            if (component == null)
            {
                throw new InvalidOperationException("Cannot set mount to nothingness");
            }
            if (_root != null)
            {
                throw new InvalidOperationException("The component is already mount, cannot modify");
            }
            if (NoParent)
            {
                throw new InvalidOperationException("The component reject from being mount(usually occur to root node)");
            }
            _root = component;
            OnMount();
            OnVisible();
        }

        protected virtual void OnMount()
        {

        }

        internal void OnVisible()
        {
            OnVisibleInternal();
            SetHasUpdate();
            foreach (BaseComponent comp in GetMapping().Select(x => x.component))
            {
                comp.OnVisible();
            }
        }

        protected virtual void OnVisibleInternal()
        {

        }

        public bool Dismount()
        {
            if (_root is null) throw new InvalidOperationException("The item is already dismounted");
            if (!_root.Contains(this))
            {
                OnDismount();
                OnHide();
                _root = null;
                return true;
            }
            return false;
        }

        protected virtual void OnDismount()
        {

        }

        internal void OnHide()
        {
            OnHideInternal();
            foreach (BaseComponent comp in GetMapping().Select(x => x.component))
            {
                comp.OnHide();
            }
        }

        protected virtual void OnHideInternal() { }

        public ConsoleContent[,] Render()
        {
            CheckLock();
            _rerender = false;
            UpdateAllocSize();
            if (!GetHasUpdate())
            {
                return (ConsoleContent[,])ContentPlace.Clone();
            }
            _localHasUpdate = false;
            ConsoleContent[,] content = RenderInternal();
            ContentPlace = content;
            if (_rerender)
            {
                return Render();
            }
            return content;
        }

        protected virtual ConsoleContent[,] RenderInternal()
        {
            ConsoleContent[,] newArr = new ConsoleContent[_allocSize.x, _allocSize.y];
            newArr = RenderPre(newArr);
            if (newArr.GetLength(0) != _allocSize.x || newArr.GetLength(1) != _allocSize.y) throw new InvalidOperationException("Cannot change size of ConsoleContent array on RenderPre");
            _lock = true;
            try
            {
                foreach (
                    (IComponent component, (uint x, uint y, uint allocX, uint allocY) meta, int prioity) compLoc
                    in ChildsMapping.OrderBy(x => x.prioity)
                )
                {
                    newArr = CopyTo(
                        ChangeSize(
                            compLoc.component.Render(),
                            (compLoc.meta.allocX, compLoc.meta.allocY),
                            ConsoleContent.GetDefault()
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
            if (newArr.GetLength(0) != _allocSize.x || newArr.GetLength(1) != _allocSize.y) throw new InvalidOperationException("Cannot change size of ConsoleContent array on RenderPost");
            return newArr;
        }

        protected virtual ConsoleContent[,] RenderPre(ConsoleContent[,] content) => content;

        protected virtual ConsoleContent[,] RenderPost(ConsoleContent[,] content) => content;

        protected void ReRender()
        {
            _rerender = true;
            SetHasUpdate();
        }

        public bool Deactive(Event deactiveEvent)
        {
            if (ActiveHandler.GetCurrActive() != this) return false;
            try
            {
                _activeLock = true;
                bool state = OnDeactive(deactiveEvent);
                if (state)
                {
                    _isActive = false;
                    ActiveHandler.SetInactive(this);
                }
                return state;
            }
            finally
            {
                _activeLock = false;
                SetHasUpdate();
            }
        }

        public bool IsActive()
        {
            if (ActiveHandler is null) return false;
            return ActiveHandler.GetCurrActive() == this;
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
                _isActive = ActiveHandler.SetActive(this);
            }
            catch
            {
                _isActive = false;
                if (ActiveHandler.GetCurrActive() == this) ActiveHandler.SetInactive(this);
                throw;
            }
            if (_isActive)
            {
                OnActive();
                SetHasUpdate();
            }
            return _isActive;
        }

        protected virtual void OnActive() { }

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
            if (_root == null)
                content = $"{clsName} (allocX={GetAllocSize().x}, allocY={GetAllocSize().y}) - {Debug_Info() ?? ""}\r\n";
            string inner = String.Join(
                "\n",
                ChildsMapping
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

        public virtual string AsLatex()
        {
            return "";
        }
    }

    public abstract class Component<TS> : BaseComponent where TS : ComponentStore
    {
        public readonly TS Store;

        public Component(TS store) : base()
        {
            Store = store;
        }

        public Component(ComponentConfig config, TS store) : base(config)
        {
            Store = store;
        }

        public virtual TS ComponentStoreConstructor()
        {
            TS store = (new EmptyStore() as TS);
            if (store is null)
            {
                throw new InvalidOperationException("To use ComponentStore without providing on constructor, ComponentStoreConstructor must be defined on the component");
            }
            return store;
        }

        public Component() : base()
        {
            Store = ComponentStoreConstructor();
        }

        public Component(ComponentConfig config) : base(config)
        {
            Store = ComponentStoreConstructor();
        }
    }

    public abstract class Component : Component<EmptyStore>
    {
        public override EmptyStore ComponentStoreConstructor()
        {
            return new EmptyStore();
        }

        public Component() : base() { }
        public Component(ComponentConfig config) : base(config) { }
    }
}
