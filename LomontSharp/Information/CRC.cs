using System;

namespace Lomont.Information
{
    /// <summary>
    /// A general CRC, handles sizes 8 - 64 bits
    /// TODO - use INumeric later to make nicer general size version
    /// </summary>
    public class CRC
    {
        #region Simple class
        public CRC(
                int bitWidth,
                ulong normalPolynomial,
                ulong initialValue,
                bool reflectInput,
                bool reflectOutput,
                ulong finalXOR
            )
        {
            this.InitialValue = initialValue;
            this.FinalXor = finalXOR;
            table = MakeCRCTable(bitWidth, normalPolynomial, reflectInput);
            this.reflectOutput = reflectOutput;
            this.reflectInput = reflectInput;
            this.bitWidth = bitWidth;
        }

        public CRC(CRCDefinition definition)
        :this(definition.BitWidth, definition.NormalPolynomial, definition.InitialValue, definition.ReflectInput, definition.ReflectOutput, definition.FinalXOR)
        {

        }


        bool reflectInput;
        bool reflectOutput;
        int bitWidth;

        public ulong[] table;

        public ulong crc =0,InitialValue,FinalXor;

        
        /// <summary>
        /// For incremental CRC, call Start to init, then Add until all data added,
        /// then Finish to get final CRC
        /// </summary>
        public void Start()
        {
            crc = InitialValue;
        }

        public void Add(byte datum)
        {
            switch (reflectInput, reflectOutput)
            {
                case (false, false):
                    crc = (crc << 8) ^ table[((crc >> 24) ^ datum) & 0xff];
                    break;
                case (true, true):
                    crc = (crc >> 8) ^ table[(crc ^ datum) & 0xff];
                    break;
                default:
                    // todo - implement other versions if needed
                    throw new NotImplementedException("");
            }
        }


        public void Add(ReadOnlySpan<byte> buffer)
        {
            switch (reflectInput, reflectOutput)
            {
                case (false, false):
                    foreach (var b in buffer)
                        crc = (crc << 8) ^ table[((crc >> 24) ^ b) & 0xff];
                    break;
                case (true, true):
                    foreach (var b in buffer)
                        crc = (crc >> 8) ^ table[(crc ^ b) & 0xff];
                    break;
                default:
                    // todo - implement other versions if needed
                    throw new NotImplementedException("");
            }
        }

        public ulong Finish() => crc ^ FinalXor;

        /// <summary>
        /// Do entire block at once. 
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public ulong Hash(ReadOnlySpan<byte> buffer)
        {
            return CRC64ByTable(buffer, bitWidth, InitialValue, FinalXor, reflectInput, reflectOutput, table);
        }

        #endregion

        #region Tools
        /// <summary>
        /// Make table for parsing using the normal or reflected algorithms
        /// Supports bit sizes 8 (9?) to 64
        /// TODO - extend lower bits?
        /// </summary>
        /// <param name="bitWidth"></param>
        /// <param name="normalPolynomial"></param>
        /// <param name="reflectInput"></param>
        /// <returns></returns>
        public static ulong[] MakeCRCTable(
            int bitWidth,
            ulong normalPolynomial,
            bool reflectInput
        )
        {
            ulong[] table = new ulong[256];

            // make mask of 1's, without overflowing item
            ulong mask = (((1UL << (bitWidth - 1)) - 1UL) << 1) | 1UL;
            ulong topbit = 1UL << (bitWidth - 1);

            // for each input byte, get a table entry
            for (var b = 0U; b < 256; ++b)
            {
                ulong remainder = ((reflectInput) ? Reflect(b, 8) : b) << (bitWidth - 8);
                // mod 2 division, one bit at a time
                for (var i = 0; i < 8; ++i)
                {
                    if ((remainder & topbit) != 0)
                        remainder = (remainder << 1) ^ normalPolynomial;
                    else
                        remainder <<= 1;
                }

                if (reflectInput) remainder = Reflect(remainder, bitWidth);

                table[b] = remainder & mask;
            }

            return table;

        }
        #endregion

        #region CRC definitions

        /// <summary>
        /// CRC definition
        /// </summary>
        public class CRCDefinition
        {
            public CRCDefinition(
                string name, 
                int bitWidth, 
                ulong poly, 
                ulong init, 
                bool reflectInput, 
                bool reflectOutput,
                ulong xor, 
                ulong? check,
                params string[] alternateNames)
            {
                Name = name;
                BitWidth = bitWidth;
                NormalPolynomial = poly;
                InitialValue = init;
                ReflectInput = reflectInput;
                ReflectOutput = reflectOutput;
                FinalXOR = xor;
                AlternateNames = alternateNames;
                Check = check;
            }

            public CRCDefinition(string name, int bitWidth, ulong poly, ulong init, bool reflectInput, bool reflectOutput, ulong xor, params string [] alternateNames)
            :this(name, bitWidth, poly, init, reflectInput, reflectOutput, xor, null, alternateNames)
            {
            }

            public string Name;
            public int BitWidth; // can read from polynomial?
            public ulong NormalPolynomial; // handles up to 64 bit polys. Leading 1 not present
            public ulong InitialValue;
            public ulong FinalXOR;
            public bool ReflectInput; // reflect each byte first
            public bool ReflectOutput;
            // result when ASCII "123456789" is fed through algorithm
            // little endian
            public ulong? Check; 
            public string[] AlternateNames;

            // todo - add these
            // example: CRC-32 normal is 0x04C11DB7, Reversed 0xEDB88320, Reversed Reciprocal 0x82608EDB
            // also need checksums for little and big endian
            enum GenType
            {
                Normal,     // high order bit shifted off high end 
                Reversed,   // reversed, including high order bit, thus low order bit shifted off high end
                Reciprocal, // low order bit shifted off low end (always 1)
                ReversedReciprocal, // Reciprocal, reversed as above
            }

        }

