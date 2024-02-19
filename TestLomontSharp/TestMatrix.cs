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
        public void TestCofactors()
        {
            var c1 = new Mat4(
                1,2,3,4,
                5,4,7,2,
                7,8,1,2,
                4,4,1,2
                );
            var c2 = Mat4.Cofactors(c1);
            var c3 = new Mat4(
                -48,36,8,20,
                8,-6,20,-14,
                -72,86,12,-34,
                160,-152,-48,72
                );
            Assert.True((c3 - c2).MaxNorm() == 0);
        }

        [Test]
        public void TestOps()
        {
            var m3 = new Mat3();
            var m4 = new Mat4();

            // set some values
            m3.Apply((i, j, v) => i + j * j);
            m4.Apply((i, j, v) => i + j * j);

            // *,+, -, /
            var m5 = 2 * m3 + m3 * 5 - m3 * m3 / 3.0;
            var m6 = 2 * m4 + m4 * 5 - m4 * m4 / 3.0;

            Assert.True(m5 != Mat3.Zero);
            Assert.True(m6 != Mat4.Zero);

            // unary
            var m7 = +m5 + (-m5);
            var m8 = +m6 + (-m6);
            Assert.True(m7 == Mat3.Zero);
            Assert.True(m8 == Mat4.Zero);


            Assert.True(true);
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
            Assert.AreEqual((new Vec3(5, 8, 14) - ans1).Length, 0.0); // should be identical, no float error here

            var m2 = Mat4.Translation(1, 2, 3);
            var ans2 = m2 * new Vec3(7, 2, -1);
            Assert.AreEqual((new Vec3(8, 4, 2) - ans2).Length, 0.0); // should be identical, no float error here
        }

        [Test]
        public void Test3x2Mult()
        {
            var v2 = new Vec2(1, 2);
            var m = new Mat3(
                3, 1, 10,
                6, 7, 10,
                9, 10, -24
            );

            // projective
            // x' = 3*1+1*2+10 = 15
            // y' = 6*1+7*2+10 = 30
            // w' = 9*1+10*2-24 = 5
            // x = x'/w' = 3
            // y = y'/w'= 6

            var ans = m * v2; // treat as projective
            Console.WriteLine($"{ans}");

            Assert.True((ans - new Vec2(3,6)).Length < 0.0001);
        }


        [Test]
        public void Test3Mult()
        {
            var v3 = new Vec3(1, 2, 3);
            var m = new Mat3(
                3, 4, 5,
                6, 7, 8,
                9, 10, 11
                );
            Assert.True((m * v3 - new Vec3(26, 44, 62)).Length<0.0001);
        }
        [Test]
        public void Test3Outer()
        {
            var m = Vec3.Outer(new Vec3(1, 2, 3), new Vec3(5, 4, 7));
            Assert.True(
                (m - 
                new Mat3(
                    5, 4, 7,
                    10, 8, 14,
                    15, 12, 21
                    )).MaxNorm()<0.00001);
        }

        [Test]
        public void Test3DetInv()
        {
            var m1 = new Mat3(-6, -9, 7, -2, -7, 4, -5, -3, 4);
            var m2 = new Mat3(-16, 15, 13, -12, 11, 10, -29, 27, 24);

            Assert.True(Math.Abs(m1.Det - 1) < 0.00001);
            Assert.True(Math.Abs(m2.Det - 1) < 0.00001);
            Assert.True((m1 * m2 - Mat3.Identity).MaxNorm()<0.000001);
            Assert.True((m2 * m1 - Mat3.Identity).MaxNorm()<0.000001);

            Assert.True((m1.Inverse()- m2).MaxNorm()<0.000001);
            Assert.True((m2.Inverse()- m1).MaxNorm()<0.000001);
        }

        [Test]
        public void Test3to4()
        {
            var m1a = new Mat3(-6, -9, 7, -2, -7, 4, -5, -3, 4);
            var m4 = new Mat4(m1a);
            var m1b = m4.ToMat3();
            Assert.True((m1a- m1b).MaxNorm()<0.0001);
            Assert.True(Math.Abs(m4[3, 3]-1)<0.0001);
        }

        [Test]
        public void TestRotVec()
        {
            var rx = Mat3.FromRotationVector(Vec3.XAxis * Math.PI);

            var p1 = new Vec3(0,0,1);

            Assert.True((rx*p1 - new Vec3(0,0,-1)).Length < 0.0001);

            rx = Mat3.FromRotationVector(Vec3.XAxis * Math.PI/2);

            p1 = new Vec3(0, 0, 1);

            Assert.True((rx * p1 - new Vec3(0, -1, 0)).Length < 0.0001);

            //var v = rx.ToRotationVector();
            //Assert.True((v - Vec3.XAxis * Math.PI).Length < 0.0001);

            var r0 = Mat3.FromRotationVector(Vec3.Zero); // should be identity
            Assert.True((r0-Mat3.Identity).MaxNorm() < 0.00001);

        }

        [Test]
        public void TestExp()
        {
             //Mat3.RotationExp();
            //Assert.True(false);
        }

    }
}
