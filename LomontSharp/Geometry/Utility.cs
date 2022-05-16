using System;
using System.Collections.Generic;
using System.Diagnostics;
using Lomont.Numerical;
using static System.Math;

namespace Lomont.Geometry
{
    public static class Utility
    {
        // todo - organize this file better

        // get normal to triangle
        public static Vec3 Normal(Vec3 p0, Vec3 p1, Vec3 p2)
        {
            var d1 = p1 - p0;
            var d2 = p2 - p0;
            var n = Vec3.Cross(d1, d2);
            return n.Normalized();

        }

        /// <summary>
        /// Clip a segment to a box
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="minCorner"></param>
        /// <param name="maxCorner"></param>
        /// <returns>if clipped, and the clipped points</returns>
        public static (bool, Vec3 clippedP1, Vec3 clippedP2) ClipSegmentToBox(Vec3 p1, Vec3 p2, Vec3 minCorner, Vec3 maxCorner)
        {
            // Smits’ method, with improvements from "An Efficient and Robust Ray–Box Intersection Algorithm" by Williams, Barrus, Morley, and Shirley

            // parametric line L(t)=p1+t*(p2-p1)
            // clip on t

            var dir = p2 - p1;

            // clip each dim one at a time, track min, bail if out of bounds
            var (tmin, tmax) = ClipSlab(dir.X, minCorner.X, maxCorner.X, p1.X);

            // misses box?
            if (tmax < 0 || 1 < tmin)
                return (false, Vec3.Zero, Vec3.Zero);

            var (tymin, tymax) = ClipSlab(dir.Y, minCorner.Y, maxCorner.Y, p1.Y);

            // misses box?
            if (tymax < 0 || 1 < tymin)
                return (false, Vec3.Zero, Vec3.Zero);
            if (tmin > tymax || tymin > tmax)
                return (false, Vec3.Zero, Vec3.Zero);

            // keep min values
            if (tymin > tmin) // note this inequality seems backwards because we're more restrictive
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;

            var (tzmin, tzmax) = ClipSlab(dir.Z, minCorner.Z, maxCorner.Z, p1.Z);

            // misses box?
            if (tzmax < 0 || 1 < tzmin)
                return (false, Vec3.Zero, Vec3.Zero);
            if ((tmin > tzmax) || (tzmin > tmax))
                return (false, Vec3.Zero, Vec3.Zero);

            // final bounds
            if (tzmin > tmin)
                tmin = tzmin;
            if (tzmax < tmax)
                tmax = tzmax;

            // check against actual segment overlap
            if (tmin <= 1 && 0 <= tmax)
            {
                tmin = Max(0, tmin);
                tmax = Min(1, tmax);

                p1 = p1 + dir * tmin;
                p2 = p1 + dir * tmax;

                return (true, p1, p2);
            }

            return (false, Vec3.Zero, Vec3.Zero);

            // helper - clips a bound to a slab of values, handles infinities properly
            static (double min, double max) ClipSlab(double dir, double minVal, double maxVal, double val)
            {
                // DO NOT CHECK FOR ZERO! This is designed to work correctly with IEEE 754 math
                var div = 1 / dir;

                if (div >= 0) // note - handles infinity cases also
                    return ((minVal - val) * div, (maxVal - val) * div);
                else
                    return ((maxVal - val) * div, (minVal - val) * div);
            }

        }

        static bool isZero(double v)
        {
            return Math.Abs(v) < 1e-5; // todo - make better
        }

        /// <summary>
        /// Polygon area. Assumes is planar
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static double PolygonArea(IList<Vec3> points)
        {
            // Stokes theorem
            // http://geomalgorithms.com/a01-_area.html

            var len = points.Count;
            if (len < 3)
                return 0;

            // normal
            var p0 = points[0];
            var p1 = points[1];
            var p2 = points[2];

            var normal = Vec3.Cross(p0 - p1, p2 - p1);
            if (isZero(normal.Length))
                return 0; // no normal, fails an assumption

            normal.Normalize();

            var sum = new Vec3();
            for (var i = 0; i < len; ++i)
                sum += Vec3.Cross(points[i], points[(i + 1) % len]);

            return Abs(0.5 * Vec3.Dot(normal, sum));
        }



