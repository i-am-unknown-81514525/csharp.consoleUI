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
            if (childsMapping.Count(x => x.component == component) == 0)
            {
                throw new InvalidOperationException("The parent doesn't have this direct challenge");
            }
            (uint _, uint _2, uint allocX, uint allocY) = childsMapping.Where(x => x.component == component).First().location;
            return (allocX, allocY);
        }

        public void UpdateAllocSize()
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
            }
        }

        public virtual ConsoleContent[,] Render()
        {
            if (!GetHasUpdate())
            {
                return (ConsoleContent[,])contentPlace.Clone();
            }
            ConsoleContent[,] newArr = new ConsoleContent[allocSize.x, allocSize.y];
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
            return newArr;
        }

    }
}