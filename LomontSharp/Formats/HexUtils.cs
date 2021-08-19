using System;

namespace Lomont.Formats
{
    public static class HexUtils
    {
        public static void Hexdump(string filename, int start = 0, int length = -1)
        {
            // uses Chris Lomont hexdump code
            var hd = new HexDumper();
            hd.Dump(filename,start,length,false);
        }
        public static void Hexdump(byte [] data, int start = 0, int length = -1)
        {
            // uses Chris Lomont hexdump code
            var hd = new HexDumper();
            hd.Dump(data, start, length, false);
        }

        /// <summary>
        /// Hex string to value
        /// todo - move to utils?
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static ulong ParseHex(string hex)
        {
            if (hex.ToLower().StartsWith("0x"))
                return Convert.ToUInt64(hex, 16); // if prefixed

            // not prefixed
            return UInt64.Parse(hex, System.Globalization.NumberStyles.HexNumber);
        }

    }
}
