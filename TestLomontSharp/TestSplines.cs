using System.Numerics;
using Lomont.Numerical;
using NUnit.Framework;
using Lomont.Geometry;
using System.Collections.Generic;
using System;
using NUnit.Framework.Legacy;

namespace Tests
{
    public class NurbLibTests
    {
        Vec3 NewPoint3(Random r)
        {
            var x = r.Next(-10, 10);
            var y = r.Next(-10, 10);
            var z = r.Next(-10, 10);
            return new Vec3(x, y, z);
        }

        Vec2 NewPoint2(Random r)
        {
            var x = r.Next(-10, 10);
            var y = r.Next(-10, 10);
            return new Vec2(x, y);
        }

        bool Same3(Vec3 a, Vec3 b)
        {
            return (a - b).Length < 0.0001;
        }
        bool Same2(Vec2 a, Vec2 b)
        {
            return (a - b).Length < 0.0001;
        }

        [SetUp]
        public void Setup()
        {
        }

        public void BasicTestT<T>(Func<Random, T> NewPoint, Func<T, T, bool> Same)
                where T :
                IAdditiveIdentity<T, T>,
                IAdditionOperators<T, T, T>,
                ISubtractionOperators<T, T, T>,
                IMultiplyOperators<T, double, T>,
                IDistance<T, T, double>
        {
            // from Example 2.3 of section 2.5, extended in section 3.1

            // must have number of knots = number of points + degree + 1
            var r = new Random(1234);
            var P0 = NewPoint(r);
            var P1 = NewPoint(r);
            var P2 = NewPoint(r);
            var P3 = NewPoint(r);
            var P4 = NewPoint(r);
            var P5 = NewPoint(r);
            var P6 = NewPoint(r);
            var P7 = NewPoint(r);
            var points = new List<T> { P0, P1, P2, P3, P4, P5, P6, P7 };

            var p = 2; // degree 2
            var U = new double[] { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 }; // 11 knots
            var bspline = new BSplineT<T>(p, points, U);

            // must have 11 = # points + 2 + 1 => #points = 8

            var u = 5.0 / 2.0; // eval here
            var C = bspline.CurvePoint(u);

            // C(5/2) = 1/2 p2 + 6/8 p3 + 1/8 p4
            ClassicAssert.True(Same(C, P2 * (1.0 / 8.0) + P3 * (6 / 8.0) + P4 * (1.0 / 8.0)));
        }


        [Test]
        public void BasicTest()
        {
            //BasicTestT(r => r.NextDouble(), (a, b) => Math.Abs(a - b) < 0.0001);
            BasicTestT(NewPoint2, Same2);
            BasicTestT(NewPoint3, Same3);
        }

        void DerivativesT<T>(Func<Random, T> NewPoint, Func<T, T, bool> Same)
                where T :
                IAdditiveIdentity<T, T>,
                IAdditionOperators<T, T, T>,
                ISubtractionOperators<T, T, T>,
                IMultiplyOperators<T, double, T>,
                IDistance<T, T, double>
        {
            var r = new Random(1234);
            double[] U = { 0, 0, 0, 1, 2, 3, 4, 4, 5, 5, 5 }; // 11 knots
            int p = 2; // quadratic curve, so needs 11=n+p+1 => n=8 points
            // example from section 3.3
            for (var pass = 0; pass < 3; ++pass)
            {
                var P0 = NewPoint(r);
                var P1 = NewPoint(r);
                var P2 = NewPoint(r);
                var P3 = NewPoint(r);
                var P4 = NewPoint(r);
                var P5 = NewPoint(r);
                var P6 = NewPoint(r);
                var P7 = NewPoint(r);
                var pts = new List<T> { P0, P1, P2, P3, P4, P5, P6, P7 };
                var bs = new BSplineT<T>(p, pts, U);
                var u = 5.0 / 2.0; // parameter
                var ders = bs.CurveDerivatives(u);

                ClassicAssert.True(ders.Count == p + 1);

                var C0 = ders[0];
                // C(5/2) = 1/2 p2 + 6/8 p3 + 1/8 p4
                ClassicAssert.True(Same(C0, (P2 * (1.0 / 8.0) + P3 * (6 / 8.0) + P4 * (1.0 / 8.0))));

                var C1 = ders[1];
                ClassicAssert.True(Same(C1, P2 * (-0.5) + P4 * 0.5));
            }
        }

        [Test]
        public void Derivatives()
        {
            //DerivativesT<double>(r=>r.NextDouble(), (a,b)=>Math.Abs(a-b)<0.0001);
            DerivativesT<Vec2>(NewPoint2, Same2);
            DerivativesT<Vec3>(NewPoint3, Same3);
        }

        [Test]
        public void Interface()
        {
            var r = new Random(1323);
            var degree = 3;
            // 5 points
            var points = new List<Vec3> { NewPoint3(r), NewPoint3(r), NewPoint3(r), NewPoint3(r), NewPoint3(r) };
            // # knots = # points + degree + 1 = 5+3+1 = 9
            var knots = new double[] { 0, 0, 0, 0.4, 0.5, 0.8, 1, 1, 1 };
            var bs1 = new BSpline3(degree, points, knots);
            var bs2 = new BSpline3(bs1);

            var u = 0.6;
            var p1 = bs1.CurvePoint(u);
            var p2 = bs2.CurvePoint(u);
            ClassicAssert.True(Same3(p1, p2));

#if true
            ClassicAssert.AreEqual(bs1.Knots.Length, 9);
            ClassicAssert.AreEqual(bs1.Points.Count, 5);
            bs1.InsertKnot(0.7, 2);
            ClassicAssert.AreEqual(bs1.Knots.Length, 11);
            ClassicAssert.AreEqual(bs1.Points.Count, 7);

            bs2.InsertKnot(0.45, 1);
            bs1.InsertKnot(0.75, 3);

            var p1b = bs1.CurvePoint(u);
            var p2b = bs2.CurvePoint(u);
            ClassicAssert.True(Same3(p1, p1b));
            ClassicAssert.True(Same3(p1b, p2b));

            var len = bs1.Points.Count;
            ClassicAssert.AreEqual(len, 10);


            var removed = bs1.RemoveKnot(0.7, 1);
            ClassicAssert.AreEqual(removed, 1);
            ClassicAssert.AreEqual(bs1.Points.Count, len - 1);
            var p1c = bs1.CurvePoint(u);
            ClassicAssert.True(Same3(p1, p1c));

            removed = bs1.RemoveKnot(0.7, 1);
            ClassicAssert.AreEqual(removed, 1);
            ClassicAssert.AreEqual(bs1.Points.Count, len - 2);
            var p1d = bs1.CurvePoint(u);
            removed = bs1.RemoveKnot(0.7, 1);
            ClassicAssert.AreEqual(bs1.Points.Count, len - 2);
            ClassicAssert.AreEqual(removed, 0);

            ClassicAssert.True(Same3(p1, p1d));





#endif
        }
    }
}