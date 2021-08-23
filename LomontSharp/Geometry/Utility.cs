using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Numerical;

namespace Lomont.Geometry
{
    public static class Utility
    {
        // get normal to triangle
        public static Vec3 Normal(Vec3 p0, Vec3 p1, Vec3 p2)
        {
            var d1 = p1 - p0;
            var d2 = p2 - p0;
            var n = Vec3.Cross(d1, d2);
            return n.Normalize();

        }

        static bool isZero(double v)
        {
            return Math.Abs(v) < 1e-5; // todo - make better
        }


        // given p in quad p0-p3, find u,v parameter space coords [0,1]
        public static(double u, double v) InvertBilinear(Vec2 p,
            Vec2 p0, Vec2 p1, Vec2 p2, Vec2 p3)
        {
            // https://iquilezles.org/www/articles/ibilinear/ibilinear.htm

            var h = p - p0;
            var e = p1 - p0;
            var f = p3 - p0;
            var g = p0 - p1 + p2 - p3;
            var a = Vec2.Cross2D(g, f);
            var b = Vec2.Cross2D(e, f) + Vec2.Cross2D(h, g);
            var c = Vec2.Cross2D(h, e);
            var d = b * b - 4 * a * c;

            double v1 = Double.NaN, v2 = Double.NaN; // marked as invalid

            if (isZero(a) && isZero(b))
                return (0.5, 0.5); // error
            else if (!isZero(a) && d < 0)
                return (0.5, 0.5); // error
            else if (isZero(a) && !isZero(b))
                v1 = -c / b;
            else
            {
                var sqr = Math.Sqrt(d);
                v1 = (-b + sqr) / (2 * a);
                v2 = (-b + sqr) / (2 * a);
            }

            double u1, u2;

            double mTol = 1e-4;

            Debug.Assert(!Double.IsNaN(v1));
            // compute denominators: ek+v*gk for each 
            var d1 = e + v1 * g;
            if (isZero(d1.Length))
                return (0.5, 0.5); // error
            if (Math.Abs(d1.X) > Math.Abs(d1.Y))
                u1 = (h.X - v1 * f.X) / d1.X;
            else
                u1 = (h.Y - v1 * f.Y) / d1.Y;
            if ((Bilinear(u1, 1, v1, 1, p0, p1, p2, p3) - p).Length < mTol)
                return (u1, v1);

            if (Double.IsNaN(v2))
                return (0.5, 0.5); // error
            // compute denominators: ek+v*gk for each 
            var d2 = e + v2 * g;
            if (isZero(d2.Length))
                return (0.5, 0.5); // error
            if (Math.Abs(d2.X) > Math.Abs(d2.Y))
                u2 = (h.X - v2 * f.X) / d2.X;
            else
                u2 = (h.Y - v2 * f.Y) / d2.Y;
            if ((Bilinear(u2, 2, v2, 2, p0, p2, p2, p3) - p).Length < mTol)
                return (u2, v2);

            return (0.5, 0.5); // error
        }

        // Bilinear interpolate
        // u,v goes (0,0) to (umax,vmax) inclusive
        // points in circumference order
        // u along p0-p1 and p3-p2 edge, v along p2-p3 and p0-p3
        public static Vec2 Bilinear(
            double u, double umax,
            double v, double vmax,
            Vec2 p0,
            Vec2 p1,
            Vec2 p2,
            Vec2 p3
        )
        {
            var alpha = u / umax;
            var beta = v / vmax;
            return p0 + alpha * (p1 - p0) + beta * (p3 - p0) + alpha * beta * (p0 - p1 + p2 - p3);
        }
        // Bilinear interpolate
        // u,v goes (0,0) to (umax,vmax) inclusive
        // points in circumference order
        // u along p0-p1 and p3-p2 edge, v along p2-p3 and p0-p3
        public static double Bilinear(
            double u, double umax, double v, double vmax,
            double p0,
            double p1,
            double p2,
            double p3
        )
        {
            var alpha = u / umax;
            var beta = v / vmax;
            return p0 + alpha * (p1 - p0) + beta * (p3 - p0) + alpha * beta * (p0 - p1 + p2 - p3);
        }

        public static bool PointInTriangle(Vec2 p, Vec2 p0, Vec2 p1, Vec2 p2)
        {
            // three half plane tests
            var d0 = p1 - p0;
            var d1 = p2 - p1;
            var d2 = p0 - p2;
            var s0 = Vec2.Cross2D(d0, p - p0);
            var s1 = Vec2.Cross2D(d1, p - p1);
            var s2 = Vec2.Cross2D(d2, p - p2);
            if (s0 <= 0 && s1 <= 0 && s2 <= 0)
                return true;
            if (s0 >= 0 && s1 >= 0 && s2 >= 0)
                return true;
            return false;
        }

