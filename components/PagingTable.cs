using System.Collections.Generic;
using System.Linq;
using System;
using ui.utils;
using ui.math;

namespace ui.components
{
    public struct Field
    {
        private IComponent[] _comp;
        public IComponent[] comp { get => _comp.ToArray(); }

        public Field(IEnumerable<IComponent> src)
        {
            _comp = src.ToArray();
        }
    }

    internal class PagingTableInner : VirtualTable<FormattedTable>
    {
        public readonly int col;
        private readonly Field top;

        protected override FormattedTable InnerConstructor()
        {
            return new FormattedTable();
        }

        public PagingTableInner(Field top)
        {
            this.col = top.comp.Length;
            this.top = new Field(top.comp);
            ForceResize((col, 2));
            for (int x = 0; x < col; x++)
            {
                this[x, 0] = top.comp[x];
            }
        }

        public void PushFields(IEnumerable<Field> fields)
        {
            Field[] fieldsArr = fields.ToArray();
            ForceResize((col, 1 + fieldsArr.Length));
            for (int y = 1; y - 1 < fieldsArr.Length; y++)
            {
                for (int x = 0; x < inner.GetSize().x; x++)
                {
                    this[x, y] = new Padding();
                }
            }
            for (int y = 1; y - 1 < fieldsArr.Length; y++)
            {
                int arr_y = y - 1;
                Field field = fieldsArr[arr_y];
                if (field.comp.Length != inner.GetSize().x)
                {
                    throw new InvalidOperationException("Cannot use field with different size");
                }
                for (int x = 0; x < field.comp.Length; x++)
                {
                    this[x, y] = field.comp[x];
                }
            }
        }

        public override void InsertColumn(int idx, SplitAmount amount = null)
        {
            inner.InsertColumn(idx, amount);
        }
        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            inner.InsertRow(idx, amount);
        }
        public override void RemoveColumn(int idx)
        {
            inner.RemoveColumn(idx);
        }
        public override void RemoveRow(int idx)
        {
            inner.RemoveRow(idx);
        }
    }

    public class PagingTable : Container
    {
        public BoundedSpinner spinner = new BoundedSpinner("Page", 1, 1, 1);

        private protected PagingTableInner inner;

        //Reactive of overlap with type int and default value: `0`, Trigger: SetHasUpdate();
        private int _overlap = 0;
        public int overlap
        {
            get => _overlap; set
            {
                if (value < 0) throw new InvalidOperationException("Cannot have negative overlap, overlap must be >= 0");
                _overlap = value;
                SetHasUpdate();
            }
        }

        private int pg_idx = 0; // the index of the first field of the page
        private int pg_end_idx = 0; // the index of the last field of the page
        private int virt_pg_idx = 0; // the virtual field space when resizing so it return to exact same page after a series of resize. Change on page change

        protected List<Field> fields = new List<Field>();

        public PagingTable(Field top)
        {
            inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent()
            {
                inner,
                (new HorizontalGroupComponent() {
                    (spinner, new Fraction(1, 1))
                }, 1)
            });
            spinner.onChange = ChangePage;
        }

        public PagingTable(Field top, GroupComponentConfig config, bool isLeft = true)
        {
            inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent()
            {
                inner,
                (isLeft ? new HorizontalGroupComponent() {
                    config,
                    (spinner, new Fraction(1, 1))
                } :
                new HorizontalGroupComponent() {
                    (spinner, new Fraction(1, 1)),
                    config
                }, 1)
            });
            spinner.onChange = ChangePage;
        }

        public PagingTable(Field top, GroupComponentConfig left, GroupComponentConfig right)
        {
            inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent()
            {
                inner,
                (
                    new HorizontalGroupComponent() {
                        left,
                        (spinner, new Fraction(1, 1)),
                        right
                    }, 1
                )
            });
            spinner.onChange = ChangePage;
        }

        public int GetPageRenderAmount()
        {
            int size = (int)GetAllocSize().y - 3;
            int act_size = size - overlap;
            if (act_size < 1) act_size = 1;
            return act_size;
        }

        public void ChangePage(int page)
        {
            pg_idx = virt_pg_idx = GetPageRenderAmount() * (page - 1);
            UpdateRender();
        }

        public void Push(Field item)
        {
            int curr_count = fields.Count;
            fields.Add(item);
            if (curr_count - 2 <= pg_end_idx) // Only update the render if it is in page (or likely)
            {
                virt_pg_idx = curr_count;
                UpdateRender();
            }
            UpdateSpinner();
        }

        public void RemoveLast()
        {
            int rm_idx = fields.Count - 1;
            fields.RemoveAt(rm_idx);
            if (pg_idx <= rm_idx && rm_idx <= pg_end_idx)
            {
                UpdateRender();
            }
            UpdateSpinner();
        }

        public void UpdateRender()
        {
            Field[] result;
            (result, pg_idx, pg_end_idx) = RenderWith(virt_pg_idx);
            inner.PushFields(result);
            spinner.HiddenChange(pg_idx / GetPageRenderAmount() + 1);
            SetHasUpdate();
        }

        protected (Field[], int start, int end) RenderWith(int ref_idx)
        {
            int act_size = GetPageRenderAmount();
            int start_idx = (ref_idx / act_size) * act_size;
            int end_idx = start_idx + act_size - 1; // To make this inclusive idx
            if (end_idx >= fields.Count)
            {
                end_idx = fields.Count - 1;
            }
            int count = end_idx - start_idx + 1;
            ui.DEBUG.DebugStore.AppendLine($"start_idx={start_idx}, end_idx={end_idx}, count={count} fields.Count={fields.Count}");
            if (end_idx < 0)
            {
                return (new Field[] { }, 0, 0);
            }
            if (start_idx > end_idx)
            {
                return RenderWith(end_idx);
            }
            return (fields.GetRange(start_idx, count).ToArray(), start_idx, end_idx);
        }

        public void UpdateSpinner()
        {
            int maxPage = (fields.Count - 1) / GetPageRenderAmount() + 1;
            if (maxPage != spinner.upper)
            {
                spinner.upper = maxPage;
            }
            if (maxPage < spinner.amount)
            {
                spinner.HiddenChange(maxPage);
            }
        }

        protected override void OnResize()
        {
            base.OnResize();
            UpdateSpinner();
            UpdateRender();
        }

        public Field[] GetFields()
        {
            return fields.ToArray();
        }

        public int Count()
        {
            return fields.Count();
        }
    }
}
