using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// todo - all this needs cleaned, merged, tested

namespace Lomont.Formats
{
#if false
    /// <summary>
    /// Read or write values to a stream of bits
    /// </summary>
    public sealed class Bitstream : Output
    {
        public Bitstream(byte[] data)
        {
            foreach (var b in data)
                Write(b, 8);
        }

        public Bitstream()
        {

        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"{Length}: ");
            for (var i = 0U; i < Length; ++i)
            {
                uint index = i;
                sb.Append(ReadFrom(ref index, 1));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Position of next read/write
        /// </summary>
        public uint Position { get; set; } = 0;

        /// <summary>
        /// Number of bits in stream
        /// </summary>
        public uint Length => (uint)bits.Count;

        /// <summary>
        /// Clear stream
        /// </summary>
        public void Clear()
        {
            bits.Clear();
            Position = 0;
        }

        /// <summary>
        /// Write bits, MSB first
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitLength"></param>
        public void Write(uint value, uint bitLength)
        {
            Trace.Assert(bitLength <= 32);
            for (var i = 0U; i < bitLength; ++i)
            {
                var bit = (byte)((value >> ((int)bitLength - 1)) & 1);
                Write(bit.ToString());

                if (Position == Length)
                {
                    bits.Add(bit);
                    Position = Length;
                }
                else
                {
                    bits[(int)i] = bit;
                    Position = i + 1;
                }
                value <<= 1;
            }
        }

        /// <summary>
        /// Copy in the given BitStream
        /// </summary>
        /// <param name="bitstream"></param>
        public void WriteStream(Bitstream bitstream)
        {
            var temp = bitstream.Position;
            bitstream.Position = 0;
            while (bitstream.Position < bitstream.Length)
                Write(bitstream.Read(1), 1);
            bitstream.Position = temp;
        }

        /// <summary>
        /// Read values from given position, update it, MSB first
        /// </summary>
        /// <param name="position"></param>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        public uint ReadFrom(ref uint position, uint bitLength)
        {
            uint temp = Position; // save internal state
            Position = position;
            uint value = Read(bitLength);
            position = Position; // update external value
            Position = temp; // restore internal
            return value;
        }

        /// <summary>
        /// Write the value to the bitstream in the minimum number of bits.
        /// Writes 0 using 1 bit.
        /// </summary>
        /// <param name="value"></param>
        public void Write(uint value)
        {
            if (value != 0)
                Write(value, CodecBase.BitsRequired(value));
            else
                Write(0, 1);
        }


        /// <summary>
        /// Read values from current position, MSB first
        /// </summary>
        /// <param name="bitLength"></param>
        /// <returns></returns>
        public uint Read(uint bitLength)
        {
            var value = 0U;
            for (var i = 0; i < bitLength; ++i)
            {
                var b = (uint)(bits[(int)Position++] & 1);
                Write(b.ToString());
                value <<= 1;
                value |= b;
                //value |= b<<i;
            }
            return value;
        }


        /// <summary>
        /// Convert bitstream to bytes, padding with 0 as needed to fill last byte
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            var len = (bits.Count + 7) / 8;
            var data = new byte[len];
            for (var i = 0; i < len * 8; i += 8)
            {
                var val = 0;
                for (var j = 0; j < 8; ++j)
                {
#if false
                    // MSB
                    var t = (i + j < bits.Count) ? bits[i + j] : 0;
                    val |= t << j;
#else
                    // LSB - needed for range coding to work cleanly
                    var t = (i + j < bits.Count) ? bits[i + j] : 0;
                    val <<= 1;
                    val |= t & 1;
#endif
                }
                data[i / 8] = (byte)val;
            }
            return data;
        }

        public void InsertStream(uint insertPosition, Bitstream bitstream)
        {
            bits.InsertRange((int)insertPosition, bitstream.bits);
            // todo - position update?
        }


        // todo - make optimized later, one byte per bit ok for now
        readonly List<byte> bits = new List<byte>();
    }

    /// <summary>
    /// Handles streaming bits from a buffer in the order needed
    /// </summary>
    public sealed class BitStreamer
    {
        private readonly byte[] buffer;
        private int bitPos;  // next bit to output
        private int bytePos; // next byte to pick from

