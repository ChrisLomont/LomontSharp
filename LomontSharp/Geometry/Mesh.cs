using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Lomont.Graphics;
using Lomont.Numerical;
using static System.Math;

namespace Lomont.Geometry
{
    /// <summary>
    /// A simple, naive mesh format
    /// A mesh is a collection of vertices and faces that makes a 
    /// closed surface representing a 3D shape
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// 3D points that make up the mesh
        /// </summary>
        public List<Vec3> Points { get; private set; }

        /// <summary>
        /// Indices defining faces. Each entry is a list of point indices defining the face
        /// </summary>
        public List<List<int>> Indices { get; private set; }

        /// <summary>
        /// Colors for each face
        /// </summary>
        public List<Color> FaceColors { get; private set; }

        public Mesh(List<Vec3> points, List<List<int>> indices, List<Color> faceColors)
        {
            Points = points;
            Indices = indices;
            FaceColors = faceColors;
        }

        public static Mesh MakeRing(Vec3 point1, Vec3 point2, double outerRadius, double innerRadius, int sides,
            Color color)
        {
            // point from 1 to 2
            var normal = Vec3.Unit(point1 - point2);

            var length = (point1 - point2).Length;

            var pts1 = PointCreator.MakeCircle(point2, outerRadius, sides, normal);
            var pts2 = pts1.Select(p => p + length * normal).ToList();

            var pts3 = PointCreator.MakeCircle(point2, innerRadius, sides, normal);
            var pts4 = pts3.Select(p => p + length * normal).ToList();

            Debug.Assert(pts1.Count == sides);

            var pts = new List<Vec3>();

            pts.AddRange(pts1);
            pts.AddRange(pts2);
            pts.AddRange(pts3);
            pts.AddRange(pts4);


            var indices = new List<List<int>>();

            // add outer sides
            for (var i = 0; i < sides; ++i)
            {
                var s = new List<int>
                {
                    i,
                    i + sides,
                    ((i + 1) % sides) + sides,
                    (i + 1) % sides
                };
                indices.Add(s);
            }

            // add inner sides
            var d = sides * 2;
            for (var i = 0; i < sides; ++i)
            {
                var s = new List<int>();
                s.Add(i + d);
                s.Add(((i + 1) % sides) + d);
                s.Add(((i + 1) % sides) + sides + d);
                s.Add(i + sides + d);
                indices.Add(s);
            }

            // add top/bottom
            for (var i = 0; i < sides; i++)
            {
                // i,i+1,i+1+d,i+d
                var top = new List<int>();
                top.Add(i);
                top.Add((i + 1) % sides);
                top.Add(((i + 1) % sides) + d);
                top.Add(i + d);
                indices.Add(top);

                var bot = new List<int>();
                bot.Add(i + sides);
                bot.Add(i + d + sides);
                bot.Add(((i + 1) % sides) + d + sides);
                bot.Add(((i + 1) % sides) + sides);
                indices.Add(bot);

            }


            // colors
            var colors = new List<Color>();
            for (var i = 0; i < indices.Count; ++i)
                colors.Add(color);

            return new Mesh(pts, indices, colors);
        }

        /// <summary>
        /// Given quad corner p0, and neighboring ends corners and p2, make a parallel edges quadrilateral
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="count01"></param>
        /// <param name="count02"></param>
        /// <returns></returns>
        public static (List<Vec3> vertices, List<List<int>> faces) MakeQuad(Vec3 p0, Vec3 p1, Vec3 p2, 
            int count01=1,
            int count02=1)
        {
            var vertices = new List<Vec3>();
            var faces = new List<List<int>>();
            var d1 = (p1 - p0) / count01;
            var d2 = (p2 - p0) / count02;

            for (var j = 0; j <= count01; ++j)
            for (var i = 0; i <= count02; ++i)
            {
                vertices.Add(p0 + d1 * j + d2 * i);
                if (i != count02 && j != count01)
                {
                    faces.Add(new List<int> { Index(i, j), Index(i + 1, j), Index(i + 1, j + 1) });
                    faces.Add(new List<int> { Index(i, j), Index(i + 1, j + 1), Index(i, j + 1) });
                }
            }

            return (vertices, faces);

            int Index(int i, int j) => i + j * (count02 + 1);

        }

