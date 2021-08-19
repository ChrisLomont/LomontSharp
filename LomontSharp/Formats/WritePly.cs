using System;
using System.IO;
using System.Text;
using Lomont.Numerical;

namespace Lomont.Formats
{
    /// <summary>
    /// 
    /// </summary>
    static class WritePly
    {


        /// <summary>
        /// Only works on little endian
        /// </summary>
        /// <param name="filename"></param>
        public static void Write(string filename, 
            Func<int,Vec3> getPoint,
            Func<int, Vec3> getColor,
            int count
            )
        {

            using var writer = new BinaryWriter(new FileStream(filename, FileMode.Create), Encoding.ASCII);
            //Write the headers for 3 vertices
            writer.Write(ToBytes("ply\n"));
            writer.Write(ToBytes("format binary_little_endian 1.0\n"));
            writer.Write(ToBytes($"element vertex {count}\n"));
            writer.Write(ToBytes("property float32 x\n"));
            writer.Write(ToBytes("property float32 y\n"));
            writer.Write(ToBytes("property float32 z\n"));
            writer.Write(ToBytes("property uchar red\n"));
            writer.Write(ToBytes("property uchar green\n"));
            writer.Write(ToBytes("property uchar blue\n"));
            //writer.Write(ToBytes("property float32 nx\n"));
            //writer.Write(ToBytes("property float32 ny\n"));
            //writer.Write(ToBytes("property float32 nz\n"));
            //writer.Write(ToBytes("property float32 radius\n"));

            writer.Write(ToBytes("end_header\n"));

            for (var i = 0; i < count; ++i)
            {
                var position = getPoint(i);
                writer.Write(ToBytes((float)position.X));
                writer.Write(ToBytes((float)position.Y));
                writer.Write(ToBytes((float)position.Z));

                var color = getColor(i);
                writer.Write((byte)(color.X * 255.0));
                writer.Write((byte)(color.Y * 255.0));
                writer.Write((byte)(color.Z * 255.0));

                //writer.Write(ToBytes((float) s.normal.X));
                //writer.Write(ToBytes((float) s.normal.Y));
                //writer.Write(ToBytes((float) s.normal.Z));
                //
                //writer.Write(ToBytes((float) s.radius));
            }
        }
        private static byte[] ToBytes(float value)
        {
            return BitConverter.GetBytes(value);
        }
        private static byte[] ToBytes(string theString)
        {
            return System.Text.Encoding.ASCII.GetBytes(theString);
        }
    }
}
