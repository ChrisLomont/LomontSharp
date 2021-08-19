using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lomont.Algorithms
{
    static class Combinatorics
    {

        /// <summary>
        /// Generate all k subsets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="elements"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)
        {
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }

        private static long binomial(int n, int k)
        {
            if (k < 0 || k > n) return 0;
            if (k > n - k)
            {    // take advantage of symmetry
                k = n - k;
            }
            long c = 1;
            for (int i = 1; i < k + 1; ++i)
            {
                c = c * (n - (k - i));
                c = c / i;
            }
            return c;
        }

        public static int[,] combinations(int k, int[] set)
        {
            // binomial(N, K)
            int c = (int)binomial(set.Length, k);
            // where all sets are stored
            int[,] res = new int[c, System.Math.Max(0, k)];
            // the k indexes (from set) where the red squares are
            // see image above
            int[] ind = k < 0 ? null : new int[k];
            // initialize red squares
            for (int i = 0; i < k; ++i) { ind[i] = i; }
            // for every combination
            for (int i = 0; i < c; ++i)
            {
                // get its elements (red square indexes)
                for (int j = 0; j < k; ++j)
                {
                    res[i, j] = set[ind[j]];
                }
                // update red squares, starting by the last
                int x = ind.Length - 1;
                bool loop;
                do
                {
                    loop = false;
                    // move to next
                    ind[x] = ind[x] + 1;
                    // if crossing boundaries, move previous
                    if (ind[x] > set.Length - (k - x))
                    {
                        --x;
                        loop = x >= 0;
                    }
                    else
                    {
                        // update every following square
                        for (int x1 = x + 1; x1 < ind.Length; ++x1)
                        {
                            ind[x1] = ind[x1 - 1] + 1;
                        }
                    }
                } while (loop);
            }
            return res;
        }


        public static List<long> KSubsets(int k, int n)
        {
            Trace.Assert(k <= n && n < 63);
            // init k low bits
            long bits = (1L << k) - 1;
            long top = (1L << n);
            var all = new List<long>();
            while (bits < top)
            {
                all.Add(bits);
                // next lexicographic bit permutation

                long v = bits; // current permutation of bits 
                long w;      // next permutation of bits

                // t gets v's least significant 0 bits set to 1
                // Next set to 1 the most significant bit to change, 
                // set to 0 the least significant ones, and add the necessary 1 bits.
                long t = (v | (v - 1)) + 1;
                w = t | ((((t & -t) / (v & -v)) >> 1) - 1);

                bits = w;

            }
            return all;
        }

        static bool NextLexigraphicPermutation(ulong[] array)
        {   //Knuth Algorithm L:

            // Find longest non-increasing suffix
            var i = array.Length - 1;
            while (i > 0 && array[i - 1] >= array[i])
                i--;
            // Now i is the head index of the suffix

            // Are we at the last permutation already?
            if (i <= 0)
                return false;

            // Let array[i - 1] be the pivot
            // Find rightmost element that exceeds the pivot
            var j = array.Length - 1;
            while (array[j] <= array[i - 1])
                j--;
            // Now the value array[j] will become the new pivot
            // Assertion: j >= i

            // Swap the pivot with j
            var temp = array[i - 1];
            array[i - 1] = array[j];
            array[j] = temp;

            // Reverse the suffix
            j = array.Length - 1;
            while (i < j)
            {
                temp = array[i];
                array[i] = array[j];
                array[j] = temp;
                i++;
                j--;
            }

            // Successfully computed the next permutation
            return true;
        }

        private static bool NextPermutation(int[] numList)
        {
            /*
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */
            var largestIndex = -1;
            for (var i = numList.Length - 2; i >= 0; i--)
            {
                if (numList[i] < numList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0) return false;

            var largestIndex2 = -1;
            for (var i = numList.Length - 1; i >= 0; i--)
            {
                if (numList[largestIndex] < numList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            var tmp = numList[largestIndex];
            numList[largestIndex] = numList[largestIndex2];
            numList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
            {
                tmp = numList[i];
                numList[i] = numList[j];
                numList[j] = tmp;
            }

            return true;
        }

        private static bool NextPermutation(char[] numList)
        {
            /*
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */
            var largestIndex = -1;
            for (var i = numList.Length - 2; i >= 0; i--)
            {
                if (numList[i] < numList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0) return false;

            var largestIndex2 = -1;
            for (var i = numList.Length - 1; i >= 0; i--)
            {
                if (numList[largestIndex] < numList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            var tmp = numList[largestIndex];
            numList[largestIndex] = numList[largestIndex2];
            numList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
            {
                tmp = numList[i];
                numList[i] = numList[j];
                numList[j] = tmp;
            }

            return true;
        }

    }
}