        // tables from
        // https://pidlaboratory.com/9-rozne-algorytmy-crc/
        // https://www.scadacore.com/tools/programming-calculators/online-checksum-calculator/
        // https://reveng.sourceforge.io/crc-catalogue/all.htm


        public static CRCDefinition[] CRCDefs =
        {

#if true
/*
 
 Algo           bits  poly   init     refin  refout   xor ,   check 
 


 */

            new("CRC-8", 8, 0xA7, 0x00, true, true, 0x00, 0x26, "Bluetooth"),

            new("CRC-8"             ,8   ,0x07  ,0x00   ,false  ,false  ,0x00 , 0xF4),
            new("CRC-8/CDMA2000"    ,8   ,0x9B  ,0xFF   ,false  ,false  ,0x00 , 0xDA),
            new("CRC-8/DARC"        ,8   ,0x39  ,0x00   ,true   ,true   ,0x00 , 0x15),
            new("CRC-8/DVB-S2"      ,8   ,0xD5  ,0x00   ,false  ,false  ,0x00 , 0xBC),
            new("CRC-8/EBU"         ,8   ,0x1D  ,0xFF   ,true   ,true   ,0x00 , 0x97),
            new("CRC-8/I-CODE"      ,8   ,0x1D  ,0xFD   ,false  ,false  ,0x00 , 0x7E),
            new("CRC-8/ITU"         ,8   ,0x07  ,0x00   ,false  ,false  ,0x55 , 0xA1),
            new("CRC-8/MAXIM"       ,8   ,0x31  ,0x00   ,true   ,true   ,0x00 , 0xA1),
            new("CRC-8/ROHC"        ,8   ,0x07  ,0xFF   ,true   ,true   ,0x00 , 0xD0),
            new("CRC-8/WCDMA"       ,8   ,0x9B  ,0x00   ,true   ,true   ,0x00 , 0x25),

            new("CRC-15"       ,15   ,0x4599  ,0x00   ,false   ,false,0x00 , 0x59e),

            new("CRC-16/ARC", 16, 0x8005, 0x0000, true, true, 0x0000, 0xBB3D, "ARC", "CRC-16", "CRC-16/LHA", "CRC-IBM"),
            new("CRC-16/CDMA2000", 16, 0xc867, 0xffff, false, false, 0x0000, 0x4c06),
            // wireless M-Bus protocol
            new("CRC-16/EN-13757", 16, 0x3d65, 0, false, false, 0xffff, 0xc2b7),

            // RFID tags
            new("CRC-16/GENIBUS", 16, 0x1021, 0xffff, false, false, 0xffff, 0xd64e),

            // GSM network tags
            new("CRC-16/GSM", 16, 0x1021, 0x0000, false, false, 0xffff, 0xce3c),

            // contactless ID cards
            new("CRC-16/ISO-IEC-14443-3-A", 16, 0x1021, 0xc6c6, true, true, 0x0000, 0xbf05),

            // 
            new("CRC-16/KERMIT", 16, 0x1021, 0x0000, true, true, 0x0000, 0x2189),


            new("CRC-16", 16, 0x8005, 0x0000, true, true, 0x0000, 0xBB3D, "ARC", "MODBUS", "IBM"),

            new("CRC-16/CCITT", 16, 0x1021, 0xFFFF, false, false, 0x0000, 0x29B1, "X25", "ADCCP", "SDLC/HDLC",
                "XMODEM"),
            new("CRC-16/XMODEM", 16, 0x1021, 0x0000, false, false, 0x0000, 0x31C3),

            new("CRC-16/IBM", 16, 0x8005, 0xFFFF, false, false, 0x0000, 0xAEE7, "X25", "ADCCP", "SDLC/HDLC"),

            // Philips 37PF9731 LCD TV
            new("CRC-31/Phillips", 31, 0x04c11db7, 0x7fffffff, false, false, 0x7fffffff, 0x0ce9e46c),

            new("CRC-32", 32, 0x04C11DB7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xCBF43926,
                "ISO 3309", "ANSI X3.66", "FIPS PUB 71", "FED-STD-1003", "ITU-T V.42",
                "Ethernet", "SATA", "MPEG-2", "Gzip", "PKZIP", "POSIX cksum", "PNG",
                "ZMODEM", "AUTODIN II", "FDDI"),

            new("CRC-32C", 32, 0x1EDC6F41, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0xE3069283,
                "Castagnoli", "iSCSI & SCTP", "G.hn payload", "SSE4.2"
            ),

            new("CRC-32D", 32, 0xA833982B, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0x87315576),

            new("CRC-32K", 32, 0x741B8CD7, 0xFFFFFFFF, true, true, 0xFFFFFFFF, 0x2D3DD0AE /*0xE4C03E42*/),

            new("CRC-32Q", 32, 0x814141AB, 0x00000000, false, false, 0x00000000, 0x3010BF7F, "AXIM"),

            new("CRC-32/POSIX", 32, 0x04C11DB7, 0x00000000, false, false, 0xFFFFFFFF, 0x765E7680),


            new("CRC-40/GSM", 40, 0x0004820009, 0, false, false, 0xffffffffffUL, 0xd4164fc646UL, "GSM"),

            // DLT-1 tape cartridges
            new("CRC-64/ECMA-182",64,0x000000000000001BUL,0,false,false,0,0xE4FFBEA588933790UL),

            new("CRC-64/jones",64,0xad93d23594c935a9UL,0xffffffffffffffffUL,true,true,0,0xcaa717168609f281UL),

            new("CRC-64/XZ",64,0x42f0e1eba9ea3693,0xffffffffffffffffUL,true,true,0xffffffffffffffffUL,0x995dc9bbdf1939faUL),

#endif
        };

#endregion

