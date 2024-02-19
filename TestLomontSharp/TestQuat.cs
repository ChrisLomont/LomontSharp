using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.Algorithms;
using Lomont.Numerical;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    class TestQuat
    {
        [SetUp]
        public void Setup()
        {
        }



        [Test]
        public void TestAngle()
        {
            var r = new Random(1234);
            Quat RandQuat() => Quat.FromAxisAngle(Vec3.SphericalRandom(r.NextDouble(), r.NextDouble()), r.NextDouble() * Math.PI * 2);
            double tol = 1e-8;

            void Test(Quat q1, Quat q2)
            {
                var d1 = Quat.AngleBetween(q1, q2); // default
                var d2 = Quat.AngleBetween(q1, -q2); // same rotation
                var d3 = Quat.AngleBetween(q2, q1); // symmetric

                //var d4 = Quat.AngleBetween2(q1, q2);
                //var d5 = Quat.AngleBetween2(q1, -q2);
                //var d6 = Quat.AngleBetween2(q2, q1);

                ClassicAssert.True(Math.Abs(d1 - d2) < tol);
                ClassicAssert.True(Math.Abs(d1 - d3) < tol);

                ClassicAssert.True(d1<=Math.PI);
                ClassicAssert.True(0 <= d1);

                //Console.WriteLine($"{d4} {d6}");
                //ClassicAssert.True(Math.Abs(d4 - d5) < tol);
                //ClassicAssert.True(Math.Abs(d4 - d6) < tol);
            }

            // test edges
            var qz1 = Quat.FromAxisAngle(Vec3.XAxis, 0);
            var qz2 = Quat.FromAxisAngle(Vec3.XAxis, 0);
            Test(qz1, qz2);
            Test(qz1, qz1);

            for (var i =0; i < 100; ++i)
            {
                var q1 = RandQuat();
                var q2 = RandQuat();
                Test(q1, q2);

            }

        }
    }
}