using System;
using System.Collections.Generic;
using Lomont.Algorithms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    class TestStableSort
    {
        string RandomString(int len, Random rand)
        {
            var s = "";
            while (len-- > 0)
                s += (char)(rand.Next(2) + 'a');
            return s;
        }

        class Pair
        {
            public int index;
            public string text;
        }

        [Test]
        public void Test1()
        {
            var rand = new Random(1234);

            for (var t = 0; t < 1000; ++t)
            {
                // list of random strings
                var list1 = new List<Pair>();
                var len = rand.Next(5, 100);
                for (var j = 0; j < len; ++j)
                {
                    var slen = rand.Next(3, 7);
                    list1.Add(new Pair{text = RandomString(slen, rand), index = j});
                }

                // copy it
                var list2 = new List<Pair>(list1);

                // sort one
                SortExtensions.StableSort(list1, (a, b) => a.text.CompareTo(b.text));
                //list1.Sort((a,b)=>a.Length.CompareTo(b.Length));

                // check order same:p
                for (var i = 0; i < len; ++i)
                for (var j = i+1; j < len; ++j)
                {
                    //ClassicAssert.True(list1[i].text <= list1[j].text);
                    if (list1[i].text != list1[j].text) continue;
                    ClassicAssert.True(list1[i].index < list1[j].index);
                }

            }
        }
    }
}
