using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Containers
{
    /// <summary>
    /// Simple priority queue
    /// Many operations can be made faster with more complex internals
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T>
    {
        public PriorityQueue(IComparer<T> comparer)
        {
            dict_ = new SortedDictionary<T, List<T>>(comparer);
            count_ = 0;
        }

        public void Add(T item)
        {
            if (!dict_.ContainsKey(item))
                dict_.Add(item, new List<T> { item });
            else
                dict_[item].Add(item);
            ++count_;
        }

        public T Remove()
        {
            if (Count == 0)
                throw new Exception("Priority queue already empty");
            count_--;
            var pair = dict_.First();
            var key = pair.Key;
            var value = pair.Value;
            var item = value.First();
            value.Remove(item);
            if (value.Count == 0)
                dict_.Remove(key);
            return item;
        }

        public int Count => count_;

        public void Clear()
        {
            dict_.Clear();
            count_ = 0;
        }

        #region Implementation

        private SortedDictionary<T, List<T>> dict_;
        private int count_;

        #endregion
    }
}
