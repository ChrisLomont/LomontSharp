using System;
using System.Collections.Generic;
using System.Linq;

namespace Lomont.Stats
{
    /// <summary>
    /// Represent sequence of integers
    /// </summary>
    public class Sequence
    {
        bool sorted = false;
        List<int> vals = new ();
        public void AddSample(int value)
        {
            sorted = false;
            vals.Add(value); // todo - perhaps track internals without entire list, but numeric drift?
        }

        /// <summary>
        /// Get min. Sorts internals is needed
        /// </summary>
        /// <returns></returns>
        public int Min()
        {
            if (!sorted) Sort();
            return vals[0];
        }
        /// <summary>
        /// Get max. Sorts internals is needed
        /// </summary>
        /// <returns></returns>
        public int Max()
        {
            if (!sorted) Sort();
            return vals.Last();
        }

        /// <summary>
        /// Get mean
        /// </summary>
        public double Mean => (double)vals.Sum() / vals.Count;

        public double StdDev
        {
            get
            {
                var mean = Mean;
                var d2 = vals.Sum(x => (x - mean) * (x - mean));
                return Math.Sqrt((double) d2 / vals.Count);
            }
        }

        void Sort()
        {
            vals.Sort();
        }

        public override string ToString()
        {
            return $"{Min()},{Max()}:{Mean:F2},{StdDev:F2}";
        }
    }

}
