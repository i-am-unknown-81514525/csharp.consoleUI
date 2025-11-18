using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;
using ui.utils;

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
        public readonly int Col;
        private readonly Field _top;

        protected override FormattedTable InnerConstructor()
        {
            return new FormattedTable();
        }

        public PagingTableInner(Field top)
        {
            Col = top.comp.Length;
            _top = new Field(top.comp);
            ForceResize((Col, 2));
            for (int x = 0; x < Col; x++)
            {
                this[x, 0] = top.comp[x];
            }
        }

        public void PushFields(IEnumerable<Field> fields)
        {
            Field[] fieldsArr = fields.ToArray();
            ForceResize((Col, 1 + fieldsArr.Length));
            for (int y = 1; y - 1 < fieldsArr.Length; y++)
            {
                for (int x = 0; x < Inner.GetSize().x; x++)
                {
                    this[x, y] = new Padding();
                }
            }
            for (int y = 1; y - 1 < fieldsArr.Length; y++)
            {
                int arrY = y - 1;
                Field field = fieldsArr[arrY];
                if (field.comp.Length != Inner.GetSize().x)
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
            Inner.InsertColumn(idx, amount);
        }
        public override void InsertRow(int idx, SplitAmount amount = null)
        {
            Inner.InsertRow(idx, amount);
        }
        public override void RemoveColumn(int idx)
        {
            Inner.RemoveColumn(idx);
        }
        public override void RemoveRow(int idx)
        {
            Inner.RemoveRow(idx);
        }
    }

    public class PagingTable : Container
    {
        public BoundedSpinner Spinner = new BoundedSpinner("Page", 1, 1, 1);

        private protected PagingTableInner Inner;

        //Reactive of overlap with type int and default value: `0`, Trigger: SetHasUpdate();
        private int _overlap;
        public int overlap
        {
            get => _overlap; set
            {
                if (value < 0) throw new InvalidOperationException("Cannot have negative overlap, overlap must be >= 0");
                _overlap = value;
                SetHasUpdate();
            }
        }

        private int _pgIdx; // the index of the first field of the page
        private int _pgEndIdx; // the index of the last field of the page
        private int _virtPgIdx; // the virtual field space when resizing so it return to exact same page after a series of resize. Change on page change

        protected List<Field> Fields = new List<Field>();

        public PagingTable(Field top)
        {
            Inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent
            {
                Inner,
                (new HorizontalGroupComponent {
                    (Spinner, new Fraction(1, 1))
                }, 1)
            });
            Spinner.OnChange = ChangePage;
        }

        public PagingTable(Field top, GroupComponentConfig config, bool isLeft = true)
        {
            Inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent
            {
                Inner,
                (isLeft ? new HorizontalGroupComponent {
                    config,
                    (Spinner, new Fraction(1, 1))
                } :
                new HorizontalGroupComponent {
                    (Spinner, new Fraction(1, 1)),
                    config
                }, 1)
            });
            Spinner.OnChange = ChangePage;
        }

        public PagingTable(Field top, GroupComponentConfig left, GroupComponentConfig right)
        {
            Inner = new PagingTableInner(top);
            Add(new VerticalGroupComponent
            {
                Inner,
                (
                    new HorizontalGroupComponent {
                        left,
                        (Spinner, new Fraction(1, 1)),
                        right
                    }, 1
                )
            });
            Spinner.OnChange = ChangePage;
        }

        public int GetPageRenderAmount()
        {
            int size = (int)GetAllocSize().y - 3;
            int actSize = size - overlap;
            if (actSize < 1) actSize = 1;
            return actSize;
        }

        public void ChangePage(int page)
        {
            _pgIdx = _virtPgIdx = GetPageRenderAmount() * (page - 1);
            UpdateRender();
        }

        public void Push(Field item)
        {
            int currCount = Fields.Count;
            Fields.Add(item);
            if (currCount - 2 <= _pgEndIdx) // Only update the render if it is in page (or likely)
            {
                _virtPgIdx = currCount;
                UpdateRender();
            }
            UpdateSpinner();
        }

        public void RemoveLast()
        {
            int rmIdx = Fields.Count - 1;
            Fields.RemoveAt(rmIdx);
            if (_pgIdx <= rmIdx && rmIdx <= _pgEndIdx)
            {
                UpdateRender();
            }
            UpdateSpinner();
        }

        public void UpdateRender()
        {
            Field[] result;
            (result, _pgIdx, _pgEndIdx) = RenderWith(_virtPgIdx);
            Inner.PushFields(result);
            Spinner.HiddenChange(_pgIdx / GetPageRenderAmount() + 1);
            SetHasUpdate();
        }

        protected (Field[], int start, int end) RenderWith(int refIdx)
        {
            int actSize = GetPageRenderAmount();
            int startIdx = (refIdx / actSize) * actSize;
            int endIdx = startIdx + actSize - 1; // To make this inclusive idx
            if (endIdx >= Fields.Count)
            {
                endIdx = Fields.Count - 1;
            }
            int count = endIdx - startIdx + 1;
            Debug.DebugStore.AppendLine($"start_idx={startIdx}, end_idx={endIdx}, count={count} fields.Count={Fields.Count}");
            if (endIdx < 0)
            {
                return (new Field[] { }, 0, 0);
            }
            if (startIdx > endIdx)
            {
                return RenderWith(endIdx);
            }
            return (Fields.GetRange(startIdx, count).ToArray(), startIdx, endIdx);
        }

        public void UpdateSpinner()
        {
            int maxPage = (Fields.Count - 1) / GetPageRenderAmount() + 1;
            if (maxPage != Spinner.upper)
            {
                Spinner.upper = maxPage;
            }
            if (maxPage < Spinner.amount)
            {
                Spinner.HiddenChange(maxPage);
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
            return Fields.ToArray();
        }

        public int Count()
        {
            return Fields.Count();
        }
    }
}