        #region CRC-32

        /// <summary>
        /// CRC32 used by Ethernet, the AUTODIN II polynomial
        ///   x^32 + x^26 + x^23 + x^22 + x^16 +
        ///   x^12 + x^11 + x^10 + x^8 + x^7 + x^5 + x^4 + x^2 + x^1 + 1
        /// poly 0xEDB88320 (reversed) normal (0x04C11DB7)
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint CRC_32(ReadOnlySpan<byte> buffer) => CRC32ByTable(buffer, 32, ~0U,~0U,true,true, crc32Table);

        /// <summary>
        /// CRC32K (Koopman)
        /// Poly 0x741B8CD7, reversed 0xEB31D82E
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static uint CRC_32K(ReadOnlySpan<byte> buffer) => CRC32ByTable(buffer, 32, 0U, 0U, false, false, crc32KTable);


        // region CRC-32 
        #endregion

        #region CRC-24


        /// <summary>
        /// CRC class used in GPS messages
        /// 
        /// CRC-24Q from Qualcomm, in RTCM104V3, and PGP 6.5.1.
        /// uses error poly
        /// x^24+ x^23+ x^18+ x^17+ x^14+ x^11+ x^10+ x^7+ x^6+ x^5+ x^4+ x^3+ x+1
        /// i.e., mask 0x1864CFB. Standard requires seed of 0, built in here
        ///
        /// Detects:
        /// 1. double bit errors in 24 bit word
        /// 2. odd number of errors
        /// 3. Any burst less than or equal to 24 bits in length
        /// 4. larger bursts with false positive rate less than or equal to 2^-23
        /// </summary>
        /// <returns></returns>
        public static uint CRC_24Q(ReadOnlySpan<byte> buffer) => CRC32ByTable(buffer, 24, 0U, 0U, true, true, Crc24QTable) & 0x00_FF_FFFF;

        // region CRC24
        #endregion

        #region CRC smaller than 24
        
        /// <summary>
        /// 16 bit CRC, poly 1021
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static ushort CRC16(ReadOnlySpan<byte> buffer) =>
            (ushort)(CRC32ByTable(buffer, 16, 0xFFFF, 0xFFFF, false, false, crcTable16_1021));

        /// <summary>
        /// 12 bit CRC, poly B41
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static ushort CRC12(ReadOnlySpan<byte> buffer) => 
            (ushort)(CRC32ByTable(buffer, 12, 0xFFF, 0, false, false, crcTable12_0B41));

        // CRCs smaller than 24
        #endregion

        #region Tables

