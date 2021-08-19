#if false// todo - add back once .net 6.0

namespace TestLomontSharp
{
    class TestVector
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {

            // half float
            var vh1 = new Vec3h((Half)1, (Half)2, (Half)3);
            var vh2 = new Vec3h((Half)7, (Half)8, (Half)11);

            // float
            var vf1 = new Vec3f(1, 2, 3);
            var vf2 = new Vec3f(7, 8, 11);

            // double
            var vd1 = new Vec3d(1, 2, 3);
            var vd2 = new Vec3d(7, 8, 11);

            // int
            var vi1 = new Vec3i(1, 2, 3);
            var vi2 = new Vec3i(7, 8, 11);


            Assert.AreEqual((vh1 + vh2).ToString(), "(8,10,14)");
            Assert.AreEqual((vf1 + vf2).ToString(), "(8,10,14)");
            Assert.AreEqual((vd1 + vd2).ToString(), "(8,10,14)");
            Assert.AreEqual((vi1 + vi2).ToString(), "(8,10,14)");


            var m = new Mat4d(
                1, 0, 5, 0,
                0, 1, 0, 4,
                2, 0, 1, 0,
                0, 3, 0, 1
                );

            var v4 = new Vec4d(2, 4, 5, 6);

            Assert.AreEqual((m*v4).ToString(), "(27,28,9,18)");
        }

    }
}
#endif