        public static bool PointInPolygon(List<Vec2> vert, Vec2 p)
        {
            // not quite robust, but small and easy
            // https://stackoverflow.com/questions/217578/how-can-i-determine-whether-a-2d-point-is-within-a-polygon
            // 
            int i, j;
            bool c = false;
            var nvert = vert.Count;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((vert[i].Y > p.Y) != (vert[j].Y > p.Y)) &&
                    (p.X < (vert[j].X - vert[i].X) * (p.Y - vert[i].Y) / (vert[j].Y - vert[i].Y) + vert[i].X))
                    c = !c;
            }
            return c;
        }

        /// <summary>
        /// Given a point p in the same plane as a triangle p0,p1,p2,
        /// find three coords c0,c1,c2 with
        /// p = sum ci pi
        /// If p in triangle, then sum ci = 1
        /// </summary>
        public static void Barycentric(Vec2 p, Vec2 a, Vec2 b, Vec2 c, out double u, out double v, out double w)
        {
            // used in OpenGL triangle rendering
            // https://stackoverflow.com/questions/26513712/algorithm-for-coloring-a-triangle-by-vertex-color
            // https://gamedev.stackexchange.com/questions/23743/whats-the-most-efficient-way-to-find-barycentric-coordinates

            Vec2 v0 = b - a, v1 = c - a, v2 = p - a;
            var d00 = Vec2.Dot(v0, v0);
            var d01 = Vec2.Dot(v0, v1);
            var d11 = Vec2.Dot(v1, v1);
            var d20 = Vec2.Dot(v2, v0);
            var d21 = Vec2.Dot(v2, v1);
            var denom = d00 * d11 - d01 * d01;
            Debug.Assert(!isZero(denom));
            v = (d11 * d20 - d01 * d21) / denom;
            w = (d00 * d21 - d01 * d20) / denom;
            u = 1.0 - v - w;
        }

        /// <summary>
        /// Testing function for barycentric code
        /// </summary>
        /// <returns></returns>
        public static void DumpBarycentric()
        {
            var a = new Vec2(0, 0);
            var b = new Vec2(1, 1);
            var c = new Vec2(-1, 1);


            for (var y = 0.1; y < 0.9; y += 0.05)
            {
                var p = new Vec2(0, y);
                double ay, by, cy;
                Barycentric(p, a, b, c, out ay, out by, out cy);

                for (var x = -y; x <= y; x += y / 10)
                {
                    p = new Vec2(x, y);
                    double ax, bx, cx;
                    Barycentric(p, a, b, c, out ax, out bx, out cx);

                    var d = Math.Abs(ax - ay);
                    Debug.Assert(d < 0.0000000001);

                }
            }
        }

        // does segment a1-a2 intersect segment b1-b2?
        public static bool Intersects(Vec2 a1, Vec2 a2, Vec2 b1, Vec2 b2)
        {
            // todo - move to geometry helper class
            // derived from  https://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect
            var p = a1;
            var q = b1;
            var r = a2 - a1;
            var s = b2 - b1;
            var rxs = Vec2.Cross2D(r, s);
            var qmpxr = Vec2.Cross2D(q - p, r);

            var tolerance = 1e-6;
            var rxsz = Math.Abs(rxs) < tolerance;
            var qmpxrz = Math.Abs(qmpxr) < tolerance;

            if (r.Length < tolerance || s.Length < tolerance)
                return false; // todo - can check point on segment


            if (rxsz && qmpxrz)
            { // collinear
                var r2 = Vec2.Dot(r, r);
                var t0 = Vec2.Dot(q - p, r) / r2;
                var t1 = Vec2.Dot(q + s - p, r) / r2;
                var min = Math.Min(t0, t1);
                t1 = Math.Max(t1, t0);
                t0 = min;
                Debug.Assert(t0 < t1);
                // see if [t0,t1] intersects [0,1]
                // misses if t1 < 0 or 1 < t0, negate
                return !(t1 < 0 || 1 < t0);
            }
            else if (rxsz && !qmpxrz)
            { // parallel and non-intersecting
                return false;
            }
            else
            { // rxs != 0
                var t = Vec2.Cross2D(q - p, s) / rxs;
                var u = qmpxr / rxs;
                if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                    return true; // hit at (t,u) parameter
            }

            return false;
        }

        // sloppy algo, allows false negatives on some edge of polygon cases
        public static bool PointInPolygon(Vec2 point, List<Vec2> polygon)
        {
            var (testx, testy) = point;
            int i, j;
            bool c = false;
            var nvert = polygon.Count;
            var p = polygon;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (
                    ((p[i].Y > testy) != (p[j].Y > testy)) &&
                    (testx - p[i].X < (p[j].X - p[i].X) * (testy - p[i].Y) / (p[j].Y - p[i].Y)
                    )
                )
                    c = !c;
            }

            return c;
        }

    }
}