        public static readonly uint[] Crc24QTable =
        {
            0x00000000u, 0x01864CFBu, 0x028AD50Du, 0x030C99F6u, 0x0493E6E1u, 0x0515AA1Au, 0x061933ECu, 0x079F7F17u,
            0x08A18139u, 0x0927CDC2u, 0x0A2B5434u, 0x0BAD18CFu, 0x0C3267D8u, 0x0DB42B23u, 0x0EB8B2D5u, 0x0F3EFE2Eu,
            0x10C54E89u, 0x11430272u, 0x124F9B84u, 0x13C9D77Fu, 0x1456A868u, 0x15D0E493u, 0x16DC7D65u, 0x175A319Eu,
            0x1864CFB0u, 0x19E2834Bu, 0x1AEE1ABDu, 0x1B685646u, 0x1CF72951u, 0x1D7165AAu, 0x1E7DFC5Cu, 0x1FFBB0A7u,
            0x200CD1E9u, 0x218A9D12u, 0x228604E4u, 0x2300481Fu, 0x249F3708u, 0x25197BF3u, 0x2615E205u, 0x2793AEFEu,
            0x28AD50D0u, 0x292B1C2Bu, 0x2A2785DDu, 0x2BA1C926u, 0x2C3EB631u, 0x2DB8FACAu, 0x2EB4633Cu, 0x2F322FC7u,
            0x30C99F60u, 0x314FD39Bu, 0x32434A6Du, 0x33C50696u, 0x345A7981u, 0x35DC357Au, 0x36D0AC8Cu, 0x3756E077u,
            0x38681E59u, 0x39EE52A2u, 0x3AE2CB54u, 0x3B6487AFu, 0x3CFBF8B8u, 0x3D7DB443u, 0x3E712DB5u, 0x3FF7614Eu,
            0x4019A3D2u, 0x419FEF29u, 0x429376DFu, 0x43153A24u, 0x448A4533u, 0x450C09C8u, 0x4600903Eu, 0x4786DCC5u,
            0x48B822EBu, 0x493E6E10u, 0x4A32F7E6u, 0x4BB4BB1Du, 0x4C2BC40Au, 0x4DAD88F1u, 0x4EA11107u, 0x4F275DFCu,
            0x50DCED5Bu, 0x515AA1A0u, 0x52563856u, 0x53D074ADu, 0x544F0BBAu, 0x55C94741u, 0x56C5DEB7u, 0x5743924Cu,
            0x587D6C62u, 0x59FB2099u, 0x5AF7B96Fu, 0x5B71F594u, 0x5CEE8A83u, 0x5D68C678u, 0x5E645F8Eu, 0x5FE21375u,
            0x6015723Bu, 0x61933EC0u, 0x629FA736u, 0x6319EBCDu, 0x648694DAu, 0x6500D821u, 0x660C41D7u, 0x678A0D2Cu,
            0x68B4F302u, 0x6932BFF9u, 0x6A3E260Fu, 0x6BB86AF4u, 0x6C2715E3u, 0x6DA15918u, 0x6EADC0EEu, 0x6F2B8C15u,
            0x70D03CB2u, 0x71567049u, 0x725AE9BFu, 0x73DCA544u, 0x7443DA53u, 0x75C596A8u, 0x76C90F5Eu, 0x774F43A5u,
            0x7871BD8Bu, 0x79F7F170u, 0x7AFB6886u, 0x7B7D247Du, 0x7CE25B6Au, 0x7D641791u, 0x7E688E67u, 0x7FEEC29Cu,
            0x803347A4u, 0x81B50B5Fu, 0x82B992A9u, 0x833FDE52u, 0x84A0A145u, 0x8526EDBEu, 0x862A7448u, 0x87AC38B3u,
            0x8892C69Du, 0x89148A66u, 0x8A181390u, 0x8B9E5F6Bu, 0x8C01207Cu, 0x8D876C87u, 0x8E8BF571u, 0x8F0DB98Au,
            0x90F6092Du, 0x917045D6u, 0x927CDC20u, 0x93FA90DBu, 0x9465EFCCu, 0x95E3A337u, 0x96EF3AC1u, 0x9769763Au,
            0x98578814u, 0x99D1C4EFu, 0x9ADD5D19u, 0x9B5B11E2u, 0x9CC46EF5u, 0x9D42220Eu, 0x9E4EBBF8u, 0x9FC8F703u,
            0xA03F964Du, 0xA1B9DAB6u, 0xA2B54340u, 0xA3330FBBu, 0xA4AC70ACu, 0xA52A3C57u, 0xA626A5A1u, 0xA7A0E95Au,
            0xA89E1774u, 0xA9185B8Fu, 0xAA14C279u, 0xAB928E82u, 0xAC0DF195u, 0xAD8BBD6Eu, 0xAE872498u, 0xAF016863u,
            0xB0FAD8C4u, 0xB17C943Fu, 0xB2700DC9u, 0xB3F64132u, 0xB4693E25u, 0xB5EF72DEu, 0xB6E3EB28u, 0xB765A7D3u,
            0xB85B59FDu, 0xB9DD1506u, 0xBAD18CF0u, 0xBB57C00Bu, 0xBCC8BF1Cu, 0xBD4EF3E7u, 0xBE426A11u, 0xBFC426EAu,
            0xC02AE476u, 0xC1ACA88Du, 0xC2A0317Bu, 0xC3267D80u, 0xC4B90297u, 0xC53F4E6Cu, 0xC633D79Au, 0xC7B59B61u,
            0xC88B654Fu, 0xC90D29B4u, 0xCA01B042u, 0xCB87FCB9u, 0xCC1883AEu, 0xCD9ECF55u, 0xCE9256A3u, 0xCF141A58u,
            0xD0EFAAFFu, 0xD169E604u, 0xD2657FF2u, 0xD3E33309u, 0xD47C4C1Eu, 0xD5FA00E5u, 0xD6F69913u, 0xD770D5E8u,
            0xD84E2BC6u, 0xD9C8673Du, 0xDAC4FECBu, 0xDB42B230u, 0xDCDDCD27u, 0xDD5B81DCu, 0xDE57182Au, 0xDFD154D1u,
            0xE026359Fu, 0xE1A07964u, 0xE2ACE092u, 0xE32AAC69u, 0xE4B5D37Eu, 0xE5339F85u, 0xE63F0673u, 0xE7B94A88u,
            0xE887B4A6u, 0xE901F85Du, 0xEA0D61ABu, 0xEB8B2D50u, 0xEC145247u, 0xED921EBCu, 0xEE9E874Au, 0xEF18CBB1u,
            0xF0E37B16u, 0xF16537EDu, 0xF269AE1Bu, 0xF3EFE2E0u, 0xF4709DF7u, 0xF5F6D10Cu, 0xF6FA48FAu, 0xF77C0401u,
            0xF842FA2Fu, 0xF9C4B6D4u, 0xFAC82F22u, 0xFB4E63D9u, 0xFCD11CCEu, 0xFD575035u, 0xFE5BC9C3u, 0xFFDD8538u
        };


