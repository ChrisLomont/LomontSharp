using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.Algorithms;
using NUnit.Framework;

namespace TestLomontSharp
{
    class TestAhoCorasick
    {

        [Test]
        public void Test1()
        {
            var rand = new Random(1234);

            AhoCorasick.Check();


            for (var pass = 0; pass < 1000; ++pass)
            {
                var text = TestBoyerMoore.Gen(rand, 10000);
                var patternCount = 1;//rand.Next(1, 4);
                var patterns = new List<byte[]>();

                //text = new byte[] {1,2,3,4};
                //patternCount = 1;
                //patterns = new List<byte[]>
                //{
                //    new byte[] {1,2 }
                //};

                for (var i =0 ; i < patternCount; ++i)
                    patterns.Add(TestBoyerMoore.Gen(rand, rand.Next(1, 5)));
                var matchesAho = AhoCorasick.Find(text, patterns);

                List<(int offset, int patternIndex)> matchesBrute = new List<(int offset, int patternIndex)>();
                for (var pi = 0; pi < patternCount; ++pi)
                {
                    var pattern = patterns[pi];
                    var temp = TestBoyerMoore.BruteFind(text, pattern);
                    matchesBrute.AddRange(temp.Select(offset=>(offset,pi)));
                }

                var same = true;
                Assert.True(matchesAho.Count == matchesBrute.Count);

                // need same ordering
                same &= matchesAho.TrueForAll(pair => matchesBrute.Contains(pair));
                same &= matchesBrute.TrueForAll(pair => matchesAho.Contains(pair));

                Assert.True(same);
            }
        }

    }
}
