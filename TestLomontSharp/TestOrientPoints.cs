using NUnit.Framework;

namespace TestLomontSharp
{
    internal class TestOrientPoints
    {
        [Test]
        public void Test1()
        {
            Assert.True(Lomont.Geometry.OrientPoints.TestOrientation3D());
            Assert.True(Lomont.Geometry.OrientPoints.TestOrientation2D());
        }
    }
}
