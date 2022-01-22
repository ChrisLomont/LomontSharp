using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Formats;
using NUnit.Framework;

namespace TestLomontSharp
{
    public class TestBitStream
    {
        [Test]
        public void Test()
        {
            var bs = new BitStream(1);
            bs.WriteBitsMsb(1234, 17);
            Assert.AreEqual(bs.BitLength,17);
            bs.WriteBitsMsb(7686, 21);
            Assert.AreEqual(bs.BitLength, 38);
            bs.WriteBitsMsb(247, 8);
            Assert.AreEqual(bs.BitLength, 46);
            bs.Pad(8);
            Assert.AreEqual(bs.BitLength, 48);
            bs.WriteBit(1);
            Assert.AreEqual(bs.BitLength, 49);
            bs.Pad(32);
            Assert.AreEqual(bs.BitLength, 64);

            var i1 = bs.ReadBitsMsb(0, 17);
            var i2 = bs.ReadBitsMsb(17, 21);
            var i3 = bs.ReadBitsMsb(38, 8);

            Assert.AreEqual(i1, 1234);
            Assert.AreEqual(i2, 7686);
            Assert.AreEqual(i3, 247);

            bs.ReadPosition = 0;
            Assert.AreEqual(bs.ReadBitsMsb(17),1234);
            Assert.AreEqual(bs.ReadPosition,17);
            bs.ReadPosition = 0;
            Assert.AreEqual(bs.ReadBitsMsb(17), 1234);
            bs.ReadPosition = 38;
            Assert.AreEqual(bs.ReadBitsMsb(8), 247);
            Assert.AreEqual(bs.ReadPosition, 46);



        }
    }
}
