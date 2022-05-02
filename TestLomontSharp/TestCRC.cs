using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Information;
using NUnit.Framework;

namespace TestLomontSharp
{
    public class TestCRC
    {
        [Test]
        public void TestCRC1()
        {
            var check = "123456789";
            var checkBytes = ASCIIEncoding.ASCII.GetBytes(check);

            Assert.AreEqual(CRC.CRC_32(checkBytes), 0xCBF43926);
            
            //Assert.AreEqual(CRC.CRC_32K(checkBytes),
            //    0x2D3DD0AE
            //    0xE4C03E42
            //        );



        }

        [Test]
        public void TestClass()
        {

            var check = "123456789";
            var checkBytes = ASCIIEncoding.ASCII.GetBytes(check);

            var c1 = new CRC32();
            c1.Start();
            c1.Add(checkBytes);
            var crc = c1.Finish();
            Assert.AreEqual(crc, 0xCBF43926);

            c1.Start();
            foreach (var b in checkBytes )
                c1.Add(b);
            crc = c1.Finish();
            Assert.AreEqual(crc, 0xCBF43926);
        }

        void SameTbl(uint [] tbl1, uint [] tbl2)
        {
            for (var i = 0; i < tbl1.Length; ++i)
                Assert.AreEqual(tbl1[i], tbl2[i]);
        }

        [Test]
        public void TestCRCTable()
        {
            SameTbl(
                CRC.MakeCRCTable(32, 0x04C11DB7, true).Select(v=>(uint)v).ToArray(),
                CRC.crc32Table
                );
            SameTbl(
                CRC.MakeCRCTable(32, 0x741B8CD7, false).Select(v => (uint)v).ToArray(),
                CRC.crc32KTable
            );

            SameTbl(
                CRC.MakeCRCTable(16, 0x1021, false).Select(v => (uint)v).ToArray(),
                CRC.crcTable16_1021.Select(v=>(uint)v).ToArray()
            );

            SameTbl(
                CRC.MakeCRCTable(12, 0x0B41, false).Select(v => (uint)v).ToArray(),
                CRC.crcTable12_0B41.Select(v => (uint)v).ToArray()
            );

            //SameTbl(
            //    CRC.MakeTable(24, 0x1864CFB, true, true),
            //    CRC.Crc24QTable
            //);
        }

        [Test]
        public void Test24Q()
        {
            var check = "123456789";
            var checkBytes = ASCIIEncoding.ASCII.GetBytes(check);
            var crc24 = CRC.CRC_24Q(checkBytes);
            Assert.AreEqual(crc24, 0x00CDE703);
        }

        // [Test]
        public void Test16()
        {
            // //0xE5CC - http://srecord.sourceforge.net/crc16-ccitt.html says 0x29B1 is WRONG
            // // new("CRC-16/CCITT",16,0x1021,0xFFFF,false,false,0x0000, 0x0C73, "X25","ADCCP","SDLC/HDLC","XMODEM"),
            // var crc = new CRC(CRC.CRCDefs[0]);
            // 
            // // 0x1->17 CE
            // // 0x2->0F DF
            // // 0x4->1F BE
            // // 0x8->1F 3F
            // // 0xF->18 90
            // var crc1 = crc.Hash(new byte[] { 0x1 });
            // var crc2 = crc.Hash(new byte[] { 0x2 });
            // var crc3 = crc.Hash(new byte[] { 0x4 });
            // var crc4 = crc.Hash(new byte[] { 0x8 });
            // var crc5 = crc.Hash(new byte[] { 0xF });
            // Assert.True(false);

        }

        [Test]
        public void TestDefs()
        {
            var check = "123456789";
            var checkBytes = ASCIIEncoding.ASCII.GetBytes(check);

            List<string> failed = new();
            foreach (var d in CRC.CRCDefs)
            {
                if (d.Check.HasValue)
                {
                    var crc = new CRC(
                        d.BitWidth, 
                        d.NormalPolynomial, 
                        d.InitialValue, 
                        d.ReflectInput, 
                        d.ReflectOutput,
                        d.FinalXOR);
                    var c = crc.Hash(checkBytes);
                    if (c != d.Check)
                        failed.Add(d.Name);
                }
            }

            Assert.AreEqual(failed.Count, 0);


        }

    }
}