        /* 
        CRC len, hex polynomial, result of ASCII "0123456789" without quotes (init vector all 1's, i.e. 0xFFFF..)
        16,  1021, 29B1
        12,   B41,  90B
        */
        public static uint[] crcTable12_0B41 = new uint[256]
        {
            // CRC12 polynomial 0xB41
            0x0000, 0x0B41, 0x0DC3, 0x0682, 0x00C7, 0x0B86, 0x0D04, 0x0645, 0x018E, 0x0ACF, 0x0C4D, 0x070C, 0x0149, 0x0A08, 0x0C8A, 0x07CB,
            0x031C, 0x085D, 0x0EDF, 0x059E, 0x03DB, 0x089A, 0x0E18, 0x0559, 0x0292, 0x09D3, 0x0F51, 0x0410, 0x0255, 0x0914, 0x0F96, 0x04D7,
            0x0638, 0x0D79, 0x0BFB, 0x00BA, 0x06FF, 0x0DBE, 0x0B3C, 0x007D, 0x07B6, 0x0CF7, 0x0A75, 0x0134, 0x0771, 0x0C30, 0x0AB2, 0x01F3,
            0x0524, 0x0E65, 0x08E7, 0x03A6, 0x05E3, 0x0EA2, 0x0820, 0x0361, 0x04AA, 0x0FEB, 0x0969, 0x0228, 0x046D, 0x0F2C, 0x09AE, 0x02EF,
            0x0C70, 0x0731, 0x01B3, 0x0AF2, 0x0CB7, 0x07F6, 0x0174, 0x0A35, 0x0DFE, 0x06BF, 0x003D, 0x0B7C, 0x0D39, 0x0678, 0x00FA, 0x0BBB,
            0x0F6C, 0x042D, 0x02AF, 0x09EE, 0x0FAB, 0x04EA, 0x0268, 0x0929, 0x0EE2, 0x05A3, 0x0321, 0x0860, 0x0E25, 0x0564, 0x03E6, 0x08A7,
            0x0A48, 0x0109, 0x078B, 0x0CCA, 0x0A8F, 0x01CE, 0x074C, 0x0C0D, 0x0BC6, 0x0087, 0x0605, 0x0D44, 0x0B01, 0x0040, 0x06C2, 0x0D83,
            0x0954, 0x0215, 0x0497, 0x0FD6, 0x0993, 0x02D2, 0x0450, 0x0F11, 0x08DA, 0x039B, 0x0519, 0x0E58, 0x081D, 0x035C, 0x05DE, 0x0E9F,
            0x03A1, 0x08E0, 0x0E62, 0x0523, 0x0366, 0x0827, 0x0EA5, 0x05E4, 0x022F, 0x096E, 0x0FEC, 0x04AD, 0x02E8, 0x09A9, 0x0F2B, 0x046A,
            0x00BD, 0x0BFC, 0x0D7E, 0x063F, 0x007A, 0x0B3B, 0x0DB9, 0x06F8, 0x0133, 0x0A72, 0x0CF0, 0x07B1, 0x01F4, 0x0AB5, 0x0C37, 0x0776,
            0x0599, 0x0ED8, 0x085A, 0x031B, 0x055E, 0x0E1F, 0x089D, 0x03DC, 0x0417, 0x0F56, 0x09D4, 0x0295, 0x04D0, 0x0F91, 0x0913, 0x0252,
            0x0685, 0x0DC4, 0x0B46, 0x0007, 0x0642, 0x0D03, 0x0B81, 0x00C0, 0x070B, 0x0C4A, 0x0AC8, 0x0189, 0x07CC, 0x0C8D, 0x0A0F, 0x014E,
            0x0FD1, 0x0490, 0x0212, 0x0953, 0x0F16, 0x0457, 0x02D5, 0x0994, 0x0E5F, 0x051E, 0x039C, 0x08DD, 0x0E98, 0x05D9, 0x035B, 0x081A,
            0x0CCD, 0x078C, 0x010E, 0x0A4F, 0x0C0A, 0x074B, 0x01C9, 0x0A88, 0x0D43, 0x0602, 0x0080, 0x0BC1, 0x0D84, 0x06C5, 0x0047, 0x0B06,
            0x09E9, 0x02A8, 0x042A, 0x0F6B, 0x092E, 0x026F, 0x04ED, 0x0FAC, 0x0867, 0x0326, 0x05A4, 0x0EE5, 0x08A0, 0x03E1, 0x0563, 0x0E22,
            0x0AF5, 0x01B4, 0x0736, 0x0C77, 0x0A32, 0x0173, 0x07F1, 0x0CB0, 0x0B7B, 0x003A, 0x06B8, 0x0DF9, 0x0BBC, 0x00FD, 0x067F, 0x0D3E
        };

        public static uint[] crcTable16_1021 = new uint[256]
        {
            // CRC16 polynomial 0x1021
            0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7, 0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C,
            0xD1AD, 0xE1CE, 0xF1EF,
            0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6, 0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD,
            0xC39C, 0xF3FF, 0xE3DE,
            0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485, 0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE,
            0xF5CF, 0xC5AC, 0xD58D,
            0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4, 0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF,
            0xE7FE, 0xD79D, 0xC7BC,
            0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823, 0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948,
            0x9969, 0xA90A, 0xB92B,
            0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12, 0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79,
            0x8B58, 0xBB3B, 0xAB1A,
            0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41, 0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A,
            0xBD0B, 0x8D68, 0x9D49,
            0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70, 0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B,
            0xAF3A, 0x9F59, 0x8F78,
            0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F, 0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004,
            0x4025, 0x7046, 0x6067,
            0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E, 0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235,
            0x5214, 0x6277, 0x7256,
            0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D, 0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466,
            0x6447, 0x5424, 0x4405,
            0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C, 0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657,
            0x7676, 0x4615, 0x5634,
            0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB, 0x5844, 0x4865, 0x7806, 0x6827, 0x18C0,
            0x08E1, 0x3882, 0x28A3,
            0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A, 0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1,
            0x1AD0, 0x2AB3, 0x3A92,
            0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9, 0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2,
            0x2C83, 0x1CE0, 0x0CC1,
            0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8, 0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93,
            0x3EB2, 0x0ED1, 0x1EF0,
        };

