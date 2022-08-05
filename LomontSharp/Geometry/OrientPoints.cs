using Lomont.Numerical;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

            // centered points - todo - rewrite later to remove allocations
            var rlp = rl.Select(p => p - rlbar).ToList();
            var rrp = rr.Select(p => p - rrbar).ToList();

            // at this point, r0 = rrbar - sR(rlbar)

            // D., E. scale
            // todo - div/0 ?
            var s = Math.Sqrt(rrp.Sum(p => p.LengthSquared) / rlp.Sum(p => p.LengthSquared));

            // section 3. Rotations

            // cross covariance matrix M
            var zero = new Mat3();
            zero = zero - zero;
            var M = Enumerable.Range(0, n).Aggregate(zero, (cur, i) => cur + Vec3.Outer(src[i], dst[i]));
            M = s / n * M;
            M = M - s * Vec3.Outer(rlbar, rrbar);

            // calculate the 4x4 symmetric matrix Q
            var Q = new Mat4();

            var trace = M.Trace;

            Q[0, 0] = trace;
            Q[0, 1] = Q[1, 0] = M[1, 2] - M[2, 1];
            Q[0, 2] = Q[2, 0] = M[2, 0] - M[0, 2];
            Q[0, 3] = Q[3, 0] = M[0, 1] - M[1, 0];
            for (var i = 0; i < 3; i++)
                for (var j = 0; j < 3; j++)
                    Q[i + 1, j + 1] = M[i, j] + M[j, i] - (i == j ? trace : 0);

            // rotation from eigenvctor
            var rotation = GetQuatFromMaxEigenvector(Q);

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
        public static (double scale, double rotation, Vec3 translation) OrientPoints2D(List<Vec3> src, List<Vec3> dst)
        {
            // Horn method Berthold KP Horn. Closed-form solution of absolute orientation using unit quaternions. JOSA A, 4(4):629–642, 1987
            // special case from planar to planar

            var n = src.Count;

            var rl = src; // left in paper
            var rr = dst; // right in paper

            // find mapping of form rr = s*R(rl)+r0 where s is scale, R is rotation, r0 is shift

            // C. centroids
            // centroids
            var rlbar = rl.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / n;
            var rrbar = rr.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / n;

            // centered points - todo - rewrite later to remove allocations
            var rlp = rl.Select(p => p - rlbar).ToList();
            var rrp = rr.Select(p => p - rrbar).ToList();

            // at this point, r0 = rrbar - sR(rlbar)

            // D., E. scale
            // todo - div/0 ?
            var scale = Math.Sqrt(rrp.Sum(p => p.LengthSquared) / rlp.Sum(p => p.LengthSquared));


            //var cnt = src.Count;
            //var srcCentroid = src.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / cnt;
            //var dstCentroid = dst.Aggregate(new Vec3(), (cur, nxt) => cur + nxt) / cnt;

            //var srcScale = src.Max(p => (p - rlbar).Length);
            //var dstScale = dst.Max(p => (p - rrbar).Length);

            var S = 0.0;
            var C = 0.0;

            for (var i = 0; i < src.Count; ++i)
            {
                var r1 = dst[i] - rrbar;
                var r2 = src[i] - rlbar;
                C += Vec3.Dot(r2, r1);
                S += Vec3.Cross(r2, r1).Z; // (r1 cross r2) dot normal)
            }


            // sin, cos values
            var s = S / Math.Sqrt(C * C + S * S);
            var c = C / Math.Sqrt(C * C + S * S);

            var rotation = new Mat4(
                c, -s, 0, 0,
                s, c, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );

            var angle = -Math.Atan2(s, c);// Math.Acos(c);

            //var trans = rlbar - scale * (Mat4.ZRotation(angle) * rrbar);
            var trans = rrbar - scale * (Mat4.ZRotation(angle) * (rlbar));

            return (scale, angle, trans); // todo - scale
        }

        // todo - merge tests? relabel name of planar version
        public static bool TestOrientation3D()
        {
            var errCount = 0;
            var r = new Random(1234);
            var size = 1_000_000.0;
            var maxErr = 0.0;
            Func<double> v = () => r.NextDouble() * 2 * size - size;
            for (var i = 0; i < 1000; ++i)
            {
                var len = r.Next(50, 5000);
                var truthPath = new List<Vec3>();
                for (var j = 0; j < len; ++j)
                    truthPath.Add(new Vec3(v(), v(), v()));

                //src.Clear();
                //src.Add(new Vec3(1, 0, 0)); // todo - remove special points
                //src.Add(new Vec3(0, 1, 0));
                //src.Add(new Vec3(-1, 0, 0));
                //src.Add(new Vec3(0, -1, 0));
                //src.Add(new Vec3(1, 1, 1)); // NEED non planar!

                var rotationValue = r.NextDouble() * Math.PI;
                var scaleValue = r.NextDouble() * 3 + 0.01;
                var transValue = new Vec3(v(), v(), 0); // todo - add in z

                var rotm = Mat4.ZRotation(rotationValue); // todo - 3d rotations
                var scm = Mat4.Scale(scaleValue);
                var tm = Mat4.Translation(transValue);
                var m = tm * rotm * scm;

                var movedPath = truthPath.Select(v => m * v).ToList();

                var (foundScale, foundRotation, foundTrans) = OrientPoints3D(truthPath, movedPath);

                var computedPath = truthPath.Select(p => foundScale * foundRotation.Rotate(p) + foundTrans).ToList();

                var err = Enumerable.Range(0, computedPath.Count).Sum(i => (computedPath[i] - movedPath[i]).Length) / computedPath.Count;
                maxErr = Math.Max(err, maxErr);

                if (Double.IsNaN(err) || err > 1e-6)
                {
                    Console.WriteLine($"Orient error {i}  :  {err:F3}");
                    Console.WriteLine($"   rotation {rotationValue:F4} => {foundRotation}");
                    Console.WriteLine($"   trans    {transValue} => {foundTrans}");
                    Console.WriteLine($"   scale    {scaleValue:F4} => {foundScale:F4}");
                    ++errCount;
                }
            }
            Console.WriteLine($"{errCount} total 3D orientation errors, max error {maxErr:E3}");
            return errCount == 0;
        }



        public static void TestOrientation2D()
        {
            var r = new Random(5678);
            var size = 1000.0;
            Func<double> v = () => r.NextDouble() * size * 2 - size;
            var errCount = 0;
            var maxErr = 0.0;
            for (var pass = 0; pass < 200; ++pass)
            {
                var len = r.Next(100, 300);
                var truthPath = new List<Vec3>();
                for (var i = 0; i < len; ++i)
                    truthPath.Add(new Vec3(v(), v(), 0)); // planar

                var rotationValue = r.NextDouble() * Math.PI * 2 - Math.PI;
                var sc = r.NextDouble() * 3 - 0.01;

                var tr = new Vec3(v(), v(), 0);

                //todo - finish!
                //tr = new Vec3();
                //rotationValue = 3 * Math.PI / 2;
                //rotationValue = 0;
                //sc = 1.0;

                //var rot = ;
                //var trans = Mat4.Translation(tr); // 2D

                var rm = Mat4.ZRotation(rotationValue);

                var movedPath = truthPath.Select(p => sc * (rm * p) + tr).ToList();

                var (scale, foundRotation, trans1) = OrientPoints2D(movedPath, truthPath);
                //var fixTransform = Mat4.Translation(trans1) * Mat4.Scale(scale) * Mat4.ZRotation(foundRotation);
                var fm = Mat4.ZRotation(foundRotation);

                // todo - 1/scale here is weird....
                // - trans weird
                var computedPath = truthPath.Select(p => 1 / scale * (fm * p) - trans1).ToList();


                var err = Enumerable.Range(0, computedPath.Count).Sum(i => (computedPath[i] - movedPath[i]).Length) / computedPath.Count;
                maxErr = Math.Max(err, maxErr);

                if (Double.IsNaN(err) || err > 1e-6)
                {
                    Console.WriteLine($"Orient 2D error {pass}  :  {err:F3}");
                    Console.WriteLine($"   rotation {rotationValue:F4} => {foundRotation:F4}");
                    Console.WriteLine($"   trans    {tr} => {trans1}");
                    Console.WriteLine($"   scale    {sc:F4} => {scale:F4}");
                    ++errCount;
                }


                //Console.WriteLine($"{pass}: ATE {Metrics.ATE(truthPath, movedPath):F4} ATE {Metrics.ATE(truthPath, computedPath):F4}");
                //var err = 0.0;
                //if (err > 1e-4)
                //{
                //    errCount++;
                //}
                if (pass > 5) break; // todo remove
            }

            Console.WriteLine($"{errCount} total 2D orientation errors, max error {maxErr:E3}");
        }

        #region Helpers
        // get 4 coeffs for characteristic equation from nonsingular symmetric matrix m
        // x^4 + a x^3 + b x^2 + c x + d = 0
        static void GetCharPolyCoeffs(Mat4 m, double[] c)
        {
            // squares
            double q01_2 = m[0, 1] * m[0, 1];
            double q02_2 = m[0, 2] * m[0, 2];
            double q03_2 = m[0, 3] * m[0, 3];
            double q12_2 = m[1, 2] * m[1, 2];
            double q13_2 = m[1, 3] * m[1, 3];
            double q23_2 = m[2, 3] * m[2, 3];

            // mixed
            double q0011 = m[0, 0] * m[1, 1];
            double q0022 = m[0, 0] * m[2, 2];
            double q0033 = m[0, 0] * m[3, 3];
            double q0102 = m[0, 1] * m[0, 2];
            double q0103 = m[0, 1] * m[0, 3];
            double q0223 = m[0, 2] * m[2, 3];
            double q1122 = m[1, 1] * m[2, 2];
            double q1133 = m[1, 1] * m[3, 3];
            double q1223 = m[1, 2] * m[2, 3];
            double q2233 = m[2, 2] * m[3, 3];

            // a
            c[0] = -m.Trace;

            // b
            c[1] = -q01_2 - q02_2 - q03_2 + q0011 - q12_2 -
              q13_2 + q0022 + q1122 - q23_2 + q0033 + q1133 +
              q2233;

            // c
            c[2] = (q02_2 + q03_2 + q23_2) * m[1, 1] - 2 * q0102 * m[1, 2] +
              (q12_2 + q13_2 + q23_2) * m[0, 0] +
              (q01_2 + q03_2 - q0011 + q13_2 - q1133) * m[2, 2] -
              2 * m[0, 3] * q0223 - 2 * (q0103 + q1223) * m[1, 3] +
              (q01_2 + q02_2 - q0011 + q12_2 - q0022) * m[3, 3];

            // d
            c[3] = 2 * (-m[0, 2] * m[0, 3] * m[1, 2] + q0103 * m[2, 2] -
                  m[0, 1] * q0223 + m[0, 0] * q1223) * m[1, 3] +
              q02_2 * q13_2 - q03_2 * q1122 - q13_2 * q0022 +
              2 * m[0, 3] * m[1, 1] * q0223 - 2 * q0103 * q1223 + q01_2 * q23_2 -
              q0011 * q23_2 - q02_2 * q1133 + q03_2 * q12_2 +
              2 * q0102 * m[1, 2] * m[3, 3] - q12_2 * q0033 - q01_2 * q2233 +
              q0011 * q2233;
        }


        // calculate the maximum eigenvector of a symmetric 4x4 matrix
        // See "Closed-form solution of absolute orientation using unit quaternions," Horn, 1986
        static Quat GetQuatFromMaxEigenvector(Mat4 Q)
        {
            double[] rts = new double[4];
            double[] c = new double[4];
            GetCharPolyCoeffs(Q, c);

            // solve quartic
            var roots = new Complex[4];
            var type = SmallDegreePolynomialRoots.
                SolveQuartic(
                1.0, c[0], c[1], c[2], c[3],
                roots
                );
            Trace.Assert(type == 0); // all roots should be real
            var l = roots.Max(c => c.Real);

            // create the Q - l*I matrix
            var N = Q - l * Mat4.Identity;
            //var N = new double[4, 4];
            //N[0, 0] = Q[0, 0] - l; N[0, 1] = Q[0, 1]; N[0, 2] = Q[0, 2]; N[0, 3] = Q[0, 3];
            //N[1, 0] = Q[1, 0]; N[1, 1] = Q[1, 1] - l; N[1, 2] = Q[1, 2]; N[1, 3] = Q[1, 3];
            //N[2, 0] = Q[2, 0]; N[2, 1] = Q[2, 1]; N[2, 2] = Q[2, 2] - l; N[2, 3] = Q[2, 3];
            //N[3, 0] = Q[3, 0]; N[3, 1] = Q[3, 1]; N[3, 2] = Q[3, 2]; N[3, 3] = Q[3, 3] - l;

            // the columns of the inverted matrix should be multiples of
            // the eigenvector, pick the largest

            var ipiv = new int[4];
            var best = new double[4];
            var curr = new double[4];

            if (!Matrix.FactorLU(N.Values, ipiv))
            {
                Trace.TraceWarning("Max eigenvector failed, returning identity quaternion");
                return new Quat(); // identity
            }
            //best = 0; 
            best[0] = 1;
            Matrix.SolveLU(N.Values, ipiv, best);
            double len =
              best[0] * best[0] + best[1] * best[1] +
              best[2] * best[2] + best[3] * best[3];
            for (int i = 1; i < 4; i++)
            {
                curr[0] = curr[1] = curr[2] = curr[3] = 0;
                //curr = 0; 
                curr[i] = 1;
                Matrix.SolveLU(N.Values, ipiv, curr);
                double tlen =
                  curr[0] * curr[0] + curr[1] * curr[1] +
                  curr[2] * curr[2] + curr[3] * curr[3];
                if (tlen > len) { len = tlen; best = curr; }
            }
            // normalize the result
            len = 1.0 / Math.Sqrt(len);
            return new Quat(
                best[0] * len,
                best[1] * len,
                best[2] * len,
                best[3] * len
                );
        }
        #endregion

    }
}
