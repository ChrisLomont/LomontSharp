using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    internal class TestOrientPoints
    {
        [Test]
        public void Test1()
        {
            ClassicAssert.True(Lomont.Geometry.OrientPoints.TestOrientation3D());
            ClassicAssert.True(Lomont.Geometry.OrientPoints.TestOrientation2D());
        }
    }
}
