using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Formats
{
    /// <summary>
    /// Class to read WAV files and stream the data
    /// See http://www.sonicspot.com/guide/wavefiles.html for file spec
    /// </summary>
    public sealed class WavFormat
    {
        public WavFormat()
        {
            LeftChannel = new List<short>();
            RightChannel = new List<short>();
            CenterChannel = new List<short>();
        }

        internal struct WavHeader
        {
            public uint RiffID; // "RIFF" chunk (0x52494646)
            public uint Size; // file size - 8
            public uint WavID; // "WAVE" chunk (0x57415645)
            public uint FmtID; // "fmt " chunk (0x666D7420)
            public uint FmtSize; // format data size
            public ushort Format; // compression code 1-65535
            public ushort Channels; // number of channels 1-65535
            public uint SampleRate; // sample rate 1-0xFFFFFFFF
            public uint BytePerSec; // bytes per second
            public ushort BlockSize; // block alignment
            public ushort BitsPerChannel; // bits per sample 2-65535
            public uint DataID; // "data" chunk (0x64617461)
            public uint DataSize; //
        }


        internal WavHeader Header;

        private const uint RiffId = 0x46464952;
        private const uint WavId = 0x45564157;
        private const uint FmtId = 0x20746D66;
        private const uint DataId = 0x61746164;

        public List<short> LeftChannel;
        public List<short> RightChannel;

        /// <summary>
        /// For mono files, write samples here
        /// </summary>
        public List<short> CenterChannel;

        /// <summary>
        /// Play a wav file asynchronously
        /// </summary>
        /// <param name="filename"></param>
        // internal static void Play(string filename)
        // {
        //     var wavPlayer = new SoundPlayer { SoundLocation = filename };
        //     wavPlayer.LoadCompleted += (o, e) => ((SoundPlayer)o).Play();
        //     wavPlayer.LoadAsync();
        // }


        /// <summary>
        /// Write 16 bit header, one or two channel wav file
        /// After this write raw data, then close file
        /// </summary>
        /// <param name="sampleRate"></param>
        /// <param name="sampleCount">Samples in a single channel</param>
        /// <param name="channels">Number of channels 1 or 2</param>
        /// <returns></returns>
        public bool WriteHeader(FileStream fs, string filename, int sampleRate, int sampleCount, int channels = 2)
        {
            var retval = true;
            Trace.TraceInformation("Writing audio file " + filename);

            if (channels != 1 && channels != 2)
            {
                Trace.TraceError("Illegal number of channels {0} in Write wave file", channels);
                return false;
            }

            using (var br = new BinaryWriter(fs))
            {
                var size = (uint)(sampleCount * 2 * channels + 16 + 8 + 8 + 4);

                br.Write(RiffId);
                br.Write(size);
                br.Write(WavId); // 4

                br.Write(FmtId); // 4
                br.Write((uint)16); // format size block  // 4
                br.Write((ushort)(1)); // PCM/uncompressed  // 2
                br.Write((ushort)channels); // 2 channels          // 2 
                br.Write((uint)sampleRate); // 4
                br.Write((uint)(sampleRate * 2 * channels)); // bytes/sec, 2 bytes per sample, # channels // 4
                br.Write((ushort)(2 * channels)); // blocksize is bytes per sound sample * # channels // 2
                br.Write((ushort)16); //bits per channel // 2

                br.Write(DataId); // 4
            }

            return true;
        }


        /// <summary>
        /// Write 16 bit, one or two channel wav file
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="sampleRate"></param>
        /// <param name="channels">Number of channels 1 or 2</param>
        /// <returns></returns>
        public bool Write(string filename, int sampleRate, int channels = 2)
        {
            var retval = true;
            Trace.TraceInformation("Writing audio file " + filename);

            if (channels != 1 && channels != 2)
            {
                Trace.TraceError("Illegal number of channels {0} in Write wave file", channels);
                return false;
            }

            try
            {
                using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    var sampleCount = (channels == 1) ? CenterChannel.Count : LeftChannel.Count;
                    WriteHeader(fs, filename, sampleRate, sampleCount, channels);
                    using (var br = new BinaryWriter(fs))
                    {
                        if (channels == 1)
                        {
                            br.Write((uint)(CenterChannel.Count * 2)); // data size
                            for (var i = 0; i < CenterChannel.Count; ++i)
                                br.Write((ushort)CenterChannel[i]);
                        }
                        else if (channels == 2)
                        {
                            br.Write((uint)(LeftChannel.Count * 4)); // data size

                            for (var i = 0; i < LeftChannel.Count; ++i)
                            {
                                br.Write((ushort)LeftChannel[i]);
                                br.Write((ushort)RightChannel[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                retval = false;
            }

            return retval;
        }

        /// <summary>
        /// Read data. Error messages sent through Trace.Error
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>true on success</returns>
        public bool Read(string filename)
        {
            LeftChannel = new List<short>();
            RightChannel = new List<short>();
            var retval = true;
            Trace.TraceInformation("Loading audio file " + filename);

            try
            {
                using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    Header.RiffID = br.ReadUInt32();
                    if (Header.RiffID != RiffId)
                        throw new Exception("Invalid RIFF id");
                    Header.Size = br.ReadUInt32();
                    Header.WavID = br.ReadUInt32();
                    if (Header.WavID != WavId)
                        throw new Exception("Invalid Wav id");
                    Header.FmtID = br.ReadUInt32();
                    if (Header.FmtID != FmtId)
                        throw new Exception("Invalid fmt id");
                    Header.FmtSize = br.ReadUInt32();
                    /*
                         * Code Description 
                        0 (0x0000) Unknown 
                        1 (0x0001) PCM/uncompressed 
                        2 (0x0002) Microsoft ADPCM 
                        6 (0x0006) ITU G.711 a-law 
                        7 (0x0007) ITU G.711 Âµ-law 
                        17 (0x0011) IMA ADPCM 
                         20 (0x0016) ITU G.723 ADPCM (Yamaha) 
                        49 (0x0031) GSM 6.10 
                        64 (0x0040) ITU G.721 ADPCM 
                        80 (0x0050) MPEG 
                         65,536 (0xFFFF) Experimental 
                         * */
                    Header.Format = br.ReadUInt16();
                    if (Header.Format != 1)
                        throw new Exception("Not PCM format");

                    Header.Channels = br.ReadUInt16();
                    Header.SampleRate = br.ReadUInt32();
                    Header.BytePerSec = br.ReadUInt32();
                    Header.BlockSize = br.ReadUInt16();
                    Header.BitsPerChannel = br.ReadUInt16();

                    // skip chunks until DATA
                    do
                    {
                        Header.DataID = br.ReadUInt32();
                        Header.DataSize = br.ReadUInt32();
                        if (Header.DataID != DataId)
                            fs.Seek(Header.DataSize, SeekOrigin.Current);
                    } while (Header.DataID != DataId);


                    if (Header.DataID != DataId)
                        throw new Exception("Invalid DATA id");

                    for (var i = 0; i < Header.DataSize / Header.BlockSize; i++)
                    {
                        LeftChannel.Add((short)br.ReadUInt16());
                        if (Header.Channels == 2)
                            RightChannel.Add((short)br.ReadUInt16());
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message);
                retval = false;
            }

            return retval;
        }
    }
}
