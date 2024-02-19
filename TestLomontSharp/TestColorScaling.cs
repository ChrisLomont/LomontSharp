using Lomont.Graphics;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestColorScaling
    {
        [Test]
        public void TestScaling()
        {
            // test roundtrip colors
            for (var c1 = 0; c1 <= 255; ++c1)
            {
                var r  = ColorUtils.Downscale((byte)c1);
                ClassicAssert.True(0<=r && r <= 1.0);
                var c2 = ColorUtils.Upscale(r);
                ClassicAssert.AreEqual(c1,c2);
                ClassicAssert.AreEqual(c2, (byte)(255 * r));
            }
        }
    }
}
