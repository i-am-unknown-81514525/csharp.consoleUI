using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;
using ui.utils;

namespace ui.components
{

    public abstract class VirtualTable<T> : Container, ITable where T : ITable
    {
        protected readonly T inner;
        protected virtual (int x, int y) size
        {
            get => GetSize();
            set
            {
                throw new InvalidOperationException("size cannot be directly overwritten");
            }
        }
        protected abstract T InnerConstructor();

        public VirtualTable() : base()
        {
            inner = InnerConstructor();
            Add(inner);
        }

        public virtual (int x, int y) GetSize() => inner.GetSize();

        public virtual void Resize((int x, int y) newSize)
        {
            if (newSize.x < GetSize().x || newSize.y < GetSize().y)
            {
                throw new InvalidOperationException("By default, resize to smaller size is not possible. Use ForceResize instead");
            }
            for (int x = GetSize().x; x < newSize.x; x++)
            {
                AddColumn();
            }
            for (int y = GetSize().y; y < newSize.y; y++)
            {
                AddRow();
            }
        }

        public virtual void ForceResize((int x, int y) newSize)
        {
            if (newSize.x < 1 || newSize.y < 1)
            {
                throw new InvalidOperationException("A table must at least have 1 by 1 cell");
            }
            if (newSize.x < GetSize().x)
            {
                for (int x = GetSize().x - 1; x >= newSize.x; x--)
                {
                    RemoveColumn(x);
                }
            }
            if (newSize.y < GetSize().y)
            {
                for (int y = GetSize().y - 1; y >= newSize.y; y--)
                {
                    RemoveRow(y);
                }
            }
            DEBUG.DebugStore.AppendLine($"{newSize}, {GetSize()}");
            Resize(newSize);
        }

        public virtual void AddColumn(SplitAmount amount = null) => InsertColumn(size.x, amount);
        public virtual void AddRow(SplitAmount amount = null) => InsertRow(size.y, amount);

        public abstract void InsertColumn(int idx, SplitAmount amount = null);

        public abstract void InsertRow(int idx, SplitAmount amount = null);

        public abstract void RemoveColumn(int idx);

        public abstract void RemoveRow(int idx);



        public virtual IComponent this[int x, int y]
        {
            get => inner[x, y];
            set
            {
                inner[x, y] = value;
            }
        }

        public IComponent this[(int x, int y) loc]
        {
            get => this[loc.x, loc.y];
            set => this[loc.x, loc.y] = value;
        }
    }
}
