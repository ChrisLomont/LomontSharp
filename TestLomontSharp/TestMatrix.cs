using System;
using System.Collections.Generic;
using System.Linq;
using Lomont.Algorithms;
using Lomont.Numerical;
using NUnit.Framework;

namespace TestLomontSharp
{
    class TestMatrix
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestRotation()
        {
            // check rotation orientations are right handed 
            // (positive angle is counterclockwise looking down the rotation axis)
            var tolerance = 1e-10;
            var angle = Math.PI / 2; // 90 degrees

            var t1 = (Mat4.XRotation(angle) * Vec3.YAxis - Vec3.ZAxis).Length;
            Assert.True(t1 < tolerance);

            var t2 = (Mat4.YRotation(angle) * Vec3.ZAxis - Vec3.XAxis).Length;
            Assert.True(t2 < tolerance);

            var t3 = (Mat4.ZRotation(angle) * Vec3.XAxis - Vec3.YAxis).Length; 
            Assert.True(t3 < tolerance);

        }

        [Test]
        public void TestTranslation()
        {
            var m1 = Mat4.Translation(new Vec3(1, 2, 3));
            var ans1 = m1 * new Vec3(4, 6, 11);
            Assert.AreEqual((new Vec3(5, 8, 14) - ans1).Length,0.0); // should be identical, no float error here

            var m2 = Mat4.Translation(1,2,3);
            var ans2 = m2 * new Vec3(7, 2, -1);
            Assert.AreEqual((new Vec3(8, 4, 2) - ans2).Length, 0.0); // should be identical, no float error here
        }
    }
}
