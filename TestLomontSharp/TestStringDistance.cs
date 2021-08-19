using System;
using Lomont.Algorithms;
using NUnit.Framework;

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

            Assert.AreEqual(StringDistance.Hamming("karolin", "karolin"), 0);
            Assert.AreEqual(StringDistance.Levenshtein("karolin", "karolin"), 0);
            Assert.AreEqual(StringDistance.DamerauLevenshtein("karolin", "karolin"), 0);

            Assert.AreEqual(StringDistance.Hamming("karolin", "kathrin"),3);
            Assert.AreEqual(StringDistance.Levenshtein("kitten ", "sitting"), 3);
            Assert.AreEqual(StringDistance.DamerauLevenshtein("kitten ", "sitting"), 3);

            Assert.AreEqual(StringDistance.Levenshtein("a cat", "an act"), 3);
            Assert.AreEqual(StringDistance.DamerauLevenshtein("a cat", "an act"), 2);

            Assert.AreEqual(StringDistance.Levenshtein("ABC", "ACB"), 2);
            Assert.AreEqual(StringDistance.DamerauLevenshtein("ABC", "ACB"), 1);



            Assert.AreEqual(StringDistance.LongestCommonSubsequence("human ", "chimpanzee"), (4,"hman"));

            var js = StringDistance.JaroSimilarity("winkler", "welfare");
            var jw = StringDistance.JaroWinklerSimilarity("winkler", "welfare");
            Assert.True(Math.Abs(js-0.631)<0.005);
            // https://srinivas-kulkarni.medium.com/jaro-winkler-vs-levenshtein-distance-2eab21832fd6
            // todo - why not wuit same? Assert.True(Math.Abs(jw-0.6337)<0.001);
        }

    }
}
