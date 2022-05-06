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
        public void Test1()
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

    }
}
