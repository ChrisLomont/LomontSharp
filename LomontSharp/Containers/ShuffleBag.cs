using System;
using System.Collections;
using System.Collections.Generic;

namespace Lomont.Containers
{
    /// <summary>
    /// Implements a shuffle bag, which is a container that
    /// returns an infinite sequence of items from a finite set of 
    /// choices, such that all items from the finite set occur before 
    /// there are any repeats.
    /// 
    /// This implementation also allows item frequencies, so items can 
    /// be requested to occur multiple times in the sequence before
    /// the shuffling mixes them again.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ShuffleBag<T> : IEnumerable<T>
    {
        /* Needs:
         *  1. Add item, and optional frequency
         *  2. Remove given item (requires matching optional frequency?)
         *  3. Count of items in bag, and optional frequencies
         *  4. List items in bag
         *  5. Next item (which necessarily modifies bag internals, 
         *     invalidating listing item)
         *  6. Random source repeatable, controllable from outside.
         *  7. Change frequency of an item
         *  8. Supports infinite enumeration
         *  9. Find item, with or without frequency?
         * 10. Stable item order - keep from moving under lists :)
         * 11. Ability to make sure at end there is no immediate repeats, 
         *     set a parameter
         *     
         * Use Fenwick trees to store cumulative frequency counts, only 
         * incurred if frequencies other than 1 are requested.
         * 
         * Hard to add/delete from fenwick tree, can do from last entry
         * - remove ith = zero ith entry, add in last entry, zero last entry, 
         *   shrink. reorders meaning of tree, so need additional array to 
         *   track this
         * - append - extend array, add value, seems to work ok
         * 
         */

        public ShuffleBag()
        {
            random = new Random();
            getRand = max => random.Next(max);
        }

        /// <summary>
        /// Add an item, and optional frequency.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="frequency">Item frequency, must be nonnegative</param>
        public void Add(T item, int frequency = 1)
        {
            items.Add(item);
            maxFrequencyCounts.Add(frequency);
            currentFrequencyCounts.Add(frequency);
        }

        /// <summary>
        /// Add an item at the given index, and optional frequency.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        /// <param name="frequency">Item frequency, must be nonnegative</param>
        public void Insert(int index, T item, int frequency = 1)
        {
            items.Insert(index, item);
            maxFrequencyCounts.Insert(index, frequency);
            currentFrequencyCounts.Insert(index, frequency);
        }

        /// <summary>
        /// Remove a given item, using optional frequency to disambiguate duplicates.
        /// If frequency is -1, then ignored for comparison.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="frequency">Item frequency, must be nonnegative if given</param>
        /// <returns>true if item found and removed, else false</returns>
        public bool Remove(T item, int frequency = -1)
        {
            var index = IndexOf(item, frequency);
            if (index == -1)
                return false;
            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            items.RemoveAt(index);
            maxFrequencyCounts.RemoveAt(index);
            currentFrequencyCounts.RemoveAt(index);
        }


        /// <summary>
        /// See if the shuffle bag contains a given item, using optional frequency 
        /// to disambiguate duplicates.If frequency is -1, then ignored for comparison.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="frequency">Item frequency, must be nonnegative if given</param>
        /// <returns>true if item found, else false</returns>
        public bool Contains(T item, int frequency = -1)
        {
            return IndexOf(item, frequency) != -1;
        }

        /// <summary>
        /// See if the shuffle bag contains a given item, using optional frequency 
        /// to disambiguate duplicates.If frequency is -1, then ignored for comparison.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="frequency">Item frequency, must be nonnegative if given</param>
        /// <returns>index of the item if item found, else -1</returns>
        public int IndexOf(T item, int frequency = -1)
        {
            for (var i = 0; i < items.Count; ++i)
            {
                if (items[i].Equals(item) && (frequency == -1 || maxFrequencyCounts.Frequency(i) == frequency))
                    return i;
            }

            return -1;
        }


        /// <summary>
        /// The count of items in the bag, not counting frequencies
        /// </summary>
        public int CountWithoutFrequencies
        {
            get { return items.Count; }
        }

        /// <summary>
        /// The count of items in the bag, including frequencies
        /// </summary>
        public int CountWithFrequencies
        {
            get { return maxFrequencyCounts.Total; }
        }

        /// <summary>
        /// Get item and max frequency and current frequency at a given index
        /// in the bag. This ordering does not change as the bag is shuffled.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Tuple<T, int, int> GetDetails(int index)
        {
            return new Tuple<T, int, int>(items[index],
                maxFrequencyCounts.Frequency(index),
                currentFrequencyCounts.Frequency(index)
            );
        }


        /// <summary>
        /// Get or set the item at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return items[index]; }
            set { items[index] = value; }
        }

        // erase the contents of the bag
        public void Clear()
        {
            items.Clear();
            currentFrequencyCounts.Clear();
            maxFrequencyCounts.Clear();
        }

        #region Shuffle bag

        public IEnumerator<T> GetEnumerator()
        {
            yield return Next();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        /// <summary>
        /// Get next item from bag. 
        /// </summary>
        /// <returns></returns>
        public T Next()
        {

            if (currentFrequencyCounts.Total == 0)
            {
                // we removed them all, reshuffle by adding back all frequency counts
                currentFrequencyCounts.CopyFrom(maxFrequencyCounts);
            }

            // how many items are left, including frequencies
            var totalLeft = currentFrequencyCounts.Total;

            // check we have something
            if (totalLeft == 0)
                throw new Exception("No items in ShuffleBag");

            // get value in 0 to current total-1, inclusive
            var frequencyValue = getRand(totalLeft);
            // see which item is the one selected
            var itemIndex = currentFrequencyCounts.FrequencyIndex(frequencyValue);

            // remove one item
            currentFrequencyCounts.Increase(itemIndex, -1);

            // return the item
            return items[itemIndex];
        }

        #endregion


        /// <summary>
        /// Set the source of randomness, otherwise defaults to internal
        /// random function.
        /// Source returns integer in [0,max-1] inclusive
        /// </summary>
        /// <param name="randomSource"></param>
        public void SetRandomSource(Func<int, int> randomSource)
        {
            getRand = randomSource;
        }


        #region Implementation

        // default source of randomness
        private Random random;


        // function returns a random integer in [0,max-1]
        private Func<int, int> getRand;


        // store the items inserted into the bag
        // preserves order
        private List<T> items = new List<T>();

        // max frequency counts for an item stored as fenwick tree
        private FenwickTree maxFrequencyCounts = new FenwickTree();

        // the current frequency counts, used when selecting items
        // from the shuffle bag
        private FenwickTree currentFrequencyCounts = new FenwickTree();

        #endregion

    }
}

