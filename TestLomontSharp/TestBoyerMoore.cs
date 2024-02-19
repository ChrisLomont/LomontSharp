using System;
using System.Collections.Generic;
using Lomont.Algorithms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestBoyerMoore
    {
        [Test]
        public void Test1()
        {
            var rand = new Random(1234);

            for (var pass = 0; pass < 1000; ++pass)
            {
                var text = Gen(rand, 10000);
                var pattern = Gen(rand, rand.Next(2,4));

                //text    = new byte[]{1,2,3,4};
                //pattern = new byte[] { 2, 3};

                var matchesBoyer = BoyerMoore.Find(text, pattern);
                var matchesBrute = BruteFind(text, pattern);
                var same = true;
                ClassicAssert.True(matchesBoyer.Count == matchesBrute.Count);

                for (var i = 0; i < matchesBoyer.Count; ++i)
                    same &= matchesBoyer[i] == matchesBrute[i];
                ClassicAssert.True(same);
            }


        }

        public static List<int> BruteFind(byte[]text, byte[] pattern)
        {
            var ans = new List<int>();
            for (var start = 0; start < text.Length; ++start)
            {
                var match = true;
                for (var length = 0; length < pattern.Length; ++length)
                {
                    if (text.Length <= start + length || text[start + length] != pattern[length])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                    ans.Add(start);
            }
            return ans;
        }

        public static byte[] Gen(Random rand, int length)
        {
            var b = new byte[length];
            // limit to make more hits
            for (var i = 0; i < length; ++i)
                b[i] = (byte)rand.Next(45,50);
            return b;
        }

    }
}