        public static uint[] crc32KTable = new uint[256] // poly 0x741B8CD7
        {
            0x00000000, 0x741B8CD7, 0xE83719AE, 0x9C2C9579, 0xA475BF8B, 0xD06E335C, 0x4C42A625, 0x38592AF2,
            0x3CF0F3C1, 0x48EB7F16, 0xD4C7EA6F, 0xA0DC66B8, 0x98854C4A, 0xEC9EC09D, 0x70B255E4, 0x04A9D933,
            0x79E1E782, 0x0DFA6B55, 0x91D6FE2C, 0xE5CD72FB, 0xDD945809, 0xA98FD4DE, 0x35A341A7, 0x41B8CD70,
            0x45111443, 0x310A9894, 0xAD260DED, 0xD93D813A, 0xE164ABC8, 0x957F271F, 0x0953B266, 0x7D483EB1,
            0xF3C3CF04, 0x87D843D3, 0x1BF4D6AA, 0x6FEF5A7D, 0x57B6708F, 0x23ADFC58, 0xBF816921, 0xCB9AE5F6,
            0xCF333CC5, 0xBB28B012, 0x2704256B, 0x531FA9BC, 0x6B46834E, 0x1F5D0F99, 0x83719AE0, 0xF76A1637,
            0x8A222886, 0xFE39A451, 0x62153128, 0x160EBDFF, 0x2E57970D, 0x5A4C1BDA, 0xC6608EA3, 0xB27B0274,
            0xB6D2DB47, 0xC2C95790, 0x5EE5C2E9, 0x2AFE4E3E, 0x12A764CC, 0x66BCE81B, 0xFA907D62, 0x8E8BF1B5,
            0x939C12DF, 0xE7879E08, 0x7BAB0B71, 0x0FB087A6, 0x37E9AD54, 0x43F22183, 0xDFDEB4FA, 0xABC5382D,
            0xAF6CE11E, 0xDB776DC9, 0x475BF8B0, 0x33407467, 0x0B195E95, 0x7F02D242, 0xE32E473B, 0x9735CBEC,
            0xEA7DF55D, 0x9E66798A, 0x024AECF3, 0x76516024, 0x4E084AD6, 0x3A13C601, 0xA63F5378, 0xD224DFAF,
            0xD68D069C, 0xA2968A4B, 0x3EBA1F32, 0x4AA193E5, 0x72F8B917, 0x06E335C0, 0x9ACFA0B9, 0xEED42C6E,
            0x605FDDDB, 0x1444510C, 0x8868C475, 0xFC7348A2, 0xC42A6250, 0xB031EE87, 0x2C1D7BFE, 0x5806F729,
            0x5CAF2E1A, 0x28B4A2CD, 0xB49837B4, 0xC083BB63, 0xF8DA9191, 0x8CC11D46, 0x10ED883F, 0x64F604E8,
            0x19BE3A59, 0x6DA5B68E, 0xF18923F7, 0x8592AF20, 0xBDCB85D2, 0xC9D00905, 0x55FC9C7C, 0x21E710AB,
            0x254EC998, 0x5155454F, 0xCD79D036, 0xB9625CE1, 0x813B7613, 0xF520FAC4, 0x690C6FBD, 0x1D17E36A,
            0x5323A969, 0x273825BE, 0xBB14B0C7, 0xCF0F3C10, 0xF75616E2, 0x834D9A35, 0x1F610F4C, 0x6B7A839B,
            0x6FD35AA8, 0x1BC8D67F, 0x87E44306, 0xF3FFCFD1, 0xCBA6E523, 0xBFBD69F4, 0x2391FC8D, 0x578A705A,
            0x2AC24EEB, 0x5ED9C23C, 0xC2F55745, 0xB6EEDB92, 0x8EB7F160, 0xFAAC7DB7, 0x6680E8CE, 0x129B6419,
            0x1632BD2A, 0x622931FD, 0xFE05A484, 0x8A1E2853, 0xB24702A1, 0xC65C8E76, 0x5A701B0F, 0x2E6B97D8,
            0xA0E0666D, 0xD4FBEABA, 0x48D77FC3, 0x3CCCF314, 0x0495D9E6, 0x708E5531, 0xECA2C048, 0x98B94C9F,
            0x9C1095AC, 0xE80B197B, 0x74278C02, 0x003C00D5, 0x38652A27, 0x4C7EA6F0, 0xD0523389, 0xA449BF5E,
            0xD90181EF, 0xAD1A0D38, 0x31369841, 0x452D1496, 0x7D743E64, 0x096FB2B3, 0x954327CA, 0xE158AB1D,
            0xE5F1722E, 0x91EAFEF9, 0x0DC66B80, 0x79DDE757, 0x4184CDA5, 0x359F4172, 0xA9B3D40B, 0xDDA858DC,
            0xC0BFBBB6, 0xB4A43761, 0x2888A218, 0x5C932ECF, 0x64CA043D, 0x10D188EA, 0x8CFD1D93, 0xF8E69144,
            0xFC4F4877, 0x8854C4A0, 0x147851D9, 0x6063DD0E, 0x583AF7FC, 0x2C217B2B, 0xB00DEE52, 0xC4166285,
            0xB95E5C34, 0xCD45D0E3, 0x5169459A, 0x2572C94D, 0x1D2BE3BF, 0x69306F68, 0xF51CFA11, 0x810776C6,
            0x85AEAFF5, 0xF1B52322, 0x6D99B65B, 0x19823A8C, 0x21DB107E, 0x55C09CA9, 0xC9EC09D0, 0xBDF78507,
            0x337C74B2, 0x4767F865, 0xDB4B6D1C, 0xAF50E1CB, 0x9709CB39, 0xE31247EE, 0x7F3ED297, 0x0B255E40,
            0x0F8C8773, 0x7B970BA4, 0xE7BB9EDD, 0x93A0120A, 0xABF938F8, 0xDFE2B42F, 0x43CE2156, 0x37D5AD81,
            0x4A9D9330, 0x3E861FE7, 0xA2AA8A9E, 0xD6B10649, 0xEEE82CBB, 0x9AF3A06C, 0x06DF3515, 0x72C4B9C2,
            0x766D60F1, 0x0276EC26, 0x9E5A795F, 0xEA41F588, 0xD218DF7A, 0xA60353AD, 0x3A2FC6D4, 0x4E344A03
        };