        /// <summary>
        /// Make a sphere
        /// TODO - have versions from cube, dodeca, etc. faceted and projected
        /// ,make version for spherical patches of limited angles
        /// make ellipsoidal version
        /// </summary>
        /// <param name="center"></param>
        /// <param name="radius"></param>
        /// <param name="horizontalLines">Lines parallel to equator, not including poles</param>
        /// <param name="verticalLines"></param>
        /// <returns></returns>
        public static (List<Vec3> vertices, List<List<int>> triangles) MakeSphere(Vec3 center, double radius, int horizontalLines, int verticalLines)
        {

            var vertices = new List<Vec3>();
            var faces = new List<List<int>>();

            for (var m = 1; m <= horizontalLines; m++) // this range misses the poles
            {
                // 
                var z = center.Z + radius * Cos(PI * m / (horizontalLines + 1));
                var zr = radius * Sin(PI * m / (horizontalLines + 1));
                for (var n = 0; n < verticalLines; n++)
                { 
                    var x = center.X + zr * Cos(2 * PI * n / verticalLines);
                    var y = center.Y + zr * Sin(2 * PI * n / verticalLines);
                    vertices.Add(new(x, y, z));

                    // two triangles
                    if (m != horizontalLines)
                    {
                        var n2 = (n + 1) % verticalLines;
                        faces.Add(new List<int> { Ind(n, m - 1), Ind(n, m), Ind(n2, m - 1) });
                        faces.Add(new List<int> { Ind(n2, m), Ind(n2, m - 1), Ind(n, m) });
                    }
                }
            }

            // poles
            var p1 = vertices.Count;
            var p2 = p1 + 1;
            vertices.Add(new Vec3(0, 0, radius) + center);
            vertices.Add(new Vec3(0, 0, -radius) + center);
            var h = horizontalLines;


            for (var n = 0; n < verticalLines; n++)
            {
                // two poles
                var n2 = (n + 1) % verticalLines;
                faces.Add(new List<int> { Ind(n2, 0), p1, Ind(n, 0)});
                faces.Add(new List<int> { Ind(n, h-1), p2, Ind(n2, h-1)});
            }

            return (vertices, faces);

            int Ind(int i, int j) => i + j * verticalLines;
        }



        public static Mesh MakeCylinder(Vec3 point1, Vec3 point2, double radius, int sides, Color color)
        {
            // point from 1 to 2
            var normal = Vec3.Unit(point1 - point2);

            var length = (point1 - point2).Length;

            var pts = PointCreator.MakeCircle(point2, radius, sides, normal);
            var pts2 = pts.Select(p => p + length * normal).ToList();

            Debug.Assert(pts.Count == sides);

            pts.AddRange(pts2);


            var indices = new List<List<int>>();

            // add sides
            for (var i = 0; i < sides; ++i)
            {
                var s = new List<int>();
                s.Add(i);
                s.Add(i + sides);
                s.Add(((i + 1) % sides) + sides);
                s.Add((i + 1) % sides);
                indices.Add(s);
            }

            // add top/bottom
            var top = new List<int>();
            var bot = new List<int>();
            for (var i = 0; i < sides; ++i)
            {
                top.Add(i);
                bot.Add(2 * sides - 1 - i);
            }
            indices.Add(top);
            indices.Add(bot);


            // colors
            var colors = new List<Color>();
            for (var i = 0; i < indices.Count; ++i)
                colors.Add(color);

            return new Mesh(pts, indices, colors);
        }
    }
}
