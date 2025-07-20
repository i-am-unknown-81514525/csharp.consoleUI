using System;
using System.Linq;
using System.Collections.Generic;

namespace ui.math
{
    public sealed class SplitConfig
    {
        private bool isSet = false;
        private SplitHandler handler;
        private int amount = 0;

        public SplitConfig(SplitHandler handler)
        {
            this.handler = handler;
        }

        public void Update()
        {
            int? result = handler.GetSize(this);
            if (result != null)
            {
                isSet = true;
                amount = (int)result;
            }
        }

        public bool isReady()
        {
            if (!isSet)
            {
                Update();
            }
            return isSet;
        }

        public int? TryGetValue()
        {
            if (!isReady()) return null;
            return amount;
        }

        public int GetValue()
        {
            if (!isReady()) throw new InvalidOperationException();
            return amount;
        }

    }

    public class InvalidCastException : InvalidOperationException
    {
        
    }

    public class SplitAmount
    {
        private bool _isFraction;
        private Fraction frac;
        private int size;

        public SplitAmount(Fraction frac)
        {
            _isFraction = true;
            this.frac = frac;
        }

        public SplitAmount(int size)
        {
            _isFraction = false;
            this.size = size;
        }

        public bool isFraction() => _isFraction;

        public Fraction GetFraction()
        {
            if (!_isFraction) throw new InvalidCastException();
            return frac;
        }

        public int GetSize()
        {
            if (_isFraction) throw new InvalidCastException();
            return size;
        }
    }

    public class SplitHandler
    {
        internal Dictionary<SplitConfig, (SplitAmount amount, uint prioity)> amount = new Dictionary<SplitConfig, (SplitAmount amount, uint prioity)>();
        internal Dictionary<SplitConfig, int?> size = new Dictionary<SplitConfig, int?>();

        internal int totalSize;

        public SplitHandler(int totalSize)
        {
            if (totalSize <= 0) throw new ArgumentOutOfRangeException();
            this.totalSize = totalSize;
        }

        public int? GetSize(SplitConfig config)
        {
            if (!size.ContainsKey(config)) throw new InvalidOperationException("The referred key doesn't exist");
            return size[config];
        }

        public void SetTotalSize(int totalSize)
        {
            if (totalSize <= 0) throw new ArgumentOutOfRangeException();
            this.totalSize = totalSize;
        }

        public void Update()
        {
            int curr = totalSize;
            // List<KeyValuePair<SplitConfig, (SplitAmount amount, uint prioity)>> sorted = amount.OrderBy(x => x.Value.prioity).ToList();
            List<(uint prioity, List<KeyValuePair<SplitConfig, SplitAmount>> elements)> sortedGrouped = amount.
                GroupBy(
                    x => x.Value.prioity, // Key: the prioity
                    x => new KeyValuePair<SplitConfig, SplitAmount>(x.Key, x.Value.amount), // The element
                    (x, y) => (x, y.ToList()) // Form back to a tuple of prioity and the list of remapped element
                )
                .OrderBy(x => x.Item1)
                .ToList();
            int totalPrioityTier = sortedGrouped.Count();
            for (int i = 0; i < totalPrioityTier; i++)
            {
                (uint prioity, List<KeyValuePair<SplitConfig, SplitAmount>> elements) = sortedGrouped[i];
                List<KeyValuePair<SplitConfig, SplitAmount>> valueElements = elements.Where(x => !x.Value.isFraction()).ToList();
                List<KeyValuePair<SplitConfig, SplitAmount>> fracElements = elements.Where(x => x.Value.isFraction()).ToList();
                foreach (KeyValuePair<SplitConfig, SplitAmount> element in valueElements)
                {
                    if (curr <= 0)
                    {
                        size[element.Key] = 0;
                        continue;
                    }
                    int alloc = element.Value.GetSize();
                    if (alloc > curr)
                    {
                        alloc = curr;
                    }
                    size[element.Key] = alloc;
                    curr -= alloc;
                }
                Fraction totalFrac = fracElements.Sum(x=>x.Value.GetFraction());
                Fraction mul = new Fraction(1);
                if (totalFrac > 1 || i == totalPrioityTier - 1)
                {
                    mul = totalFrac.Invert();
                }
                int usedSize = 0;
                Fraction usedFraction = new Fraction(0);
                foreach (KeyValuePair<SplitConfig, SplitAmount> element in fracElements)
                {
                    if (curr <= 0)
                    {
                        size[element.Key] = 0;
                        continue;
                    }
                    Fraction frac = element.Value.GetFraction();
                    usedFraction += frac;
                    int currSize = (int)(usedFraction * mul * curr).GetFloor();
                    int alloc = currSize - usedSize;
                    usedSize = currSize;
                    if (alloc > curr)
                    {
                        alloc = curr;
                    }
                    size[element.Key] = alloc;
                    curr -= alloc;
                }
            }
            foreach (KeyValuePair<SplitConfig, int?> element in size)
            {
                element.Key.Update();
            }
        }

        public SplitConfig AddSplit(Fraction ratio, uint prioity = 1)
        {
            if (prioity < 1) throw new ArgumentOutOfRangeException();
            SplitAmount amount = new SplitAmount(ratio);
            SplitConfig config = new SplitConfig(this);
            this.amount[config] = (amount, prioity);
            this.size[config] = null;
            return config;
        }

        public SplitConfig AddSplit(int absoluteSize)
        {
            SplitAmount amount = new SplitAmount(absoluteSize);
            SplitConfig config = new SplitConfig(this);
            this.amount[config] = (amount, 0);
            this.size[config] = null;
            return config;
        }
    }
}