using System;
using System.Collections.Generic;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Stable sort for IList.
    /// Stable sorts leave items in same order if they compare as same.
    /// Dotnet built in sorts are not guaranteed to be stable.
    /// This is a simple merge sort, space overhead O(n).
    /// Could replace with more complex block sort, O(1) space overhead
    /// O(n log n) time complexity
    /// </summary>
    public static class SortExtensions
    {
        // see https://www.velir.com/ideas/2011/02/17/ilist-sorting-a-better-way for some ideas on making nicer syntax
        //  Sorts an IList in place.
        // see https://www.codeproject.com/Articles/27927/Extension-Methods-Exemplified-Sorting-Index-based

        /// <summary>
        /// Sort IList in place, using a stable sort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="comparer"></param>
        public static void StableSort<T>(this IList<T> list, Comparison<T> comparer)
        {
            var temp = new List<T>(list); // work array
            SplitAndMerge(temp, list, 0, list.Count, comparer);   // sort data from temp[] into list[]
        }

        #region Implementation
        

        // Split a[] into 2 runs, sort both runs into b[], merge both runs from b[] to a[]
        // interval [start, end)
        static void SplitAndMerge<T>(IList<T> b, IList<T> a, int start, int end, Comparison<T> comparer)
        {
            if (end - start <= 1)
                return; // consider runs of length <= 1 as sorted
            // split the run longer than 1 item into halves
            var middle = (end + start) / 2;              
            // recursively sort both runs from a[] into b[]
            SplitAndMerge(a, b, start, middle, comparer);  // sort the left  run
            SplitAndMerge(a, b, middle, end, comparer);    // sort the right run
            // merge the resulting runs from b[] into a[]
            Merge(b, a, start, middle, end, comparer);
        }

        // Left  source half is a[ start:mid-1].
        // Right source half is a[mid:end-1   ].
        // Result is            b[ start:end-1].
        static void Merge<T>(IList<T> a, IList<T> b, int start, int mid, int end, Comparison<T> comparer)
        {
            var i = start;
            var j = mid;

            // While there are elements in the left or right runs...
            for (var k = start; k < end; k++)
            {
                // If left run head exists and is <= existing right run head.
                if (i < mid && (j >= end || comparer(a[i], a[j]) <= 0))
                {
                    b[k] = a[i];
                    i++;
                }
                else
                {
                    b[k] = a[j];
                    j++;
                }
            }
        }
        #endregion

    }
}
