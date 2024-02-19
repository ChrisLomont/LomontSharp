using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Algorithms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestOptimalMatching
    {
        void RunOne(int[,] mat, int [] ans)
        {
            // 2,1,0,3 (0 indexed)
            var ans2 = OptimalMatching.Find(mat);
            
            ClassicAssert.True(ans2.Length == ans.Length);
            for (var i =0; i < ans.Length; ++i)
                ClassicAssert.True(ans[i] == ans2[i]);
        }

        [Test]
        public void Test1()
        {
            // https://hungarianalgorithm.com/examplehungarianalgorithm.php
            int[,] mat = 
            {
                // job 1  2  3  4
                { 82, 83, 69, 92 }, // worker 1
                { 77, 37, 49, 92 }, // worker 2
                { 11, 69, 5, 86 }, // worker 3
                { 8, 9, 98, 23 }, // worker 4
            };
            // 2,1,0,3 (0 indexed)
            int[] ans = { 2, 1, 0, 3 };

            RunOne(mat,ans);
        }
        [Test]
        public void Test2()
        {
            // https://www.geeksforgeeks.org/hungarian-algorithm-assignment-problem-set-1-introduction/
            int[,] mat =
            {
                // job 1  2  3 
                { 2500, 4000, 3500}, // worker 1
                { 4000,6000,3500}, // worker 2
                { 2000,4000,2500}, // worker 3
            };
            // 1,0,2 (0 indexed)
            int[] ans = { 1,2,0 };

            RunOne(mat, ans);
        }
        [Test]
        public void Test3()
        {
            // http://www.universalteacherpublications.com/univ/ebooks/or/Ch6/hungar.htm
            int[,] mat =
            {
                // job 1  2  3 4 
                { 20,25,22,28 }, // worker 1
                { 15,18,23,17 }, // worker 2
                { 19,17,21,24 }, // worker 3
                { 25,23,24,24 }, // worker 4
            };
            // 0,3,1,2 (0 indexed)
            int[] ans = { 0,3,1,2 };

            RunOne(mat, ans); // todo- get another test?
        }
        [Test]
        public void Test4()
        {
            // https://www.wikihow.com/Use-the-Hungarian-Algorithm
            int[,] mat =
            {
                // job 1 2  3  4
                { 10, 19,  8, 15 }, // worker 1
                { 10, 18,  7, 17 }, // worker 2
                { 13, 16,  9, 14 }, // worker 3
                { 12, 19,  8, 18 }, // worker 4
                { 14, 17, 10, 19 }, // worker 5
            };
            // 0,2,3,4,1 (0 indexed)
            int[] ans = { 0, 2, 3, 4, 1 };

            RunOne(mat, ans);
        }

    }
}
