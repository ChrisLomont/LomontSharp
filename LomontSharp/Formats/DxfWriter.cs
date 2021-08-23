using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Formats
{
    public static class DxfWriter
    {
        // http://www.autodesk.com/techpubs/autocad/acad2000/dxf/dxf_format.htm

        //static readonly string version = "AC1027"; // AC 2013
        static readonly string version = "AC1009"; // AC ??

        static readonly string header =
            "999" + Environment.NewLine +
            "Created by Chris Lomont's DXF writer 2014" + Environment.NewLine +
            $"0\nSECTION\n2\nHEADER\n9\n$ACADVER\n1\n{version}\n0\nENDSEC\n0\nSECTION\n2\nENTITIES\n";

        /* header
999
Created by Wolfram Mathematica 7.0 : www.wolfram.com
0
SECTION
2
HEADER
9
$ACADVER
1
AC1009
0
ENDSEC
0
SECTION
2
ENTITIES
         */
        /* each line looks like
        0
        LINE
        8
        0
        10
        1.7320508075688772
        20
        1.
        30
        0.
        11
        0.9604221727969424
        21
        1.4455
        31
        0.
                 * */
        private const string Footer = "0\nENDSEC\n0\nEOF\n";
        private const string LineFormat = "0\nLINE\n8\n{0}\n10\n{1}\n20\n{2}\n30\n{3}\n11\n{4}\n21\n{5}\n31\n{6}\n";
        private const string CircleFormat = "0\nCIRCLE\n8\n{0}\n10\n{1}\n20\n{2}\n30\n{3}\n40\n{4}\n";

        /* footer
0
ENDSEC
0
EOF
         */

        /* Format:
         * Each section starts with group code 0 followed by SECTION, and ends with 0 ENDSEC.
         * SECTIONS: 
         *    HEADER 
         *    CLASSES
         *    TABLES
         *        APPID
         *        BLOCK_RECORD
         *        DIMSTYLE
         *        LAYER
         *        LTYPE - must come before LAYER if both exist
         *        STYLE
         *        UCS
         *        VIEW
         *        VPORT
         *    BLOCKS
         *    ENTITIES
         *    OBJECTS
         *    THUMBNAILIMAGE
         *    
         * 
         *  ENTITIES holds geometry
         */

        /// <summary>
        /// Write a DXF consisting of lines
        /// Each line is 4 consecutive doubles 
        /// in order x1,y1,x2,y2
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="lines"></param>
        /// <param name="circles">(x,y,r) three tuples</param>
        /// <param name="layers">0-7 each entity</param>
        public static void Write(
            string filename,
            List<double> lines,
            List<double> circles = null,
            List<int> layers = null
        )
        {
            if ((lines.Count & 3) != 0)
                throw new ArgumentException("Lines needs to contain a multiple of 4 entries to define 2D lines");
            using (var file = File.CreateText(filename))
            {
                file.Write(header);
                var layer = 0;
                const double z1 = 0.0;
                const double z2 = 0.0;
                for (var i = 0; i < lines.Count; i += 4)
                {
                    if (layers != null)
                        layer = layers[i / 4];
                    file.Write(LineFormat, layer, lines[i], lines[i + 1], z1, lines[i + 2], lines[i + 3], z2);
                }

                if (circles != null)
                {
                    for (var i = 0; i < circles.Count; i += 3)
                    {
                        if (layers != null)
                            layer = layers[i / 3 + lines.Count / 4];
                        file.Write(CircleFormat, layer, circles[i], circles[i + 1], z1, circles[i + 2]);
                    }
                }



                file.Write(Footer);
            }
        }
    }
}