        public static uint[] crc32Table = new uint[256]
        {
            0x00000000, 0x77073096, 0xee0e612c, 0x990951ba, 0x076dc419, 0x706af48f, 0xe963a535, 0x9e6495a3,
            0x0edb8832, 0x79dcb8a4, 0xe0d5e91e, 0x97d2d988, 0x09b64c2b, 0x7eb17cbd, 0xe7b82d07, 0x90bf1d91,
            0x1db71064, 0x6ab020f2, 0xf3b97148, 0x84be41de, 0x1adad47d, 0x6ddde4eb, 0xf4d4b551, 0x83d385c7,
            0x136c9856, 0x646ba8c0, 0xfd62f97a, 0x8a65c9ec, 0x14015c4f, 0x63066cd9, 0xfa0f3d63, 0x8d080df5,
            0x3b6e20c8, 0x4c69105e, 0xd56041e4, 0xa2677172, 0x3c03e4d1, 0x4b04d447, 0xd20d85fd, 0xa50ab56b,
            0x35b5a8fa, 0x42b2986c, 0xdbbbc9d6, 0xacbcf940, 0x32d86ce3, 0x45df5c75, 0xdcd60dcf, 0xabd13d59,
            0x26d930ac, 0x51de003a, 0xc8d75180, 0xbfd06116, 0x21b4f4b5, 0x56b3c423, 0xcfba9599, 0xb8bda50f,
            0x2802b89e, 0x5f058808, 0xc60cd9b2, 0xb10be924, 0x2f6f7c87, 0x58684c11, 0xc1611dab, 0xb6662d3d,
            0x76dc4190, 0x01db7106, 0x98d220bc, 0xefd5102a, 0x71b18589, 0x06b6b51f, 0x9fbfe4a5, 0xe8b8d433,
            0x7807c9a2, 0x0f00f934, 0x9609a88e, 0xe10e9818, 0x7f6a0dbb, 0x086d3d2d, 0x91646c97, 0xe6635c01,
            0x6b6b51f4, 0x1c6c6162, 0x856530d8, 0xf262004e, 0x6c0695ed, 0x1b01a57b, 0x8208f4c1, 0xf50fc457,
            0x65b0d9c6, 0x12b7e950, 0x8bbeb8ea, 0xfcb9887c, 0x62dd1ddf, 0x15da2d49, 0x8cd37cf3, 0xfbd44c65,
            0x4db26158, 0x3ab551ce, 0xa3bc0074, 0xd4bb30e2, 0x4adfa541, 0x3dd895d7, 0xa4d1c46d, 0xd3d6f4fb,
            0x4369e96a, 0x346ed9fc, 0xad678846, 0xda60b8d0, 0x44042d73, 0x33031de5, 0xaa0a4c5f, 0xdd0d7cc9,
            0x5005713c, 0x270241aa, 0xbe0b1010, 0xc90c2086, 0x5768b525, 0x206f85b3, 0xb966d409, 0xce61e49f,
            0x5edef90e, 0x29d9c998, 0xb0d09822, 0xc7d7a8b4, 0x59b33d17, 0x2eb40d81, 0xb7bd5c3b, 0xc0ba6cad,
            0xedb88320, 0x9abfb3b6, 0x03b6e20c, 0x74b1d29a, 0xead54739, 0x9dd277af, 0x04db2615, 0x73dc1683,
            0xe3630b12, 0x94643b84, 0x0d6d6a3e, 0x7a6a5aa8, 0xe40ecf0b, 0x9309ff9d, 0x0a00ae27, 0x7d079eb1,
            0xf00f9344, 0x8708a3d2, 0x1e01f268, 0x6906c2fe, 0xf762575d, 0x806567cb, 0x196c3671, 0x6e6b06e7,
            0xfed41b76, 0x89d32be0, 0x10da7a5a, 0x67dd4acc, 0xf9b9df6f, 0x8ebeeff9, 0x17b7be43, 0x60b08ed5,
            0xd6d6a3e8, 0xa1d1937e, 0x38d8c2c4, 0x4fdff252, 0xd1bb67f1, 0xa6bc5767, 0x3fb506dd, 0x48b2364b,
            0xd80d2bda, 0xaf0a1b4c, 0x36034af6, 0x41047a60, 0xdf60efc3, 0xa867df55, 0x316e8eef, 0x4669be79,
            0xcb61b38c, 0xbc66831a, 0x256fd2a0, 0x5268e236, 0xcc0c7795, 0xbb0b4703, 0x220216b9, 0x5505262f,
            0xc5ba3bbe, 0xb2bd0b28, 0x2bb45a92, 0x5cb36a04, 0xc2d7ffa7, 0xb5d0cf31, 0x2cd99e8b, 0x5bdeae1d,
            0x9b64c2b0, 0xec63f226, 0x756aa39c, 0x026d930a, 0x9c0906a9, 0xeb0e363f, 0x72076785, 0x05005713,
            0x95bf4a82, 0xe2b87a14, 0x7bb12bae, 0x0cb61b38, 0x92d28e9b, 0xe5d5be0d, 0x7cdcefb7, 0x0bdbdf21,
            0x86d3d2d4, 0xf1d4e242, 0x68ddb3f8, 0x1fda836e, 0x81be16cd, 0xf6b9265b, 0x6fb077e1, 0x18b74777,
            0x88085ae6, 0xff0f6a70, 0x66063bca, 0x11010b5c, 0x8f659eff, 0xf862ae69, 0x616bffd3, 0x166ccf45,
            0xa00ae278, 0xd70dd2ee, 0x4e048354, 0x3903b3c2, 0xa7672661, 0xd06016f7, 0x4969474d, 0x3e6e77db,
            0xaed16a4a, 0xd9d65adc, 0x40df0b66, 0x37d83bf0, 0xa9bcae53, 0xdebb9ec5, 0x47b2cf7f, 0x30b5ffe9,
            0xbdbdf21c, 0xcabac28a, 0x53b39330, 0x24b4a3a6, 0xbad03605, 0xcdd70693, 0x54de5729, 0x23d967bf,
            0xb3667a2e, 0xc4614ab8, 0x5d681b02, 0x2a6f2b94, 0xb40bbe37, 0xc30c8ea1, 0x5a05df1b, 0x2d02ef8d
        };
        // Tables
        #endregion

