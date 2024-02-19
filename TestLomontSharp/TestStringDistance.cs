using System;
using Lomont.Algorithms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestStringDistance
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            // check against http://www.let.rug.nl/~kleiweg/lev/
            // check against https://asecuritysite.com/forensics/simstring

            ClassicAssert.AreEqual(StringDistance.Hamming("karolin", "karolin"), 0);
            ClassicAssert.AreEqual(StringDistance.Levenshtein("karolin", "karolin"), 0);
            ClassicAssert.AreEqual(StringDistance.DamerauLevenshtein("karolin", "karolin"), 0);

            ClassicAssert.AreEqual(StringDistance.Hamming("karolin", "kathrin"),3);
            ClassicAssert.AreEqual(StringDistance.Levenshtein("kitten ", "sitting"), 3);
            ClassicAssert.AreEqual(StringDistance.DamerauLevenshtein("kitten ", "sitting"), 3);

            ClassicAssert.AreEqual(StringDistance.Levenshtein("a cat", "an act"), 3);
            ClassicAssert.AreEqual(StringDistance.DamerauLevenshtein("a cat", "an act"), 2);

            ClassicAssert.AreEqual(StringDistance.Levenshtein("ABC", "ACB"), 2);
            ClassicAssert.AreEqual(StringDistance.DamerauLevenshtein("ABC", "ACB"), 1);



            ClassicAssert.AreEqual(StringDistance.LongestCommonSubsequence("human ", "chimpanzee"), (4,"hman"));

            var js = StringDistance.JaroSimilarity("winkler", "welfare");
            var jw = StringDistance.JaroWinklerSimilarity("winkler", "welfare");
            ClassicAssert.True(Math.Abs(js-0.631)<0.005);
            // https://srinivas-kulkarni.medium.com/jaro-winkler-vs-levenshtein-distance-2eab21832fd6
            // todo - why not wuit same? ClassicAssert.True(Math.Abs(jw-0.6337)<0.001);
        }

    }
}
