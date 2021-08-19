using System.Collections.Generic;

namespace Lomont.Containers
{
    /// <summary>
    /// A Fenwick Tree stores cumulative frequencies c_0,c_1,...,c_(n-1)
    /// in such a manner that changing a value is O(log n), finding a 
    /// given frequency f_i is O(log n), finding a sum of a range of
    /// frequencies is O(log n), and finding the index of the cumulative
    /// frequency containing a given cumulative frequency is O(log^2 n).
    /// 
    /// Frequencies must be non-negative.
    /// </summary>
    public class FenwickTree
    {

        /// <summary>
        /// Create an empty Fenwick tree
        /// </summary>
        public FenwickTree()
        {
            tree = new List<int>();
        }

        /// <summary>
        /// Increases value of i-th element by delta, which can be 
        /// positive, negative, or zero.
        /// </summary>
        /// <param name="i"></param>
        /// <param name="delta"></param>
        public void Increase(int i, int delta)
        {
            for (; i < tree.Count; i |= i + 1)
                tree[i] += delta;
        }

        /// <summary>
        /// Returns sum of elements with indexes left..right, inclusive
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public int SumRange(int left, int right)
        {
            return Sum(right) - Sum(left - 1);
        }

        /// <summary>
        /// Return sum of items 0 through index, inclusive
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int Sum(int index)
        {
            var sum = 0;
            while (index >= 0)
            {
                sum += tree[index];
                index &= index + 1;
                index--;
            }
            return sum;
        }

        /// <summary>
        /// Get the frequency at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int Frequency(int index)
        {
            return SumRange(index, index);
        }

        /// <summary>
        /// Append value to end of cumulative value list
        /// </summary>
        /// <param name="value"></param>
        public void Add(int value)
        {
            tree.Add(0); // increase length of items
            Increase(tree.Count - 1, value);
        }

        /// <summary>
        /// insert a value into the cumulative value tree at the
        /// given index.
        /// </summary>
        /// <param name="value"></param>
        public void Insert(int index, int value)
        {
            Add(0); // appends a value of 0

            // shift values up
            for (var i = tree.Count - 1; i > index; --i)
            {
                var v = Frequency(i - 1);
                Increase(i - 1, -v); // subtract value from i-1 th
                Increase(i, v);    // add value to i th
            }
            Increase(index, value); // place value
        }

        public void RemoveAt(int index)
        {
            var value = Frequency(index); // get value at this spot
            Increase(index, -value); // subtract it

            // shift values down
            for (var i = index; i < tree.Count + 1; ++i)
            {
                var v = Frequency(i + 1);
                Increase(i + 1, -v); // subtract value from i+1 th
                Increase(i, v);      // add value to i th
            }

            tree.RemoveAt(tree.Count - 1); // remove last entry
        }

        /// <summary>
        /// Empty the tree
        /// </summary>
        public void Clear()
        {
            tree.Clear();
        }

        /// <summary>
        /// Number of entries stored in the tree
        /// </summary>
        public int Count { get { return tree.Count; } }

        /// <summary>
        /// Total sum of frequencies in tree
        /// </summary>
        public int Total { get { return Sum(tree.Count - 1); } }

        /// <summary>
        /// Copy a given tree into this one
        /// </summary>
        /// <param name="treeToCopy"></param>
        public void CopyFrom(FenwickTree treeToCopy)
        {
            tree.Clear();
            tree.AddRange(treeToCopy.tree);
        }

        /// <summary>
        /// Given a frequency sum, find the index of an entry
        /// containing the sum. Returns -1 is there is no such index.
        /// </summary>
        /// <param name="sum"></param>
        /// <returns></returns>
        public int FrequencyIndex(int sum)
        {
            // binary search. Todo - this is O(log^2 n), is there an O(log n)?

            if (sum > Total)
                return -1; // does not occur

            int imin = 0, imax = tree.Count - 1;

            // continue searching while [imin,imax] is not empty
            while (imax >= imin)
            {
                // calculate the midpoint for roughly equal partition
                int imid = (imin + imax) / 2;
                var midVal = Sum(imid);//SumRange(imid);
                var nextVal = Frequency(imid + 1);
                if (midVal <= sum && sum < nextVal)
                    return imid; // sum found at index imid
                                 // determine which subarray to search
                else if (tree[imid] < sum) // (A[imid] < key)
                    // change min index to search upper subarray
                    imin = imid + 1;
                else
                    // change max index to search lower subarray
                    imax = imid - 1;
            }
            // key was not found
            return -1;


        }

        #region Implementation

        // In this implementation, the tree is represented by a vector<int>.
        // Elements are numbered by 0, 1, ..., n-1.
        // tree[i] is sum of elements with indexes i&(i+1)..i, inclusive.
        // (Note: this is a bit different from what is proposed in Fenwick's article.
        // To see why it makes sense, think about the trailing 1's in binary
        // representation of indexes.)

        private List<int> tree;

        #endregion


    }
}
