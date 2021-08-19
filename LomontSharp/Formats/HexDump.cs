// Chris Lomont 2019
// console mode Hex Dump

using System;
using System.Collections.Generic;
using System.IO;

/*
TODO
    commands from xxd:
    -a | -autoskip
       toggle autoskip: a single '*' replaces nul-lines, default off
    -b | -bits
       switch to binary
    -c cols | -cols cols
    <cols> octets per line, default 16 (should default to fill window?)
    -e 
      switch to little endian
    -g bytes | -groupsize bytes
       group bytes togheter, default 2 normal, 4 in little endian, 1 in buts mode
    -h | -help
      this stuff
    -i | -include
       make C include file
    -o offset
      add to offet position
    -r | -revert
       reverse hexdump into a binary
    -seek offset
    -u
       use uppercase for numbers, default lowercase
    -v | -version
       show version string


    other ideas
    duplicate rows condensed into *, unless -v (verbose)
    - highlight pattern
    -C show chars too
    -n 32 = # of bytes to show
    -s offset where to start
    -e is a format string (see xxd, other tools)
    -c no colors
    -b no border items
    -x tagged for parsing
    -h to C header file
    -?? bounds start and finish
    - find
    - diff
    goto command
    - binary
    -version
    - little endian/bigendian

    editing like hecate?


    add block sizes, etc.

    add options for some types: floats, int, unsigned, decimal, 32, 16, 8, 64 bit...

    see also https://github.com/sharkdp/hexyl, xxd, https://github.com/evanmiller/hecate


 */

namespace Lomont.Formats
{
    class HexDump
    {
        static void Usage()
        {

        }
        static void Main2(string[] args)
        {
            if (args.Length != 1)
            {
                Usage();
                Environment.Exit(-1);
            }
            var dumper = new HexDumper();
            dumper.Dump(args[0]);
            Console.WriteLine("hexdump v0.1, Chris Lomont, 2019");
            Environment.Exit(0);
        }
    }

    public class HexDumper
    {
        void Write(string text)
        {
            Console.Write(text);
        }
        void WriteLine(string text = "")
        {
            Console.WriteLine(text);
        }

        public void Dump(Stream stream, int start = 0, int length = -1, bool interactive = true)
        {
            var color = Console.ForegroundColor;
            // wrap all code in here, to ensure cleanup
            using (BinaryReader reader = new BinaryReader(stream))
            {
                data = reader;
                position = start;
                dataLength = length <= 0 ? data.BaseStream.Length : position + length;
                positionChars = (int)((Math.Ceiling(Math.Log(dataLength, 2)) + 3) / 4);
                positionFormat = $"{{0:X{positionChars}}}";
                DumpHelper(interactive);
            }

            Console.ForegroundColor = color; // restore
        }

        public void Dump(byte [] data, int start = 0, int length = -1, bool interactive = true)
        {
            var stream = new MemoryStream(data);
            Dump(stream, start, length, interactive);
        }

        public void Dump(string filename, int start = 0, int length = -1, bool interactive = true)
        {
            if (!File.Exists(filename))
            {
                WriteLine($"File {filename} does not exist.");
                return;
            }

            using var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            Dump(stream, start, length, interactive);
        }

        BinaryReader data;
        long dataLength;
        long position;
        string positionFormat;

        int bytesPerLine = 16; // bytes to read
        int bytesPerGroup = 8; // extra space this often
        int positionChars = 5;
        int linesPerPage = 20;

        void WriteHeaderFooter(char left, char t, char m, char r)
        {
            Console.ForegroundColor = outerBorderColor;
            Write(left.ToString());
            for (var i = 0; i < positionChars + 2; ++i)
                Write(m.ToString());
            Write(t.ToString());
            // bytes
            for (var i = 0; i < bytesPerLine; i += bytesPerGroup)
            {
                for (var j = 0; j < 3 * bytesPerGroup + 1; ++j)
                    Write(m.ToString());
                Write(t.ToString());
            }
            // chars
            for (var i = 0; i < bytesPerLine; i += bytesPerGroup)
            {
                for (var j = 0; j < bytesPerGroup + 2; ++j)
                    Write(m.ToString());
                if (i < bytesPerLine - bytesPerGroup)
                    Write(t.ToString());
            }
            WriteLine(r.ToString());
        }

        bool continuous = false;
        void DumpHelper(bool interactive)
        {
            continuous = !interactive;
            SizeScreen();
            WriteHeaderFooter(UL, UT, D, UR);
            while (position < dataLength)
            {
                SizeScreen();
                DumpPage(position == 0 ? linesPerPage - 2 : linesPerPage - 2);
                if (!continuous && !Command())
                    break;
                Console.CursorLeft = 0;
            }
            WriteHeaderFooter(LL, LT, D, LR);
        }

        bool Command()
        {
            Console.ForegroundColor = outerBorderColor;
            Write("SPACE or pgdn for next, ESC exit, pgup, c for continuous");
            while (true)
            {
                var k = Console.ReadKey(true);
                if (k.Key == ConsoleKey.PageDown || k.Key == ConsoleKey.Spacebar)
                {
                    return true;
                }

                if (k.Key == ConsoleKey.Escape)
                {
                    return false;
                }
                if (k.Key == ConsoleKey.C)
                {
                    continuous = true;
                    return true;
                }

                if (k.Key == ConsoleKey.PageUp)
                {
                    position -= 2 * (bytesPerLine * (linesPerPage - 2));
                    if (position < 0) position = 0;
                    data.BaseStream.Position = position;
                    WriteLine(".................");
                    return true;
                }
            }

            return true;
        }


        void SizeScreen()
        {
            // change screen sizing if possible
            //Console.WindowWidth;
            linesPerPage = Console.WindowHeight;
        }

