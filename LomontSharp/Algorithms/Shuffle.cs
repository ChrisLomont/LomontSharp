using System;
using System.Collections.Generic;
using Lomont.Numerical;

namespace Lomont.Algorithms
{
    public static class Extensions
    {

        /// <summary>
        /// Shuffle items
        /// </summary>
        /// <param name="array2D"></param>
        /// <param name="random"></param>
        /// <param name="maxValue"></param>
        public static void Shuffle<T>(T[,] array2D, LocalRandom random, int maxValue = Int32.MaxValue)
        {
            maxValue = System.Math.Min(maxValue, array2D.GetLength(0) * array2D.GetLength(1));
            var w = array2D.GetLength(0); // used to linearize array
            // Fischer-Yates shuffle of an array a of n elements (indices 0..n-1):
            for (var i = maxValue - 1; i >= 1; --i)
            {
                var j = random.Next(i);

                // swap item i and j, converting to 2D
                var x1 = i % w;
                var y1 = i / w;
                var x2 = j % w;
                var y2 = j / w;

                (array2D[x1, y1], array2D[x2, y2]) = (array2D[x2, y2], array2D[x1, y1]);
            }
        }



        /// <summary>
        /// Shuffle list. Requires source of random, rand(N) gives unif rand in [0,1,..,(N-1)]
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="rand"></param>
        public static void Shuffle<T>(this IList<T> list, Func<int, int> rand)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rand(n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();
            list.Shuffle(n=>rng.Next(n));
        }
    }

    
}