        // given p in quad p0-p3, find u,v parameter space coords [0,1]
        public static (double u, double v) InvertBilinear(Vec2 p,
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
        /// Find closest points on two segments
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static (double, Vec3 pClosest, Vec3 qClosest) SegmentToSegmentDistance(Vec3 p1, Vec3 p2, Vec3 q1, Vec3 q2)
        {
            // adapted from algorithm http://geomalgorithms.com/a07-_distance.html#dist3D_Segment_to_Segment()
            // Copyright 2001 softSurfer, 2012 Dan Sunday
            // This code may be freely used, distributed and modified for any purpose
            // providing that this copyright notice is included with it.
            // SoftSurfer makes no warranty for this code, and cannot be held
            // liable for any real or imagined damage resulting from its use.
            // Users of this code must verify correctness for their application.

            // This avoids the case where someone is doing line to point with p1, p2 as the line and q1, q2 as the point as this does not account for this right now
            Trace.Assert((q1 != q2) || ((p1 == p2) || (q1 == q2)));

            var u = p2 - p1;
            var v = q2 - q1;
            var w = p1 - q1;

            var a = Vec3.Dot(u, u); // always >= 0
            var b = Vec3.Dot(u, v);
            var c = Vec3.Dot(v, v); // always >= 0
            var d = Vec3.Dot(u, w);
            var e = Vec3.Dot(v, w);

            // discriminant
            var D = a * c - b * b; // always >= 0

            // compute the line parameters of the two closest points
            var sD = D; // sc = sN / sD, default sD = D >= 0
            var tD = D; // tc = tN / tD, default tD = D >= 0

            double sN, tN;

            if (isZero(D))
            {
                // the lines are almost parallel
                sN = 0.0; // force using point P0 on segment S1
                sD = 1.0; // to prevent possible division by 0.0 later
                tN = e;
                tD = c;
            }
            else
            {
                // get the closest points on the infinite lines
                sN = (b * e - c * d);
                tN = (a * e - b * d);
                if (sN < 0.0)
                {
                    // sc < 0 => the s=0 edge is visible
                    sN = 0.0;
                    tN = e;
                    tD = c;
                }
                else if (sN > sD)
                {
                    // sc > 1  => the s=1 edge is visible
                    sN = sD;
                    tN = e + b;
                    tD = c;
                }
            }

            if (tN < 0.0)
            {
                // tc < 0 => the t=0 edge is visible
                tN = 0.0;
                // recompute sc for this edge
                if (-d < 0.0)
                    sN = 0.0;
                else if (-d > a)
                    sN = sD;
                else
                {
                    sN = -d;
                    sD = a;
                }
            }
            else if (tN > tD)
            {
                // tc > 1  => the t=1 edge is visible
                tN = tD;
                // recompute sc for this edge
                if ((-d + b) < 0.0)
                    sN = 0;
                else if ((-d + b) > a)
                    sN = sD;
                else
                {
                    sN = (-d + b);
                    sD = a;
                }
            }

            // finally do the division to get sc and tc
            var sc = isZero(sN) ? 0.0 : sN / sD;
            var tc = isZero(tN) ? 0.0 : tN / tD;

            // closest points:
            var pp = p1 + sc * u;
            var qq = q1 + tc * v;

            return ((pp - qq).Length, pp, qq);
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
            {
                // collinear
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
            {
                // parallel and non-intersecting
                return false;
            }
            else
            {
                // rxs != 0
                var t = Vec2.Cross2D(q - p, s) / rxs;
                var u = qmpxr / rxs;
                if (0 <= t && t <= 1 && 0 <= u && u <= 1)
                    return true; // hit at (t,u) parameter
            }

            return false;
        }

        /// <summary>
        /// 2D point in polygon
        /// </summary>
        /// <param name="point"></param>
        /// <param name="polygon"></param>
        /// <returns></returns>
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


        /// <summary>
        /// Ray-triangle intersection
        /// </summary>
        /// <param name="rayOrigin"></param>
        /// <param name="rayDir"></param>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns>if hits, the point, and the distance</returns>
        public static (bool hits, Vec3 point, double distance) 
            RayTriangleIntersection(
                Vec3 rayOrigin,
                Vec3 rayDir,
                Vec3 p0,
                Vec3 p1,
                Vec3 p2
                )
        {
            // Möller–Trumbore intersection algorithm
            // https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm

            var n = rayDir.Normalized();

            // edges
            var e10 = p1 - p0;
            var e20 = p2 - p0;

            var h = Vec3.Cross(n, e20); // perp to edge 2->0 and ray
            var a = Vec3.Dot(e10, h);
            if (isZero(a)) // if h parallel to edge 1-0 then no hit
                return (false,Vec3.Zero,Double.MaxValue);

            var f = 1 / a;
            var s = rayOrigin - p0;
            var u = f * Vec3.Dot(s, h);
            if (u < 0.0 || u > 1.0)
                return (false, Vec3.Zero, Double.MaxValue);

            var q = Vec3.Cross(s, e10);
            var v = f * Vec3.Dot(n, q);

            if (v < 0.0 || u + v > 1.0)
                return (false, Vec3.Zero, Double.MaxValue);

            // t is parametric intersection point on line
            var t = f * Vec3.Dot(e20, q);

            // no hit if too close or behind origin
            if (t < 0.000001)
                return (false, Numerical.Vec3.Zero, 0);

            return (true, rayOrigin + n * t, t);
        }


        /// <summary>
        /// compute the best fit plane for a cloud of points
        /// returns a point on the plane and a unit normal vector to the plane
        /// note the normal is not unique: it can be +- of the same vector
        /// </summary>
        /// <param name="points"></param>
        /// <param name="normal"></param>
        /// <param name="planePoint"></param>
        public static (Vec3 normal, Vec3 pointOnPlane) BestFitPlane(IList<Vec3> points)
        {
            // uses the algorithms covered at:
            // https://math.stackexchange.com/questions/99299/best-fitting-plane-given-a-set-of-points
            // http://www.janssenprecisionengineering.com/downloads/Fit-plane-through-data-points.pdf
            // Simple algorithm
            // https://www.ilikebigbits.com/2015_03_04_plane_from_points.html

            var n = points.Count;
            if (n < 3)
                return (new Vec3(0, 0, 1), new Vec3());

            // compute centroid
            var sum = new Vec3();
            foreach (var c in points)
                sum += c;
            var centroid = sum / n;
            var (xc, yc, zc) = centroid;

            // make 3xn matrix of n points
            var mat = new Matrix(3, n);
            // compute matrix, moving points to centroid for stability
            for (var i = 0; i < n; ++i)
            {
                mat[0, i] = points[i ].X - xc;
                mat[1, i] = points[i ].Y - yc;
                mat[2, i] = points[i ].Z - zc;
            }

            // singular value decomposition
            var svd = new SingularValueDecomposition(mat);
            // todo - check svd.Condition number is nice

            // singular vectors, needed to make U public in Mapack library
            var nx = svd.U[0, 2];
            var ny = svd.U[1, 2];
            var nz = svd.U[2, 2];

            var normal = new Vec3(nx, ny, nz).Normalized();
            var planePoint = new Vec3(xc, yc, zc);
            return (normal, planePoint);
        }


    }
}