        void DumpPage(int lines)
        {
            for (var i = 0; i < lines; ++i)
            {
                DumpLine();
                if (position >= dataLength)
                    return;
            }
        }

        static char UL = '┌';
        static char UR = '┐';
        static char UT = '┬';
        static char LL = '└';
        static char LR = '┘';
        static char LT = '┴';
        static char VS = '│';
        static char VD = '│'; // wanted dashed, or lighter?
        static char D = '─';
        static ConsoleColor outerBorderColor = ConsoleColor.White;
        static ConsoleColor innerBorderColor = ConsoleColor.DarkGray;
        static ConsoleColor positionColor = ConsoleColor.Gray;




        // https://en.wikipedia.org/wiki/Whitespace_character
        // https://en.wikipedia.org/wiki/ASCII#Character_groups
        enum ByteType
        {
            // check in this order
            Null,        // 0
            WhiteSpace,  // 0x20, 9,A,B,C,D
            Symbol,      // $+<=>^`|~ = 0x24,2B,3C,3D,3E,5E,60,7C,7E
            Control,     // 0x01-0x1F and 0x7F except above
            Punctuation, // 0x20-0x2F, 3A-40, 5B-5F, 7B-7D, except above
            Digit,       // 0x30-0x39
            Uppercase,   // 0x41-0x5A
            Lowercase,   // 0x61-0x7A
            NonASCII     // 0x80-0xFF
        }

        private static Dictionary<ByteType, ConsoleColor> colors = new Dictionary<ByteType, ConsoleColor>()
        {
            {ByteType.Null        , ConsoleColor.DarkBlue},
            {ByteType.Control     , ConsoleColor.DarkYellow},
            {ByteType.NonASCII    , ConsoleColor.DarkRed},

            {ByteType.WhiteSpace  , ConsoleColor.White},
            {ByteType.Symbol      , ConsoleColor.DarkGray},
            {ByteType.Punctuation , ConsoleColor.Gray},
            {ByteType.Digit       , ConsoleColor.Red},
            {ByteType.Uppercase   , ConsoleColor.Yellow},
            {ByteType.Lowercase   , ConsoleColor.Yellow},
        };

        static ByteType[] types = new ByteType[256];

        static HexDumper()
        {// fill in type table
            for (var i = 0; i < 256; ++i)
                types[i] = ByteType.Null; // blank them
            for (var i = 0x30; i <= 0x39; ++i)
                types[i] = ByteType.Digit;
            for (var i = 0x41; i <= 0x5A; ++i)
                types[i] = ByteType.Uppercase;
            for (var i = 0x61; i <= 0x7A; ++i)
                types[i] = ByteType.Lowercase;
            for (var i = 0x80; i <= 0xFF; ++i)
                types[i] = ByteType.NonASCII;

            foreach (var i in new int[] { 0x20, 9, 10, 11, 12, 13 })
                types[i] = ByteType.WhiteSpace;
            foreach (var i in new int[] { 0x24, 0x2B, 0x3C, 0x3D, 0x3E, 0x5E, 0x60, 0x60, 0x7C, 0x7E })
                types[i] = ByteType.Symbol;
            for (var i = 0x01; i <= 0x01F; ++i)
                if (types[i] == ByteType.Null)
                    types[i] = ByteType.Control;
            types[0x7F] = ByteType.Control;
            for (var i = 0x01; i <= 0x01F; ++i)
                if (types[i] == ByteType.Null)
                    types[i] = ByteType.Control;

            for (var i = 0x01; i <= 0x80; ++i)
                if (types[i] == ByteType.Null)
                    types[i] = ByteType.Punctuation;
        }

        ByteType Type(byte b)
        {
            return types[b];
        }

        void SetColor(byte b)
        {
            var t = Type(b);
            Console.ForegroundColor = colors[t];
        }

        bool IsPrintable(byte b)
        {
            return 0x20 <= b && b <= 0x7E;
        }
        void DumpLine()
        {
            Console.ForegroundColor = outerBorderColor;
            Write(VS + " ");
            Console.ForegroundColor = positionColor;
            Write(String.Format(positionFormat, position));
            Console.ForegroundColor = outerBorderColor;
            Write(" " + VS + " ");
            var bytes = Get(bytesPerLine);
            // hex bytes
            for (var i = 0; i < bytesPerLine; ++i)
            {
                if (i < bytes.Length)
                {
                    var b = bytes[i];
                    SetColor(b);
                    Write($"{bytes[i]:X2} ");
                }
                else
                {
                    Write("   ");
                }

                if (i == bytesPerLine - 1)
                {
                    Console.ForegroundColor = outerBorderColor;
                    Write(VD.ToString());

                }
                else if ((i % bytesPerGroup) == bytesPerGroup - 1)
                {
                    Console.ForegroundColor = innerBorderColor;
                    Write(VD + " ");
                }
            }
            // chars
            Write(" ");
            for (var i = 0; i < bytesPerLine; ++i)
            {
                if (i < bytes.Length)
                {
                    var b = bytes[i];
                    SetColor(b);
                    // non-printable 0-31
                    if (!IsPrintable(b))
                        Write(".");
                    else
                        Write(((char)bytes[i]).ToString());
                }
                else
                {
                    Write(" ");
                }

                if (i == bytesPerLine - 1)
                {
                    Console.ForegroundColor = outerBorderColor;
                    Write(" " + VD);
                }
                else if ((i % bytesPerGroup) == bytesPerGroup - 1)
                {
                    Console.ForegroundColor = innerBorderColor;
                    Write(" " + VD + " ");
                }
            }
            WriteLine();
        }

        byte[] Get(long length)
        {
            length = Math.Min(length, dataLength - position);
            var b = data.ReadBytes((int)length);
            position += length;
            return b;
        }

    }

}
