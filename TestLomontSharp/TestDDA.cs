using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Lomont.Algorithms;
using NUnit.Framework;
using NUnit.Framework.Legacy;

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
            foreach (var (x, y) in DDA.Dim2Fast(p1.X, p1.Y, p2.X, p2.Y))
                lst.Add(new(x, y));
            return lst;
        }
        List<Point> Compute2a(Point p1, Point p2)
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
                ,Compute2a(p1, p2)
                ,Compute3(p1, p2)
                ,Compute4(p1, p2)
            };
        }

        // apply test to each seq
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


        // are lists the same points, up to given distance?
        bool Same(List<Point> s1, List<Point> s2, int dist = 0)
        {
            if (s1.Count != s2.Count) return false;
            for (var i = 0; i < s1.Count; ++i)
                if (Dist(s1[i], s2[i]) > dist)
                    return false;
            return true;
        }

        // slope of points
        double Slope(Point p1, Point p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            return (double)dy / dx;
        }

        // length of line in pixels
        int Length(Point p1, Point p2)
        {
            var dx = Math.Abs(p2.X - p1.X);
            var dy = Math.Abs(p2.Y - p1.Y);
            return Math.Max(dx, dy) + 1;
        }

        //  Make random points
        (Point p1, Point p2) Pts(int sz)
        {
            var x1 = rand.Next(-sz, sz + 1);
            var y1 = rand.Next(-sz, sz + 1);
            var x2 = rand.Next(-sz, sz + 1);
            var y2 = rand.Next(-sz, sz + 1);

            return (new Point(x1, y1), new Point(x2, y2));

        }

        [Test]
        public void TestEndpoints()
        {
            var sz = 1000;
            for (var i = 0; i < 500; ++i)
            {
                var (p1, p2) = Pts(sz);

                TestAll(p1, p2, seq =>
                    {
                        ClassicAssert.True(seq[0] == p1);
                        ClassicAssert.True(seq.Last() == p2);
                    }
                );
            }
        }


        [Test]
        public void TestRandom()
        {
            var sz = 1000;
            for (var i = 0; i < 500; ++i)
            {
                var (p1, p2) = Pts(sz);

                var seqs = MakeN(p1, p2);

                foreach (var seq in seqs)
                    ClassicAssert.True(Same(seqs[0], seq, 1)); // todo - dist to 0
            }
        }

        [Test]
        public void TestRandomSymmetry()
        {
            var sz = 1000;
            for (var i = 0; i < 500; ++i)
            {
                var (p1,p2) = Pts(sz);

                var n = Length(p1,p2);
                var s1 = Compute2a(p1, p2);
                var s2 = Compute2a(p2, p1);

                ClassicAssert.True(s1.Count == n);
                ClassicAssert.True(s2.Count == n);

                s2.Reverse();
                ClassicAssert.True(Same(s1, s2));
            }
        }



        [Test]
        public void TestDiagonal()
        {
            var p1 = new Point(0, 0);
            var p2 = new Point(10, 10);

            TestAll(p1, p2,
                lst =>
                {
                    ClassicAssert.True(lst.Count == 11);

                    for (var i = 0; i < lst.Count; ++i)
                    {
                        var p = lst[i];
                        ClassicAssert.True(p.X == i && p.Y == i);
                    }

                }
            );
        }
    }
}
