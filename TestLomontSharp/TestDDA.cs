using System;
using System.Collections.Generic;
using System.Drawing;
using Lomont.Algorithms;
using NUnit.Framework;

namespace TestLomontSharp
{
    public class TestDDA
    {
        Random rand = new Random(1234); // make reproducible

        //[SetUp]
        //public void Setup()
        //{
        //}

        List<Point> Compute1(Point p1, Point p2)
        {
            var lst = new List<Point>();
            DDA.Dim2(p1.X,p1.Y,p2.X,p2.Y, (x, y) => lst.Add(new(x, y)));
            return lst;
        }
        List<Point> Compute2(Point p1, Point p2)
        {
            var lst = new List<Point>();
            foreach (var (x, y) in DDA.Dim2(p1.X, p1.Y, p2.X, p2.Y))
                lst.Add(new(x, y));
            return lst;
        }

        List<Point> Compute3(Point p1, Point p2)
        {
            var lst = new List<Point>();
            foreach (var pt in DDA.DimN(new List<int> { p1.X, p1.Y }, new List<int> { p2.X, p2.Y }))
                lst.Add(new(pt[0], pt[1]));
            return lst;
        }

        List<Point> Compute4(Point p1, Point p2)
        {
            var lst = new List<Point>();
            DDA.DimN(new List<int>{p1.X,p1.Y}, new List<int>{p2.X,p2.Y}, pt=>lst.Add(new (pt[0],pt[1])));
            return lst;
        }

        List<List<Point>> MakeN(Point p1, Point p2)
        {
            return new()
            {
                Compute1(p1, p2)
                ,Compute2(p1, p2)
                //,Compute3(p1, p2)
                //,Compute4(p1, p2)
            };
        }

        void TestAll(Point p1, Point p2, Action<List<Point>> test)
        {
            var lsts = MakeN(p1, p2);
            foreach (var lst in lsts)
                test(lst);
        }

        // Manhattan dist
        int Dist(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }

        [Test]
        public void TestRandom()
        {
            var sz = 1000; 
            for (var i = 0; i < 500; ++i)
            {
                var x1 = rand.Next(-sz, sz + 1);
                var y1 = rand.Next(-sz, sz + 1);
                var x2 = rand.Next(-sz, sz + 1);
                var y2 = rand.Next(-sz, sz + 1);
                var seqs = MakeN(new Point(x1, y1), new Point(x2, y2));
                for (var j = 0; j < seqs.Count; ++j)
                {
                    for (var k = 0; k < seqs[0].Count; ++k)
                        Assert.True(Dist(seqs[0][k], seqs[j][k]) == 0); // todo - make ==0
                }
            }
        }

        [Test]
        public void TestRandomSymmetry()
        {
            var sz = 1000;
            for (var i = 0; i < 500; ++i)
            {
                var x1 = rand.Next(-sz, sz + 1);
                var y1 = rand.Next(-sz, sz + 1);
                var x2 = rand.Next(-sz, sz + 1);
                var y2 = rand.Next(-sz, sz + 1);
                //(x1, y1, x2, y2) = (0, 0, 14, 23);
                var (p1, p2) = (new Point(x1, y1), new Point(x2, y2));
                var seqs1 = MakeN(p1,p2);
                var seqs2 = MakeN(p2,p1);
                for (var j = 0; j < seqs1.Count; ++j)
                {
                    var len = seqs1[0].Count;
                    for (var k = 0; k < len; ++k)
                    {
                        Assert.True(Dist(seqs1[0][k], seqs1[j][k]) == 0);
                        Assert.True(Dist(seqs1[0][k], seqs2[j][len - 1 - k]) < 2); // todo - make 0
                    }
                }
            }
        }



        [Test]
        public void TestDDA1()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(10, 10);

            TestAll(p1, p2,
                lst =>
                {
                    Assert.True(lst.Count == 11);

                    for (var i = 0; i < lst.Count; ++i)
                    {
                        var p = lst[i];
                        Assert.True(p.X == i && p.Y == i);
                    }

                }
            );
        }
    }
}
