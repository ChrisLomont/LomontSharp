using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Formats;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace TestLomontSharp
{
    public class TestBitStream
    {
        [Test]
        public void Test()
        {
            var bs = new BitStream(1);
            bs.WriteBitsMsb(1234, 17);
            ClassicAssert.AreEqual(bs.BitLength,17);
            bs.WriteBitsMsb(7686, 21);
            ClassicAssert.AreEqual(bs.BitLength, 38);
            bs.WriteBitsMsb(247, 8);
            ClassicAssert.AreEqual(bs.BitLength, 46);
            bs.Pad(8);
            ClassicAssert.AreEqual(bs.BitLength, 48);
            bs.WriteBit(1);
            ClassicAssert.AreEqual(bs.BitLength, 49);
            bs.Pad(32);
            ClassicAssert.AreEqual(bs.BitLength, 64);

            var i1 = bs.ReadBitsMsb(0, 17);
            var i2 = bs.ReadBitsMsb(17, 21);
            var i3 = bs.ReadBitsMsb(38, 8);

            ClassicAssert.AreEqual(i1, 1234);
            ClassicAssert.AreEqual(i2, 7686);
            ClassicAssert.AreEqual(i3, 247);

            bs.ReadPosition = 0;
            ClassicAssert.AreEqual(bs.ReadBitsMsb(17),1234);
            ClassicAssert.AreEqual(bs.ReadPosition,17);
            bs.ReadPosition = 0;
            ClassicAssert.AreEqual(bs.ReadBitsMsb(17), 1234);
            bs.ReadPosition = 38;
            ClassicAssert.AreEqual(bs.ReadBitsMsb(8), 247);
            ClassicAssert.AreEqual(bs.ReadPosition, 46);



        }
    }
}
