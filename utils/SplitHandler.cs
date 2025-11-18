using System;
using System.Collections.Generic;
using System.Linq;
using ui.math;

namespace ui.utils
{
    public sealed class SplitConfig
    {
        private bool _isSet;
        private SplitHandler _handler;
        private int _amount;

        public SplitConfig(SplitHandler handler)
        {
            _handler = handler;
        }

        public void Update()
        {
            int? result = _handler.GetSize(this);
            if (result != null)
            {
                _isSet = true;
                _amount = (int)result;
            }
        }

        public bool IsReady()
        {
            if (!_isSet)
            {
                Update();
            }
            return _isSet;
        }

        public int? TryGetValue()
        {
            if (!IsReady()) return null;
            return _amount;
        }

        public int GetValue()
        {
            if (!IsReady()) throw new InvalidOperationException();
            return _amount;
        }

    }

    public class InvalidCastException : InvalidOperationException
    {

    }

    public sealed class SplitAmount : IComparable<SplitAmount>
    {
        private bool _isFraction;
        private Fraction _frac;
        private int _size;

        public uint prioity { get; }

        public SplitAmount(Fraction frac, uint prioity = 1)
        {
            if (prioity < 1) throw new ArgumentOutOfRangeException();
            _isFraction = true;
            _frac = frac;
        }

        public SplitAmount(int size)
        {
            _isFraction = false;
            prioity = 0;
            _size = size;
        }

        public bool IsFraction() => _isFraction;

        public Fraction GetFraction()
        {
            if (!_isFraction) throw new InvalidCastException();
            return _frac;
        }

        public int GetSize()
        {
            if (_isFraction) throw new InvalidCastException();
            return _size;
        }

        public static implicit operator SplitAmount(Fraction frac) => new SplitAmount(frac);
        public static implicit operator SplitAmount(int abs) => new SplitAmount(abs);
        public static implicit operator SplitAmount((Fraction frac, uint prioity) value) => new SplitAmount(value.frac, value.prioity);

        public static bool operator ==(SplitAmount left, SplitAmount right)
        {
            return (
                (
                    !(left is null) &&
                    !(right is null) &&
                    left.prioity == right.prioity &&
                    (
                        left.IsFraction() ?
                        (
                            right.IsFraction() && left.GetFraction() == right.GetFraction()
                        ) : (
                            !right.IsFraction() && left.GetSize() == right.GetSize()
                        )
                    )
                ) || (
                    left is null &&
                    right is null
                )
            );
        }

        public static bool operator !=(SplitAmount left, SplitAmount right) => !(left == right);

        public override bool Equals(object obj)
        {
            if (!(obj is SplitAmount)) return false;
            return this == (SplitAmount)obj;
        }

        public override int GetHashCode()
        {
            int prioHash = (7919 * (int)prioity); // The 1000th prime number
            return (IsFraction() ? GetFraction().GetHashCode() : GetSize().GetHashCode()) ^ prioHash;
        }

        public int CompareTo(SplitAmount right)
        {
            // if (this is null && right is null) return 0;
            // if (this is null) return -1;
            // if (right is null) return 1;
            if (this == right)
            {
                return 0;
            }
            if (prioity < right.prioity)
            {
                return 1; // larger
            }
            if (prioity > right.prioity)
            {
                return -1; // smaller
            }
            if (!IsFraction() && right.IsFraction())
            {
                return 1;
            }
            if (IsFraction() && !right.IsFraction())
            {
                return -1;
            }
            if (IsFraction())
            {
                if (GetFraction() < right.GetFraction())
                {
                    return -1;
                }

                return 1;
            }

            if (GetSize() < right.GetSize())
            {
                return -1;
            }

            return 1;
        }
    }

    public class SplitHandler
    {
        protected Dictionary<SplitConfig, SplitAmount> Amount = new Dictionary<SplitConfig, SplitAmount>();
        protected Dictionary<SplitConfig, int?> Size = new Dictionary<SplitConfig, int?>();

        protected int TotalSize;

        public SplitHandler(int totalSize)
        {
            if (totalSize <= 0) throw new ArgumentOutOfRangeException();
            TotalSize = totalSize;
        }

