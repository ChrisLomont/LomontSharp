using System.Collections.Generic;

namespace Lomont.Information
{
    /// <summary>
    /// Simple CRC32 class, 0xEDB88320 polynomial
    /// </summary>
    public static class Crc32
    {
        /// <summary>
        /// NOT THREAD SAFE - todo
        /// </summary>
        /// <param name="data"></param>
        /// <param name="polynomial"></param>
        /// <param name="crc"></param>
        /// <returns></returns>
        public static uint Compute(IEnumerable<byte> data, uint polynomial = 0xEDB88320, uint crc = 0)
        {
            if (initPoly != polynomial)
                CRC32Init(polynomial);
            crc = ~crc;
            foreach (var b in data)
                crc = crc32Table[b ^ (crc & 0xFF)] ^ (crc >> 8);
            return ~crc;
        }

        static uint [] crc32Table = new uint[256];
        static uint initPoly = 0;

        static void CRC32Init(uint poly = 0xEDB88320)
        {

            for (uint i = 0; i < 256; ++i)
            {
                uint r = i;
                for (uint j = 0; j < 8; ++j)
                    r = (r >> 1) ^ (poly & ~((r & 1) - 1));

                crc32Table[i] = r;
            }
            initPoly = poly;
        }

    }
}
