using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Formats
{
    public static class ByteWriter
    {
        public static void Write(List<byte> bytes, string text, bool zeroTerminated = true)
        {
            var txt = Encoding.UTF8.GetBytes(text);
            foreach (var b in txt)
                bytes.Add(b);
            if (zeroTerminated)
                bytes.Add(0); // 0 terminated
        }

        // MSB
        public static void Write(List<byte> bytes, int value, int length, int address = -1)
        {
            var shift = length * 8 - 8;
            for (var i = 0; i < length; ++i)
            {
                var b = (byte)(value >> shift);
                if (address == -1)
                    bytes.Add(b);
                else
                    bytes[address++] = b;
                shift -= 8;
            }
        }

    }
}
