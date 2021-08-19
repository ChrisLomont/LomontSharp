using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lomont.Containers
{
    /// <summary>
    /// Quick lookup structure for items associated to numerical ranges
    /// </summary>
    /// <typeparam name="TRange"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class RangeSet<TRange,TKey> where TRange : IComparable
    {
        /// <summary>
        /// Add item in range. Throws if range overlaps any previous ones
        /// </summary>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <param name="item"></param>
        public void Add(TRange low, TRange high, TKey item)
        {
            Pair p = new(low, high, item);
            items.Add(p);
            // Insert(p);
        }

        /// <summary>
        /// Seek an item. Return (true, Record) if found, else (false,null)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public (bool found, Pair Match) Find(TRange value)
        {
            var len = 0;
            foreach (var f in items)
            {
                len++;
                if (f.Low.CompareTo(value) <= 0 && 0 <= f.High.CompareTo(value))
                {
                    stats.Add(len);
                    return (true, f);
                }
            }

            return (false, null);
        }

        // todo - replace with faster lookups
        List<Pair> items = new();

        #region Binary Tree
        class Node
        {

            public Node left; // values lower than this 
            public Node right; // values higher than this
            public Pair content;
        }

        Node root;

        // sort on lower bounds
        void Insert(Pair pair)
        {
            if (root == null)
            {
                root = new Node {content = pair};
                return;
            }

            var c = root;
            while (true)
            {
                if (pair.Low.CompareTo(c.content.Low) < 0)
                { // pair is < current
                    if (c.left != null)
                        c = c.left;
                    else
                    {
                        c.left = new Node{content = pair};
                        return;
                    }
                }
                else if (c.content.Low.CompareTo(pair.Low) < 0)
                { // cur < pair
                    if (c.right != null)
                        c = c.right;
                    else
                    {
                        c.right = new Node { content = pair };
                        return;
                    }
                }
                else
                    throw new Exception("Double insert");
            }
        }

        /// <summary>
        /// Return item or null
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Pair FindTree(TRange value)
        {

            var c = root;
            while (c != null)
            {
                // todo

            }

            return null;
        }

        #endregion
        
        /// <summary>
        /// Store an item with a range
        /// </summary>
        public record Pair(TRange Low, TRange High, TKey Item);


#region stats
        // static stats
        static List<int> stats = new List<int>();
        /// <summary>
        /// Dump stats to Trace
        /// </summary>
        /// <param name="clearStats"></param>
        public static void DumpStats(bool clearStats)
        {
            if (!stats.Any()) return;
            var (min,max) = (stats.Min(),stats.Max());
            var total = stats.Sum();
            var avg = (double) total / stats.Count;
            Trace.WriteLine($"Range stats: {stats.Count} total queries, {min} min, {max} max, {avg:F2} avg");
            if (clearStats) stats.Clear();
        }

#endregion

    }
}