        #region Implementation
        /// <summary>
        /// Do CRC from table. Does not pre or post invert CRC
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="crc"></param>
        /// <param name="reflectedOutput"></param>
        /// <param name="table"></param>
        /// <param name="crcInit"></param>
        /// <param name="crcXor"></param>
        /// <param name="reflectedInput"></param>
        /// <returns></returns>
        static uint CRC32ByTable(
            ReadOnlySpan<byte> buffer,
            int bitWidth,
            uint crcInit,
            uint crcXor,
            bool reflectedInput,
            bool reflectedOutput,
            uint[] table
            )
        {
            // bit mask, careful to not overflow the value
            uint mask = (((1U << (bitWidth - 1)) - 1) << 1) | 1;
            uint crc = crcInit;

            if (reflectedInput)
            {
                crc = (uint)Reflect(crcInit, bitWidth); // todo - store them reflected?
                foreach (var b in buffer)
                    crc = ((crc >> 8) ^ table[(crc ^ b) & 0xff]) & mask;
            }
            else
            {
                foreach (var b in buffer)
                    crc = ((crc << 8) ^ table[((crc >> (bitWidth - 8)) ^ b) & 0xff]) & mask;

            }


            if (reflectedInput ^ reflectedOutput)
                crc = (uint)Reflect(crc, bitWidth); // from Mark Adler, CRC-12/3GPP
            return (crc ^ crcXor) & mask;
        }

        /// <summary>
        /// Do CRC from table. Does not pre or post invert CRC
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="reflectedOutput"></param>
        /// <param name="table"></param>
        /// <param name="crcInit"></param>
        /// <param name="crcXor"></param>
        /// <param name="reflectedInput"></param>
        /// <returns></returns>
        static ulong CRC64ByTable(
            ReadOnlySpan<byte> buffer,
            int bitWidth,
            ulong crcInit,
            ulong crcXor,
            bool reflectedInput,
            bool reflectedOutput,
            ulong[] table
        )
        {
            // bit mask, careful to not overflow the value
            ulong mask = (((1UL << (bitWidth - 1)) - 1) << 1) | 1;
            ulong crc = crcInit;

            if (reflectedInput)
            {
                crc = Reflect(crcInit, bitWidth); // todo - store them reflected?
                foreach (var b in buffer)
                    crc = ((crc >> 8) ^ table[(crc ^ b) & 0xff]) & mask;
            }
            else
            {
                foreach (var b in buffer)
                    crc = ((crc << 8) ^ table[((crc >> (bitWidth - 8)) ^ b) & 0xff]) & mask;

            }


            if (reflectedInput ^ reflectedOutput)
                crc = Reflect(crc, bitWidth); // from Mark Adler, CRC-12/3GPP
            return (crc ^ crcXor) & mask;
        }




        // reverse bottom b [0,64] bits
        static ulong Reflect(ulong v, int b)
        {
            if (b == 0) return v;
            ulong testBit = 1UL << (b - 1);
            ulong t = v;
            for (var i = 0; i < b; ++i)
            {
                if ((t & 1L) != 0)
                    v |= testBit;
                else
                    v &= ~testBit;
                testBit >>= 1;
                t >>= 1;
            }
            return v;
        }
        #endregion

        #region Notes


        /* TODO - implement general version
         Example: https://pidlaboratory.com/9-rozne-algorytmy-crc/
         
        best CRC polys for various hamming distances by Koopman
        https://users.ece.cmu.edu/~koopman/crc/
         
        Ross Williams notes
        https://zlib.net/crc_v3.txt

        good testing place 
        http://www.zorc.breitbandkatze.de/crc.html
        https://www.scadacore.com/tools/programming-calculators/online-checksum-calculator/
        https://www.lammertbies.nl/comm/info/crc-calculation
        https://crccalc.com/

        /*
        For error detection, CRCs are often used, but most common ones are not optimal for short packets of
        the type used in embedded networks. Recent research has found optimal CRC polynomials for the uses
        we'll use. The webpage http://users.ece.cmu.edu/~koopman/crc/index.html seems most up to date.
    
        CRC Refs:
        "The Effectiveness of Checksums for Embedded Networks," Maxino, https://users.ece.cmu.edu/~koopman/thesis/maxino_ms.pdf
        "Cyclic Redundancy Code (CRC) Polynomial Selection For Embedded Networks," Koopman,  https://users.ece.cmu.edu/~koopman/roses/dsn04/koopman04_crc_poly_embedded.pdf
        "Best CRC polynomials, " Koopman, http://users.ece.cmu.edu/~koopman/crc/index.html
    
    
        Small table CRC
        https://wiki.wxwidgets.org/Development:_Small_Table_CRC
    
        (0xb41; 0x1683) <=> (0xc16; 0x182d) {1773,1773,27,27} | gold | (*o) CRC-12F/6.2 ("13203")
         0xb41 = x^12 +x^10 +x^9 +x^7 +x +1.

        see https://users.ece.cmu.edu/~koopman/crc/crc32.html

        http://users.ece.cmu.edu/~koopman/networks/dsn02/dsn02_koopman.pdf

        https://reveng.sourceforge.io/crc-catalogue/all.htm

        good tool - reverse engineering, testing
        https://reveng.sourceforge.io/

        common error for CRC-16/CCITT
        http://srecord.sourceforge.net/crc16-ccitt.html

        // various CCITT versions
        https://www.lammertbies.nl/comm/info/crc-calculation

        // some tests
        https://github.com/snksoft/crc/blob/master/crc_test.go

         */
        #endregion

    }
}
