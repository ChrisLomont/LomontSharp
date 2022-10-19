using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Containers;
using Lomont.Algorithms;

namespace TestLomontSharp
{
    public class TestBinaryTree
    {
        [Test]
        public void Test1()
        {
            var t = new BinaryTree<int>();
            var Nil = BinaryTree<int>.Nil;
            Assert.True(Nil != null);
            var t1 = t.Insert(1);
            Assert.True(t.Contains(1));
            Assert.False(t.Contains(0));
            var g1 = t.Find(1);
            Assert.True(g1 != null);
            Assert.True(g1.Value == 1);
            Assert.True(g1.Count == 1);

            // see if same
            var t2 = t.Insert(1);
            Assert.True(t1.Equals(t2));
            Assert.True(g1.Equals(t2));
            Assert.True(t1.Count == 2);
            Assert.True(g1.Count == 2);

            t.Delete(0); // try to remove

            t.Delete(1); // check count, still on in there?

            Assert.True(t.Successor(-1) == t2);
            Assert.True(t.Successor(1) == Nil);
            Assert.True(t.Successor(2) == Nil);

            Assert.True(t.Predecessor(-1) == Nil);
            Assert.True(t.Predecessor(1) == Nil);
            Assert.True(t.Predecessor(2) == t2);

            //var g2=t.Remove(1);
            Assert.True(t.Minimum().Value == 1);
            Assert.True(t.Maximum().Value == 1);

            t.Insert(10);
            Assert.True(t.Minimum().Value == 1);
            Assert.True(t.Maximum().Value == 10);

            t.Delete(1); // check none in 

            Assert.True(t.Minimum().Value == 10);
            Assert.True(t.Maximum().Value == 10);


            t.Insert(-20);
            Assert.True(t.Minimum().Value == -20);
            Assert.True(t.Maximum().Value == 10);


            t.Insert(20);
            Assert.True(t.Minimum().Value == -20);
            Assert.True(t.Maximum().Value == 20);

            t.Insert(-10);
            Assert.True(t.Minimum().Value == -20);
            Assert.True(t.Maximum().Value == 20);

            t.Delete(1); // try

            //t.Successor;
            //t.Remove;
            //t.Minimum;
            //t.Maximum;
            //t.Contains;
            //t.Predecessor;
        }

        [Test]
        public void TestOrder()
        {
            var t = new BinaryTree<int>();
            var Nil = BinaryTree<int>.Nil;
            // insert evens 
            var evens = Enumerable.Range(-100, 201).Select(v => v * 2).ToArray();
            evens.Shuffle();

            foreach (var e in evens)
                t.Insert(e);

            for (var k = -200; k <= 200; k += 2)
            {
                Assert.True(t.Contains(k));
                Assert.False(t.Contains(k + 1));
                if (k != -200 && k != 200)
                {
                    Assert.True(t.Successor(k+1).Value == k + 2);
                    Assert.True(t.Successor(k).Value == k + 2);
                    Assert.True(t.Predecessor(k).Value == k - 2);
                    Assert.True(t.Predecessor(k-1).Value == k - 2);
                }
            }

            Assert.AreEqual(t.Successor(8).Value, 10);
            Assert.AreEqual(t.Successor(9).Value, 10);
            Assert.AreEqual(t.Successor(10).Value, 12);
            Assert.AreEqual(t.Successor(11).Value, 12);
            Assert.True(t.Successor(1000) == Nil);
            Assert.True(t.Successor(-49).Value == -48);
            Assert.True(t.Successor(0).Value == 2);

            Assert.True(t.Predecessor(10).Value == 8);
            Assert.True(t.Predecessor(-1000) == Nil);
            Assert.True(t.Predecessor(-49).Value == -50);
            Assert.True(t.Predecessor(0).Value == -2);


        }

    }
}