        private readonly ulong maxBitPos;
        private ulong bitsRead;
        public BitStreamer(byte[] data, ulong maxBitPos)
        {
            this.maxBitPos = maxBitPos;
            bitsRead = 0;
            buffer = data;
            bitPos = 7;
            bytePos = 0;
        }

        /// <summary>
        /// Next bit. Returns zeroes if off the end
        /// </summary>
        /// <returns></returns>
        public uint NextBit()
        {
            if (maxBitPos <= bitsRead)
                return 0;
            bitsRead++;

            var bit = buffer[bytePos] >> bitPos;
            bitPos--;
            if (bitPos < 0)
            {
                bytePos++;
                bitPos = 7;
            }
            return (uint)(bit & 1);
        }
    }

    class BitStream
    {
        // bit to write to
        private int bitIndex = 0;
        internal void Write(IList<byte> output, int value, int bitCount)
        {
            Trace.WriteLine($"->({value},{bitCount}) ");
            while (bitCount > 0)
            {
                var byteIndex = bitIndex >> 3;
                if (output.Count <= byteIndex)
                    output.Add(0);
                output[byteIndex] |= (byte)((value & 1) << (bitIndex & 7)); // set one bit
                value >>= 1;
                bitCount--;
                bitIndex++;
            }
        }

        /// <summary>
        /// Try to read bitCount bits from the compressed data stream.
        /// Return true if enough bits left, else false for overflow
        /// </summary>
        /// <param name="data"></param>
        /// <param name="bitCount"></param>
        /// <returns>The value obtained</returns>
        internal int Read(IReadOnlyList<byte> data, int bitCount)
        {
            var codeWord = 0;
            for (var i = 0; i < bitCount; ++i)
            {
                var byteIndex = bitIndex >> 3;
                if (data.Count <= byteIndex)
                    throw new Exception("Bitreader out of bounds");
                var b = (data[byteIndex] >> (bitIndex & 7)) & 1;
                codeWord |= (b << i);
                bitIndex++;
            }
            Trace.WriteLine("->({0},{1}) ", codeWord, bitCount);
            return codeWord;
        }
    }

    /// <summary>
    /// Manage reads/write to a (possibly) remote source of 
    /// bytes as a bitstream.
    /// </summary>
    private class FastBitStream
    {

        internal FastBitStream(List<byte> dataIn = null)
        {
            bitStream = dataIn ?? new List<byte>();
        }


        /// <summary>
        /// Write the value to the byte stream using bitCount bits
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitCount"></param>
        public void Write(int value, int bitCount)
        {
            DebugMessage("->({0},{1}) ", value, bitCount);

            var startByte = bitIndex >> 3; // starting byte
            var lastByte = (bitIndex + bitCount - 1) >> 3; // ending byte
            while (bitStream.Count <= lastByte)
                bitStream.Add(0); // pad end

            value &= (1 << bitCount) - 1; // mask word
            value <<= (bitIndex & 7); // align the word

            while (startByte <= lastByte)
            {
                bitStream[startByte++] |= (byte)(value & 255);
                value >>= 8; // next byte
            }
            bitIndex += bitCount; // this many bits used up
        }

        /// <summary>
        /// Try to read bitCount bits from the compressed data stream.
        /// Return true if enough bits left, else false for overflow
        /// </summary>
        /// <param name="bitCount"></param>
        /// <param name="value"></param>
        /// <returns>The value obtained</returns>
        public bool Read(int bitCount, out ushort value)
        {
            var codeWord = 0;
            value = 0;
            // get next codeword, update internals
            // we do this to avoid reading memory that is not ours to read
            long startByte = bitIndex >> 3;
            long endByte = (bitIndex + bitCount - 1) >> 3;
            if (endByte >= bitStream.Count)
                return false; // will access out of bounds
            while (endByte >= startByte)
            {
                codeWord <<= 8;
                codeWord |= bitStream[(int)endByte--];
            }

            // shift down and mask
            codeWord = (codeWord >> (bitIndex & 7)) & ((1 << bitCount) - 1);
            // update position
            bitIndex += bitCount;

            DebugMessage("->({0},{1}) ", codeWord, bitCount);
            value = (ushort)codeWord;
            return true;
        }

        // track which bit gets written next
        private int bitIndex = 0;

        // Where bytes are stored
        private readonly List<byte> bitStream;

        /// <summary>
        /// Obtain data stream
        /// </summary>
        public List<byte> BitStream
        {
            get { return bitStream; }
        }
    }
#endif
}
