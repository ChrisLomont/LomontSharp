using Lomont.Numerical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Lomont.Geometry
{
    public static class OrientPoints
    {
        /// <summary>
        /// implement point to point orientation
        /// from "Closed-form solution of absolute orientation using unit quaternions," Horn, 1987
        /// 
        /// Returns (scale,rotation,translation) that maps src into dst so sum of squares distances is minimal
        ///  as dst = s*R(src)+ t0
        ///  
        /// todo - also return error....
        /// </summary>
        public static (double scale, Quat rotation, Vec3 translation) OrientPoints3D(List<Vec3> src, List<Vec3> dst)
        {

            // steps from paper
            // A. obtain local bases: we ignore, treat points as in same space, since we have coordinates already


            // B. finding translation
            var n = src.Count;
            Trace.Assert(n == dst.Count, "Src and dst on point match must be same size");
            if (n < 3)
                throw new Exception("need at least 3 points");

            var rl = src; // left in paper
            var rr = dst; // right in paper

            // find mapping of form rr = s*R(rl)+r0 where s is scale, R is rotation, r0 is shift

            // C. centroids
            // centroids
            var rlbar = rl.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / n;
            var rrbar = rr.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / n;

            // D., E. scale
            var s = Math.Sqrt(dst.Sum(p => (p - rrbar).LengthSquared) / src.Sum(p => (p - rlbar).LengthSquared));

            // section 3. Rotations

            // cross covariance matrix M
            var M = Enumerable.Range(0, n).Aggregate(Mat3.Zero, (cur, i) => cur + Vec3.Outer(src[i], dst[i]));
            M = s * M / n - s * Vec3.Outer(rlbar, rrbar); // remove centroids, scale, ...

            // calculate the 4x4 symmetric matrix N
            var N = new Mat4();

            var trace = M.Trace;

            N[0, 0] = trace;
            N[0, 1] = N[1, 0] = M[1, 2] - M[2, 1];
            N[0, 2] = N[2, 0] = M[2, 0] - M[0, 2];
            N[0, 3] = N[3, 0] = M[0, 1] - M[1, 0];
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                    N[i + 1, j + 1] = M[i, j] + M[j, i] - (i == j ? trace : 0);

            // rotation from eigenvctor
            var rotation = GetQuatFromMaxEigenvector(N);

            var translation = rrbar - s * rotation.Rotate(rlbar);

            return (s, rotation, translation);

        }



        /// <summary>
        /// given list of points p and q that correspond under some absolute orientation
        /// (rotation, translation, scaling), compute that orientation
        /// returned orientation maps src into dst
        /// requires all points to be in the z = 0 plane
        /// returns scale, rotation (counter-clockwise), translation that maps src into dest
        /// dst = scale*rotation(src) + translation
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        public static (double scale, double rotation, Vec2 translation) OrientPoints2D(List<Vec2> src, List<Vec2> dst)
        {
            // Horn method Berthold KP Horn. Closed-form solution of absolute orientation using unit quaternions. JOSA A, 4(4):629–642, 1987
            // special case from planar to planar

            var n = src.Count;
            // B. finding translation
            Trace.Assert(n == dst.Count, "Src and dst on point match must be same size");
            if (n < 3)
                throw new Exception("need at least 3 points");

            var rl = src; // left in paper
            var rr = dst; // right in paper

            // find mapping of form rr = s*R(rl)+r0 where s is scale, R is rotation, r0 is shift

            // C. centroids
            // centroids
            var rlbar = rl.Aggregate(new Vec2(), (cur, nxt) => cur + nxt) / n;
            var rrbar = rr.Aggregate(new Vec2(), (cur, nxt) => cur + nxt) / n;

            // D., E. scale
            var scale = Math.Sqrt(dst.Sum(p => (p - rrbar).LengthSquared) / src.Sum(p => (p - rlbar).LengthSquared));

            // angle
            var S = 0.0;
            var C = 0.0;

            for (var i = 0; i < src.Count; ++i)
            {
                var r1 = dst[i] - rrbar;// dst
                var r2 = src[i] - rlbar; // src
                C += Vec2.Dot(r2, r1);
                S += Vec2.Cross2D(r2, r1); // (r1 cross r2) dot normal)
            }

            // sin, cos values
            var s = S / Math.Sqrt(C * C + S * S);
            var c = C / Math.Sqrt(C * C + S * S);

            var angle = Math.Atan2(s, c);// Math.Acos(c);

            // todo - add rotation to vec2, mat3?
            var (cx, cy) = rlbar;
            var rot_rlbar = new Vec2(c * cx - s * cy, s * cx + c * cy);

            var trans = rrbar - scale * rot_rlbar;

            return (scale, angle, trans);
        }

        // todo - merge tests? relabel name of planar version
        public static bool TestOrientation3D()
        {
#if false
            var c1 = new Mat4(
                1,2,3,4,
                5,2,17,8,
                -4,-3,-2,-1,
                -8,-7,-26,-5
                );
            var c2 = Cofactors(c1);
            Console.WriteLine($"{c1}");
            Console.WriteLine($"{c2}");
            return; // todo 
#endif

            var errCount = 0;
            var r = new Random(1234);
            var size = 100_000.0;
            var maxErr = 0.0;
            Func<double> v = () => r.NextDouble() * 2 * size - size;
            for (var pass = 0; pass < 1000; ++pass)
            {
                var len = r.Next(50, 10_000);
                var truthPath = new List<Vec3>();
                for (var j = 0; j < len; ++j)
                    truthPath.Add(new Vec3(v(), v(), v()));

                // todo - remove
#if false
                truthPath.Clear();

#if false
                truthPath.Add(new Vec3(0, 0, 0));
                truthPath.Add(new Vec3(1, 0, 0));
                truthPath.Add(new Vec3(0, 1, 0));
                truthPath.Add(new Vec3(0, 0, 1));
                truthPath.Add(new Vec3(1, 1, 1));
                truthPath.Add(new Vec3(7, 8, 9));
#else
                //truthPath.Add(new Vec3(1, 0, 0));
                //truthPath.Add(new Vec3(0, 0, 1));
                //truthPath.Add(new Vec3(0, 1, 1));

                var (w, h) = (1.0, 2.0);
                truthPath.Add(new Vec3( w, h, 0));
                truthPath.Add(new Vec3(-w, h, 0));
                truthPath.Add(new Vec3(-w,-h, 0));
                truthPath.Add(new Vec3( w,-h, 0));
//                truthPath.Add(new Vec3(9,4,5));

#endif
#endif

                var rotationValue = r.NextDouble() * Math.PI * 2 - Math.PI;
                var rotationDirection = Vec3.SphericalRandom(r.NextDouble(), r.NextDouble());

                var scaleValue = r.NextDouble() * 3 + 0.01;
                var transValue = new Vec3(v(), v(), v());

                // todo - remove
                //scaleValue = 1.0;
                //transValue = Vec3.Zero;
                //rotationValue = Math.PI/2;
                //rotationDirection = (Vec3.YAxis+Vec3.XAxis).Normalized();
                //rotationDirection = Vec3.ZAxis;

                var scm = Mat4.Scale(scaleValue);
                var tm = Mat4.Translation(transValue);
                var rotation = Quat.FromRotationVector(rotationValue * rotationDirection);
                var rotm = rotation.ToRotationMatrix4();
                var m = tm * scm * rotm;

                var movedPath = truthPath.Select(v => m * v).ToList();
                //var movedPath = truthPath.Select(p => scaleValue * rotation.Rotate(p) + transValue).ToList();

                var (foundScale, foundRotation, foundTrans) = OrientPoints3D(truthPath, movedPath);

                var computedPath = truthPath.Select(p => foundScale * foundRotation.Rotate(p) + foundTrans).ToList();

                var err = Enumerable.Range(0, computedPath.Count).Sum(i => (computedPath[i] - movedPath[i]).Length) / computedPath.Count;
                maxErr = Math.Max(err, maxErr);

                if (Double.IsNaN(err) || err > 1e-6)
                {
                    Console.WriteLine($"Orient 3D error {pass}  :  {err:F3}");
                    Console.WriteLine($"   rotation {NiceRot(rotation):F4} => {NiceRot(foundRotation)}");
                    Console.WriteLine($"   trans    {transValue} => {foundTrans}");
                    Console.WriteLine($"   scale    {scaleValue:F4} => {foundScale:F4}");
                    ++errCount;
                }
                //break; // todo - remove
            }
            Console.WriteLine($"{errCount} total 3D orientation errors, max error {maxErr:E3}");
            return errCount == 0;
        }

        /// <summary>
        /// Format quat as rotation vector
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        static string NiceRot(Quat q)
        {
            var (axis, angle) = q.ToAxisAngle();
            var (x, y, z) = axis;
            return $"{angle:F2}x({x:F2},{y:F2},{z:F2})";

        }


        public static bool TestOrientation2D()
        {
            var errCount = 0;
            var r = new Random(12345);
            var size = 10_000.0;
            var maxErr = 0.0;
            Func<double> v = () => r.NextDouble() * 2 * size - size;
            for (var pass = 0; pass < 10_000; ++pass)

            {
                var len = r.Next(100, 5000); // todo - larger
                var truthPath = new List<Vec3>();
                for (var i = 0; i < len; ++i)
                    truthPath.Add(new Vec3(v(), v(), 0)); // planar

                var rotationValue = r.NextDouble() * Math.PI * 2 - Math.PI;
                var scaleValue = r.NextDouble() * 3 + 0.01;
                var transValue = new Vec3(v(), v(), 0); // z= 0


                // todo - remove block
                //transValue = new Vec3();
                //rotationValue= 3*Math.PI/2;
                //rotationValue = 0;
                //scaleValue = 1.0;

                //var rot = ;
                //var trans = Mat4.Translation(tr); // 2D

                var rm = Mat4.ZRotation(rotationValue);

                var movedPath = truthPath.Select(p => scaleValue * (rm * p) + transValue).ToList();

                var (foundScale, foundRotation, trans1_2) = OrientPoints2D(ToV2(truthPath), ToV2(movedPath));
                var foundTrans = new Vec3(trans1_2.X, trans1_2.Y, 0);

                var fm = Mat4.ZRotation(foundRotation);
                var computedPath = truthPath.Select(p => foundScale * (fm * p) + foundTrans).ToList();


                var err = Enumerable.Range(0, computedPath.Count).Sum(i => (computedPath[i] - movedPath[i]).Length) / computedPath.Count;
                maxErr = Math.Max(err, maxErr);

                if (Double.IsNaN(err) || err > 1e-6)
                {
                    Console.WriteLine($"Orient 2D error {pass}  :  {err:F3}");
                    Console.WriteLine($"   rotation {rotationValue:F4} => {foundRotation:F4}");
                    Console.WriteLine($"   trans    {transValue} => {foundTrans}");
                    Console.WriteLine($"   scale    {scaleValue:F4} => {foundScale:F4}");
                    ++errCount;
                }

                //if (pass > 5) break; // todo - remove
            }

            Console.WriteLine($"{errCount} total 2D orientation errors, max error {maxErr:E3}");
            return errCount == 0;

        }
        public static List<Vec2> ToV2(List<Vec3> v) => v.Select(p => new Vec2(p.X, p.Y)).ToList();

        #region Helpers
        // get 4 coeffs for characteristic equation from nonsingular symmetric matrix m
        // x^4 + a x^3 + b x^2 + c x + d = 0
        static void GetCharPolyCoeffs(Mat4 m, double[] c)
        {
            // squares
            double m01_2 = m[0, 1] * m[0, 1];
            double m02_2 = m[0, 2] * m[0, 2];
            double m03_2 = m[0, 3] * m[0, 3];
            double m12_2 = m[1, 2] * m[1, 2];
            double m13_2 = m[1, 3] * m[1, 3];
            double m23_2 = m[2, 3] * m[2, 3];

            // mixed
            double m0011 = m[0, 0] * m[1, 1];
            double m0022 = m[0, 0] * m[2, 2];
            double m0033 = m[0, 0] * m[3, 3];
            double m0102 = m[0, 1] * m[0, 2];
            double m0103 = m[0, 1] * m[0, 3];
            double m0223 = m[0, 2] * m[2, 3];
            double m1122 = m[1, 1] * m[2, 2];
            double m1133 = m[1, 1] * m[3, 3];
            double m1223 = m[1, 2] * m[2, 3];
            double m2233 = m[2, 2] * m[3, 3];

            // a
            c[0] = -m.Trace;

            // b
            c[1] = -m01_2 - m02_2 - m03_2 + m0011 - m12_2 -
              m13_2 + m0022 + m1122 - m23_2 + m0033 + m1133 +
              m2233;

            // c
            c[2] = (m02_2 + m03_2 + m23_2) * m[1, 1] - 2 * m0102 * m[1, 2] +
              (m12_2 + m13_2 + m23_2) * m[0, 0] +
              (m01_2 + m03_2 - m0011 + m13_2 - m1133) * m[2, 2] -
              2 * m[0, 3] * m0223 - 2 * (m0103 + m1223) * m[1, 3] +
              (m01_2 + m02_2 - m0011 + m12_2 - m0022) * m[3, 3];

            // d
            c[3] = 2 * (-m[0, 2] * m[0, 3] * m[1, 2] + m0103 * m[2, 2] -
                  m[0, 1] * m0223 + m[0, 0] * m1223) * m[1, 3] +
              m02_2 * m13_2 - m03_2 * m1122 - m13_2 * m0022 +
              2 * m[0, 3] * m[1, 1] * m0223 - 2 * m0103 * m1223 + m01_2 * m23_2 -
              m0011 * m23_2 - m02_2 * m1133 + m03_2 * m12_2 +
              2 * m0102 * m[1, 2] * m[3, 3] - m12_2 * m0033 - m01_2 * m2233 +
              m0011 * m2233;
        }

        static void CheckUnique(Complex[] roots)
        {
            for (var i = 0; i < roots.Length; ++i)
                for (var j = i + 1; j < roots.Length; ++j)
                    if ((roots[i] - roots[j]).Magnitude < 1e-4)
                    {
                        Console.WriteLine("Error - duplicate roots!");
                    }
        }

        // calculate the maximum eigenvector of a symmetric 4x4 matrix
        // See "Closed-form solution of absolute orientation using unit quaternions," Horn, 1986
        static Quat GetQuatFromMaxEigenvector(Mat4 N1)
        {
            // todo - trying to avoid overflows, ok to scale here, seems to work ok
            var N = N1 / (N1.MaxNorm() / 10);

            var rts = new double[4];
            var c = new double[4];
            GetCharPolyCoeffs(N, c);

            // solve quartic
            var roots = new Complex[4];
            var type = SmallDegreePolynomialRoots.
                SolveQuartic(
                1.0, c[0], c[1], c[2], c[3],
                roots
                );
            // todo - roots better be unique? Else what happens?
            CheckUnique(roots);
            Trace.Assert(type == 0); // all roots should be real
            var largestEigenvalue = roots.Max(c => c.Real);

            // create the N - largestEigenvalue*I matrix, find eigen vector
            var Nm = N - largestEigenvalue * Mat4.Identity;


            // cofactors method:
            var cf = Mat4.Cofactors(Nm);

            // pick biggest row
            var br = 0;
            var bestSum = 0.0;
            for (var row = 0; row < 4; ++row)
            {
                var sum = 0.0;
                for (var col = 0; col < 4; ++col)
                    sum += cf[row, col] * cf[row, col];
                if (sum > bestSum)
                {
                    bestSum = sum;
                    br = row;
                }
            }
            Trace.Assert(bestSum < 1e20); // watch for bad cases
            var q = new Quat(cf[br, 0], cf[br, 1], cf[br, 2], cf[br, 3]);
            return q.Normalized();
        }
        #endregion

    }
}
