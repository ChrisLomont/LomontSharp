using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Containers
{
    /// <summary>
    /// Represent a stable priority queue
    /// The items are queued by a key
    /// Stable means in case of ties, the first one in is the first one out
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    class StablePriorityQueue<TKey, TValue> where TKey : IComparable<TKey>
    {
        public StablePriorityQueue()
        {
            comparer = new KeyComparer();
            queue = new SortedDictionary<CounterKey, TValue>(comparer);
        }

        public void Add(TKey key, TValue value)
        {
            var ckey = new CounterKey { key = key, counter = counter };
            queue.Add(ckey, value);
            ++counter;
        }

        public (TKey, TValue) Pop()
        {
            if (!Any())
                throw new InvalidOperationException("Pop on empty stable priority queue");
            var pair = queue.First();
            queue.Remove(pair.Key);
            return (pair.Key.key, pair.Value);
        }

        public bool Any()
        {
            return queue.Any();
        }

        public void Clear()
        {
            queue.Clear();
        }

        /// <summary>
        /// Get ordered list from queue
        /// </summary>
        /// <returns></returns>
        public List<(TKey, TValue)> GetList()
        {
            var list = new List<(TKey, TValue)>();
            foreach (var pair in queue)
                list.Add((pair.Key.key, pair.Value));
            return list;
        }


        #region Implementation

        // trick - use other key, with counter to break ties
        class CounterKey
        {
            public TKey key;
            public ulong counter;
        }

        // how to compare items
        class KeyComparer : IComparer<CounterKey>
        {
            public int Compare(CounterKey x, CounterKey y)
            {
                var xy = x.key.CompareTo(y.key);
                if (xy != 0)
                    return xy;
                if (x.counter < y.counter)
                    return -1;
                if (x.counter > y.counter)
                    return 1;
                return 0;
            }
        }

        KeyComparer comparer;

        SortedDictionary<CounterKey, TValue> queue;

        ulong counter = 0;

        #endregion
    }
}
