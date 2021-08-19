using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lomont.Algorithms
{

    public static class Permutations
    {
        // Some algos
        // https://www.baeldung.com/cs/array-generate-all-permutations

        /// <summary>
        /// Generate sequence of all permutations of array
        /// Modifies array in place
        ///
        /// Uses Heap's Algorithm, minimal swaps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> GenerateMinimalSwap<T>(T[] array)
        {
            Action<int, int> SwapA = (i1, j1) =>
            {
                var t = array[i1];
                array[i1] = array[j1];
                array[j1] = t;
            };

            var n = array.Length;
            var c = new int[n]; // store counter state, init to all zeros

            yield return array;

            var i = 1; // works like stack pointer
            while (i < n)
            {
                if (c[i] < i)
                {
                    SwapA((i & 1) == 0 ? 0 : c[i], i);

                    yield return array;

                    c[i]++;
                    i = 1;
                }
                else
                {
                    c[i] = 0;
                    i++;
                }
            }
        }

        /// <summary>
        /// Generate permutations of integers 0,1,...,n-1
        /// Generation order is to hold a prefix part fixed, and generate all suffixes
        /// Early cutoffs can be done by failing the prefix check, which is called on
        /// the array of integers, and the index (inclusive) of the current prefix
        /// </summary>
        /// <param name="n"></param>
        /// <param name="permAction"></param>
        /// <param name="prefixCheck"></param>
        public static void GeneratePrefix(int n, Action<int[]> permAction, Func<int[],int,bool> prefixCheck)
        {
            var arr = new int[n];
            for (var i = 0; i < n; ++i)
                arr[i] = i;

            Recurse(0);

            // permute array from arr[start] through last
            void Recurse(int start)
            {
                if (start == n - 1)
                {
                    permAction(arr);
                    return;
                }

                if (prefixCheck(arr, start))
                    Recurse(start + 1); // no swap

                for (var i = start + 1; i < n; ++i)
                {
                    swap(arr, start, i);
                    if (prefixCheck(arr, start))
                        Recurse(start + 1);
                    swap(arr, start, i);
                }
            }
        }

        // Apply a permutation to source, start at startIndex, and write into dest
        public static void ApplyPermutation<T>(int startIndex, T[] source, T[] dest, int[] perm)
        {
            for (var i = 0; i < perm.Length; ++i)
                dest[i + startIndex] = source[perm[i]+startIndex];
        }



        #region experiments

        public static void Perm4(int n)
        {
            var arr = new int[n];
            for (var i = 0; i < n; ++i)
                arr[i] = i + 1;

            int calls = 0;

            Work(arr,0, CheckPrefix);

            Trace.WriteLine($"{calls} function calls");

            // check prefix up to (and including) end
            static bool CheckPrefix(int [] arr1, int end)
            {
                var desired = new[] {3,1,2,4,5};
                for (var i =0; i <= end; ++i)
                    if (arr1[i] != desired[i])
                        return false;
                return true;
            }


            /// <summary>
            /// Permute array from arr[start] through last
            /// </summary>
            void Work(int[] arr, int start, Func<int[],int,bool> checkPrefix)
            {
                ++calls;
                var n = arr.Length;
                if (start == n - 1)
                {
                    Dump(arr);
                    return;
                }

                if (checkPrefix(arr,start))
                    Work(arr,start+1, checkPrefix); // no swap

                for (var i = start + 1; i < n; ++i)
                {
                    swap(arr, start, i);
                    if (checkPrefix(arr, start))
                        Work(arr, start + 1, checkPrefix);
                    swap(arr, start, i);
                }
            }
        }


        public static void Perm2(int n)
        {
            var arr = new int[n];
            for (var i = 0; i < n; ++i)
                arr[i] = i + 1;

            Permute("",arr,n);
        }

        static void Dump(int [] arr)
        {
            for (var i = 0; i < arr.Length; ++i)
                Trace.Write($"{arr[i]},");
            Trace.WriteLine("");

        }

        static void Permute(string indent, int[] arr, int n)
        {
            if (n == 1)
            {
                Trace.Write(indent);
                Dump(arr);
                return;
            }

            for (var i = 0; i < n; ++i)
            {
                swap(arr, i,n-1);// remove ith element
                Permute(indent + "  ", arr,n-1); // recurse
                swap(arr, i, n - 1); // restore
            }


        }
        static void swap(int[] arr, int a, int b)
        {
            var t = arr[a];
            arr[a] = arr[b];
            arr[b] = t;

        }
        #endregion

    }


}