        public int? GetSize(SplitConfig config)
        {
            if (!Size.ContainsKey(config)) throw new InvalidOperationException("The referred key doesn't exist");
            return Size[config];
        }

        public int GetTotalSize() => TotalSize;

        public void SetTotalSize(int totalSize)
        {
            if (totalSize < 0) throw new ArgumentOutOfRangeException();
            TotalSize = totalSize;
            Update();
        }

        public void Remove(SplitConfig splitConfig)
        {
            if (!Amount.ContainsKey(splitConfig)) throw new InvalidOperationException("The config provided doesn't exist in the handler");
            Amount.Remove(splitConfig);
            Update();
        }

        public void Update()
        {
            int curr = TotalSize;
            // List<KeyValuePair<SplitConfig, (SplitAmount amount, uint prioity)>> sorted = amount.OrderBy(x => x.Value.prioity).ToList();
            List<(uint prioity, List<KeyValuePair<SplitConfig, SplitAmount>> elements)> sortedGrouped = Amount.
                GroupBy(
                    x => x.Value.prioity, // Key: the prioity
                    x => new KeyValuePair<SplitConfig, SplitAmount>(x.Key, x.Value), // The element
                    (x, y) => (x, y.ToList()) // Form back to a tuple of prioity and the list of remapped element
                )
                .OrderBy(x => x.Item1)
                .ToList();
            int totalPrioityTier = sortedGrouped.Count();
            Size = new Dictionary<SplitConfig, int?>();
            for (int i = 0; i < totalPrioityTier; i++)
            {
                (uint prioity, List<KeyValuePair<SplitConfig, SplitAmount>> elements) = sortedGrouped[i];
                List<KeyValuePair<SplitConfig, SplitAmount>> valueElements = elements.Where(x => !x.Value.IsFraction()).ToList();
                List<KeyValuePair<SplitConfig, SplitAmount>> fracElements = elements.Where(x => x.Value.IsFraction()).ToList();
                foreach (KeyValuePair<SplitConfig, SplitAmount> element in valueElements)
                {
                    if (curr <= 0)
                    {
                        Size[element.Key] = 0;
                        continue;
                    }
                    int alloc = element.Value.GetSize();
                    if (alloc > curr)
                    {
                        alloc = curr;
                    }
                    Size[element.Key] = alloc;
                    curr -= alloc;
                }
                Fraction totalFrac = fracElements.Sum(x => x.Value.GetFraction());
                if (totalFrac == 0) continue;
                Fraction mul = new Fraction(1);
                if (totalFrac > 1 || i == totalPrioityTier - 1)
                {
                    mul = totalFrac.Invert();
                }
                int oriCurr = curr;
                int usedSize = 0;
                Fraction usedFraction = new Fraction(0);
                foreach (KeyValuePair<SplitConfig, SplitAmount> element in fracElements)
                {
                    if (curr <= 0)
                    {
                        Size[element.Key] = 0;
                        continue;
                    }
                    Fraction frac = element.Value.GetFraction();
                    usedFraction += frac;
                    int currSize = (int)(usedFraction * mul * oriCurr).GetFloor();
                    int alloc = currSize - usedSize;
                    usedSize = currSize;
                    if (alloc > curr)
                    {
                        alloc = curr;
                    }
                    Size[element.Key] = alloc;
                    curr -= alloc;
                }
            }
            foreach (KeyValuePair<SplitConfig, int?> element in Size)
            {
                element.Key.Update();
            }
        }

        public SplitConfig AddSplit(Fraction ratio, uint prioity = 1)
        {
            return AddSplit(new SplitAmount(ratio, prioity));
        }

        public SplitConfig AddSplit(int absoluteSize)
        {
            return AddSplit(new SplitAmount(absoluteSize));
        }

        public SplitConfig AddSplit(SplitAmount amount)
        {
            SplitConfig config = new SplitConfig(this);
            Amount[config] = amount;
            Size[config] = null;
            Update();
            return config;
        }

        public bool Contains(SplitConfig config) => Amount.ContainsKey(config);
    }
}
