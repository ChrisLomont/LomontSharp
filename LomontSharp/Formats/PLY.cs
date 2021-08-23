using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lomont.Geometry;
using Lomont.Graphics;
using Lomont.Numerical;

namespace Lomont.Formats
{
    /// <summary>
    /// Read and write the PLY mesh format
    /// </summary>
    public class PLY
    {
        // sample files at https://people.sc.fsu.edu/~jburkardt/data/ply/ply.html
        public static void Write(string filename, IList<Vec3> vertices, IList<List<int>> faces)
        {
            Write(filename, vertices, vertices.Count, faces, faces.Count);

        }

        public static void Write(string filename, IEnumerable<Vec3> vertices, int vertexCount, IEnumerable<List<int>> faces, int faceCount)
        {
            var bin = false;
            // todo - this only for little endian

            //using var writer = new BinaryWriter(new FileStream(filename, FileMode.Create), Encoding.ASCII);
            using var writer = new StreamWriter(new FileStream(filename, FileMode.Create), Encoding.ASCII);

            // headers
            writer.Write(StringToByteArray("ply\n"));
            //writer.Write(StringToByteArray("format binary_little_endian 1.0\n"));
            writer.Write(StringToByteArray("format ascii 1.0\n"));
            writer.Write(StringToByteArray($"element vertex {vertexCount}\n"));
            writer.Write(StringToByteArray("property float32 x\n"));
            writer.Write(StringToByteArray("property float32 y\n"));
            writer.Write(StringToByteArray("property float32 z\n"));
            writer.Write(StringToByteArray($"element face {faceCount}\n"));
            writer.Write(StringToByteArray("property list uint8 int32 vertex_indices\n"));
            writer.Write(StringToByteArray("end_header\n"));

            string StringToByteArray(string s) => s;

            // vertices
            foreach (var (x, y, z) in vertices)
            {
                if (bin)
                {
                    writer.Write(Float32ToByteArray((float)x));
                    writer.Write(Float32ToByteArray((float)y));
                    writer.Write(Float32ToByteArray((float)z));
                }
                else 
                    writer.Write($"{x} {y} {z}\n");
            }

            // faces
            foreach (var face in faces)
            {
                if (bin)
                {
                    writer.Write(UcharToByteArray((byte)(face.Count))); // limits our count todo...
                    foreach (var i in face)
                        writer.Write(Int32ToByteArray(i));
                }
                else
                {
                    writer.Write($"{face.Count}");
                    foreach (var f in face)
                        writer.Write($" {f}");
                    writer.Write("\n");
                }
            }

            // helpers
            byte[] UcharToByteArray(byte n) => BitConverter.GetBytes((byte)(n));
            byte[] Int32ToByteArray(int n) => BitConverter.GetBytes(n);
            byte[] Float32ToByteArray(float value) => BitConverter.GetBytes(value);
            //byte[] StringToByteArray(string theString) => System.Text.Encoding.ASCII.GetBytes(theString);
        }

        public bool Read(string filename)
        {
            if (!File.Exists(filename))
            {
                Trace.TraceWarning($"File {filename} does not exist");
                return false;
            }

            var lines = File.ReadLines(filename); // lazy line reader
            if (!ParseHeader(lines))
                return false;

            return false; // todo
        }

        /// <summary>
        /// Parse header, return true on ok, else false
        /// </summary>
        /// <param name="lines"></param>
        /// <returns></returns>
        bool ParseHeader(IEnumerable<string> lines)
        {
            var state = 0;
            foreach (var line in lines)
            {
                if (state == 0 && line != "ply")
                {
                    Trace.TraceWarning("File missing 'ply' header");
                    return false;
                }
                //todo
            }
            return false; // todo
        }



   }
}