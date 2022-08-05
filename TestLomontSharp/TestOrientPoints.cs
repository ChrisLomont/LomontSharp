using NUnit.Framework;

namespace TestLomontSharp
{
    internal class TestOrientPoints
    {
        [Test]
        public void Test1()
        {
            Assert.True(Lomont.Geometry.OrientPoints.TestOrientation3D());
            // todo - also test 2d, currently broken?
        }
    }
}
