using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Lomont.Numerical;

namespace Lomont.Geometry
{
    public static class ExtensionMethods
    {
        public static double Distance(double a, double b) => Math.Abs(a - b);
    }

#if false
    public class BSpline1 : BSplineT<double>
    {
        public BSpline1(int degree, IList<double> points, double[] knots)
            : base(degree, points, knots)
        {
        }
        public BSpline1(BSpline1 bspline) : base(bspline)
        {
        }
    }
#endif

    public class BSpline2 : BSplineT<Vec2>
    {
        public BSpline2(int degree, IList<Vec2> points, double[] knots)
            : base(degree, points, knots)
        {
        }
        public BSpline2(BSpline2 bspline) : base(bspline)
        {
        }
    }
    public class BSpline3 : BSplineT<Vec3>
    {
        public BSpline3(int degree, IList<Vec3> points, double[] knots)
            : base(degree, points, knots)
        {
        }
        public BSpline3(BSpline3 bspline) : base(bspline)
        {
        }
    }


    /// <summary>
    /// Represent a B-Spline curve
    /// </summary>
    public class BSplineT<T>
        where T :
        IAdditiveIdentity<T, T>,
        IAdditionOperators<T, T, T>,
        ISubtractionOperators<T, T, T>,
        IMultiplyOperators<T, double, T>,
        IDistance<T, T, double>
    {
        /* Notation from Piegl and Tiller book, "The NURBS Book"

        U={u_0, u_1, ..., u_m} nondecreasing seq of real numbers
        u_i called knots, U is the knot vector

        p = degree of basis functions
        n+1 control points P0,..Pn

        Must have m = n+p+1

         */

        /// <summary>
        /// BSpline curve. Must have number of knots = number of points + degree + 1.
        /// Note n=p gives Bezier curve (TODO - explain)
        /// </summary>
        /// <param name="degree"></param>
        /// <param name="points"></param>
        /// <param name="knots"></param>
        public BSplineT(int degree, IList<T> points, double[] knots)
        {
            p = degree;
            P = points;
            U = knots;
            // m = U.Length - 1;
            // n = P.Count - 1;
            Trace.Assert(m == n + p + 1);
        }
        public BSplineT(BSplineT<T> bspline) : this(bspline.p, bspline.P, bspline.U)
        {
        }

        public int Degree => p;
        public double[] Knots => U;
        public IList<T> Points => P;

        #region Fields
        /// <summary>
        /// Knot vector, nondecr seq of m+1 real numbers
        /// </summary>
        private double[] U;

        /// <summary>
        /// m+1 knots in U
        /// </summary>
        private int m => U.Length - 1;

        /// <summary>
        /// basis degree, often 2 for quadratic or 3 for cubic
        /// </summary>
        private int p;

        /// <summary>
        /// Control points
        /// </summary>
        private IList<T> P;

        /// <summary>
        /// Number of control points - 1
        /// </summary>
        private int n => P.Count - 1;

        #endregion

        static int KnotMultiplicity(int n, int p, double u, double[] U)
        {
            var span = FindSpan(n, p, u, U);

            int up = 1, down = 0;
            while (u == U[span - down])
                ++down;
            while (u == U[span + up])
                ++up;
            return up + down - 1;
        }

        /// <summary>
        /// Determine span index where u fits in in U
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <param name="u"></param>
        /// <param name="U"></param>
        /// <returns></returns>
        static int FindSpan(int n, int p, double u, double[] U)
        {
            // Algorithm A2.1
            if (u == U[n + 1]) return n;
            var (low, high) = (p, n + 1);
            var mid = (low + high) / 2;
            while (u < U[mid] || u >= U[mid + 1])
            {
                if (u < U[mid]) high = mid;
                else low = mid;
                mid = (low + high) / 2;
            }

            return mid;
        }

        /// <summary>
        /// Store all non-vanishing basis funcs in N[0],...,N[p]
        /// </summary>
        /// <param name="i"></param>
        /// <param name="u"></param>
        /// <param name="p"></param>
        /// <param name="U"></param>
        /// <param name="N"></param>
        static void BasisFuns(int i, double u, int p, double[] U, double[] N)
        {
            // Algorithm A2.2
            // todo - array sizes correct?
            var left = new double[p + 1]; // todo - make global or passed in - do not allocate this low
            var right = new double[p + 1]; // todo - make global or passed in - do not allocate this low
            N[0] = 1.0;
            for (var j = 1; j <= p; ++j)
            {
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                var saved = 0.0;
                for (var r = 0; r < j; ++r)
                {
                    var temp = N[r] / (right[r + 1] + left[j - r]);
                    N[r] = saved + right[r + 1] * temp;
                    saved = left[j - r] * temp;
                }

                N[j] = saved;
            }
        }

        // get N[j,i] where N[j,i] is the value of ith basis func N_{span-i+j,i}(u) for 0<=i<=p and 0<=j<=i
        static void AllBasisFuns(int i, double u, int p, double[] U, double[,] N)
        { // mentioned but not defined in Algorithm, A3.4
            var N1 = new double[p + 1]; // todo - remove local allocs
            var span = i;
            for (i = 0; i <= p; ++i)
            {
                BasisFuns(span - i, u, p, U, N1); // N[i], ..., N[i+p]
                for (var j = 0; j <= i; ++j)
                    N[j, i] = N1[j];
            }
        }

        static void DersBasisFuns(int i, double u, int p, int n, double[] U, double[,] ders)
        {
            // Algorithm A2.3
            double[,] ndu = new double[p + 1, p + 1]; // todo - remove local allocs
            double[] left = new double[p + 1], right = new double[p + 1]; // todo - avoid local allocs?
            double[,] a = new double[2, p + 1]; // todo - avoid local allocs

            ndu[0, 0] = 1.0;

            for (var j = 1; j <= p; ++j)
            {
                left[j] = u - U[i + 1 - j];
                right[j] = U[i + j] - u;
                var saved = 0.0;
                for (var r = 0; r < j; ++r)
                {
                    // lower triangle
                    ndu[j, r] = right[r + 1] + left[j - r];
                    var temp = ndu[r, j - 1] / ndu[j, r];
                    // upper triangle
                    ndu[r, j] = saved + right[r + 1] * temp;
                    saved = left[j - r] * temp;
                }

                ndu[j, j] = saved;
            }

            // load basis functions
            for (var j = 0; j <= p; ++j)
                ders[0, j] = ndu[j, p];

            for (var r = 0; r <= p; ++r)
            {
                var (s1, s2) = (0, 1); // alternate rows
                a[0, 0] = 1.0;
                for (var k = 1; k <= n; ++k) // compute kth derivative
                {
                    var d = 0.0;
                    var rk = r - k;
                    var pk = p - k;
                    if (r >= k)
                    {
                        a[s2, 0] = a[s1, 0] / ndu[pk + 1, rk];
                        d = a[s2, 0] * ndu[rk, pk];
                    }

                    var j1 = 0;
                    if (rk >= -1)
                        j1 = 1;
                    else
                        j1 = -rk;
                    var j2 = 0;
                    if (r - 1 <= pk)
                        j2 = k - 1;
                    else
                        j2 = p - r;
                    for (var j = j1; j <= j2; ++j)
                    {
                        a[s2, j] = (a[s1, j] - a[s1, j - 1]) / ndu[pk + 1, rk + j];
                        d += a[s2, j] * ndu[rk + j, pk];
                    }

                    if (r <= pk)
                    {
                        a[s2, k] = -a[s1, k - 1] / ndu[pk + 1, r];
                        d += a[s2, k] * ndu[r, pk];
                    }

                    ders[k, r] = d;
                    var jt = s1;
                    s1 = s2;
                    s2 = jt;
                }
            }

            var r1 = p;
            for (var k = 1; k <= n; ++k)
            {
                for (var j = 0; j <= p; ++j) ders[k, j] *= r1;
                r1 *= (p - k);
            }
        }



        static void OneBasisFun(int p, int m, double[] U, int i, double u, out double Nip)
        { // Algorithm A2.4
            var N = new double[p]; // todo - remove local allocs, todo - array size?

            if ((i == 0 && u == U[0]) ||
                (i == m - p - 1 && u == U[m]))
            {
                Nip = 1.0;
                return;
            }

            if (u < U[i] || u >= U[i + p + 1])
            {
                Nip = 0.0;
                return;
            }

            // 0th degree search funcs
            for (var j = 0; j <= p; ++j)
            {
                if (u >= U[i + 1] && u < U[i + j + 1])
                    N[j] = 1.0;
                else
                    N[j] = 0.0;
            }

            double saved = 0;
            for (var k = 1; k <= p; ++k)
            {
                if (N[0] == 0.0)
                    saved = 0.0;
                else
                    saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);
                for (var j = 0; j < p - k + 1; ++j)
                {
                    var ULeft = U[i + j + 1];
                    var URight = U[i + j + k + 1];
                    if (N[j + 1] == 0.0)
                    {
                        N[j] = saved;
                        saved = 0.0;
                    }
                    else
                    {
                        var temp = N[j + 1] / (URight - ULeft);
                        N[j] = saved + (URight - u) * temp;
                        saved = (u - ULeft) * temp;
                    }
                }
            }
            Nip = N[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="m"></param>
        /// <param name="U"></param>
        /// <param name="i"></param>
        /// <param name="u"></param>
        /// <param name="n"></param>
        /// <param name="ders"></param>
        static void DerOneBasisFun(int p, int m, double[] U, int i, double u, int n, double[] ders)
        { // Algorithm A2.5
            if (u < U[i] || u >= U[i + p + 1])
            {
                for (var k = 0; k <= n; ++k)
                    ders[k] = 0;
                return;
            }

            double[,] N = new double[p + 1, p + 1]; // todo - remove local allocs
            double[] ND = new double[p + 1]; // todo - remove local allocs

            for (var j = 0; j <= p; ++j)
            {
                if (u >= U[i + j] && u < U[i + j + 1])
                    N[j, 0] = 1.0;
                else
                    N[j, 0] = 0.0;
            }

            double saved = 0.0;
            for (var k = 0; k <= p; ++k)
            {
                if (N[0, k - 1] == 0.0)
                    saved = 0.0;
                else
                    saved = ((u - U[i]) * N[0, k - 1]) / ((U[i + k] - U[i]));
                for (var j = 0; j < p + k + 1; ++j)
                {
                    var ULeft = U[i + j + 1];
                    var URight = U[i + j + k + 1];
                    if (N[j + 1, k - 1] == 0.0)
                    {
                        N[j, k] = saved;
                        saved = 0.0;
                    }
                    else
                    {
                        var temp = N[j + 1, k - 1] / (URight - ULeft);
                        N[j, k] = saved + (URight - u) * temp;
                        saved = (u - ULeft) * temp;
                    }
                }
            }


            ders[0] = N[0, p]; // func value
            for (var k = 1; k <= n; ++k)
            {
                for (var j = 0; j <= k; ++j)
                    ND[j] = N[j, p - k];
                for (var jj = 0; jj <= k; ++jj)
                {
                    if (ND[0] == 0.0)
                        saved = 0.0;
                    else
                        saved = ND[0] / (U[i + p - k + jj] - U[i]);
                    for (var j = 0; j < k - jj + 1; ++j)
                    {
                        var ULeft = U[i + j + 1];
                        var URight = U[i + j + p + jj + 1];
                        if (ND[j + 1] == 0.0)
                        {
                            ND[j] = (p - k + jj) * saved;
                            saved = 0.0;
                        }
                        else
                        {
                            var temp = ND[j + 1] / (URight - ULeft);
                            ND[j] = (p - k + jj) * (saved - temp);
                            saved = temp;
                        }
                    }
                }

                ders[k] = ND[0];
            }
        }

        /// <summary>
        /// Compute curve point
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <param name="U"></param>
        /// <param name="P"></param>
        /// <param name="u"></param>
        /// <param name="C"></param>
        static void CurvePoint(int n, int p, double[] U, IList<T> P, double u, out T C)
        { // Algorithm A3.1
            var N = new double[p + 1]; // todo - remove local allocations?, todo - size?
            var span = FindSpan(n, p, u, U);
            BasisFuns(span, u, p, U, N);
            C = T.AdditiveIdentity; // zero
            for (var i = 0; i <= p; ++i)
                C += P[span - p + i] * N[i];
        }

        /// <summary>
        /// Get curve derivatives, kth derivative in CK[k]
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <param name="U"></param>
        /// <param name="P"></param>
        /// <param name="u"></param>
        /// <param name="d"></param>
        /// <param name="CK"></param>
        static void CurveDerivsAlg1(int n, int p, double[] U, IList<T> P, double u, int d, T[] CK)
        { // Algorithm A3.2
            var du = Math.Min(d, p);
            for (var k = p + 1; k <= d; ++k)
                CK[k] = T.AdditiveIdentity; // zero
            var span = FindSpan(n, p, u, U);
            var nders = new double[p + 1, p + 1]; // todo - remove local allocs
            DersBasisFuns(span, u, p, du, U, nders);
            for (var k = 0; k <= du; ++k)
            {
                CK[k] = T.AdditiveIdentity; // zero
                for (var j = 0; j <= p; ++j)
                    CK[k] = CK[k] + P[span - p + j] * nders[k, j];
            }
        }

        /// <summary>
        /// Compute control points of curve derivatives
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p"></param>
        /// <param name="U"></param>
        /// <param name="P"></param>
        /// <param name="d"></param>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <param name="PK"></param>
        static void CurveDerivCpts(int n, int p, double[] U, IList<T> P, int d, int r1, int r2, T[,] PK)
        { // Algorithm A3.3
            var r = r2 - r1;
            for (var i = 0; i <= r; ++i)
                PK[0, i] = P[r1 + i];
            for (var k = 1; k <= d; ++k)
            {
                var tmp = p - k + 1;
                for (var i = 0; i <= r - k; ++i)
                    PK[k, i] = (PK[k - 1, i + 1] - PK[k - 1, i]) * (tmp / (U[r1 + i + p + 1] - U[r1 + i + k]));
            }
        }

        /// <summary>
        /// Compute derivatives up to d-th on curve at parameter u
        /// 0th derivative is simply curve value at that point
        /// </summary>
        static void CurveDerivsAlg2(int n, int p, double[] U, IList<T> P, double u, int d, T[] CK)
        {
            // Algorithm A3.4 - does same as Algorithm A3.2?
            var du = Math.Min(d, p);
            for (var k = p + 1; k <= d; ++k) CK[k] = T.AdditiveIdentity; // zero
            var span = FindSpan(n, p, u, U);
            var N = new double[p + 1, p + 1]; // todo - remove local allocs
            var PK = new T[p + 1, p + 1]; // todo - remove local allocs
            AllBasisFuns(span, u, p, U, N);
            CurveDerivCpts(n, p, U, P, du, span - p, span, PK);
            for (var k = 0; k <= du; ++k)
            {
                CK[k] = T.AdditiveIdentity; // zero
                for (var j = 0; j <= p - k; ++j)
                {
                    CK[k] = CK[k] + PK[k, j] * N[j, p - k];
                }
            }
        }


        /// <summary>
        /// Insert a new knot into the curve
        /// </summary>
        /// <param name="np"></param>
        /// <param name="p">degree</param>
        /// <param name="UP">Old knots</param>
        /// <param name="Pw">Old points</param>
        /// <param name="u">new knot value</param>
        /// <param name="k">Index where to insert</param>
        /// <param name="s">Initial knot multiplicity</param>
        /// <param name="r">knot insert multiplicity</param>
        /// <param name="nq">new </param>
        /// <param name="UQ">new knots</param>
        /// <param name="Qw">New points</param>
        static void CurveKnotIns(int np, int p, double[] UP, IList<T> Pw, double u, int k, int s, int r, out int nq, double[] UQ, List<T> Qw)
        {
            // algorithm A5.1
            var mp = np + p + 1;
            nq = np + r;

            var Rw = new T[p + 1]; // todo - remove local allocs

            // load new knot vector
            for (var i = 0; i <= k; ++i) UQ[i] = UP[i];
            for (var i = 1; i <= r; ++i) UQ[k + i] = u;
            for (var i = k + 1; i <= mp; ++i) UQ[i + r] = UP[i];

            // unaltered control points
            for (var i = 0; i <= k - p; ++i) Qw[i] = Pw[i];
            for (var i = k - s; i <= np; ++i) Qw[i + r] = Pw[i];
            for (var i = 0; i <= p - s; ++i) Rw[i] = Pw[k - p + i];

            int L = 0;
            for (var j = 1; j <= r; ++j) // insert r times
            {
                L = k - p + j;
                for (var i = 0; i <= p - j - s; ++i)
                {
                    var alpha = (u - UP[L + i]) / (UP[i + k + 1] - UP[L + i]);
                    Rw[i] = Rw[i + 1] * alpha + Rw[i] * (1.0 - alpha);
                }
                Qw[L] = Rw[0];
                Qw[k + r - j - s] = Rw[p - j - s];
            }
            for (var i = L + 1; i < k - s; ++i) // remaining points
                Qw[i] = Rw[i - L];

        }

        /// <summary>
        /// Remove knot u (index r) num times
        /// Modifies knot vector U and points list Pw
        /// </summary>
        /// <param name="n"></param>
        /// <param name="p">degree</param>
        /// <param name="U">knots</param>
        /// <param name="Pw">points</param>
        /// <param name="u">knot parameter</param>
        /// <param name="r">Knot index</param>
        /// <param name="s">Initial knot multiplicity</param>
        /// <param name="num">times to remove</param>
        /// <param name="t">Number of knots removed</param>
        static void RemoveCurveKnot(int n, int p, double[] U, IList<T> Pw, double u, int r, int s, int num, out int t)
        { // algorithm A5.8
            int knotsRemovedCount = 0;
            var TOL = 1e-5; // see book
            var temp = new T[2 * p + 1]; // todo - remove local allocs
            var m = n + p + 1;
            var ord = p + 1;
            var fout = (2 * r - s - p) / 2; // first ctrl point out
            var last = r - s;
            var first = r - p;
            int i, j; // used throughout
            for (t = 0; t < num; ++t)
            {
                var off = first - 1; // diff in index between temp and P
                temp[0] = Pw[off];
                temp[last + 1 - off] = Pw[last + 1];
                i = first;
                j = last;
                int ii = 1, jj = last - off;
                var remflag = 0;
                while (j - i > t)
                { // new control points for one removal
                    var alfi = (u - U[i]) / (U[i + ord + t] - U[i]);
                    var alfj = (u - U[j - t]) / (U[j + ord] - U[j - t]);
                    temp[ii] = (Pw[i] - temp[ii - 1] * (1.0 - alfi)) * (1.0 / alfi);
                    temp[jj] = (Pw[j] - temp[jj + 1] * alfj) * (1.0 / (1.0 - alfj));
                    i = i + 1;
                    ii = ii + 1;
                    j = j - 1;
                    jj = jj - 1;
                } // end of while loop
                if (j - i < t) // check if knot removable
                {
                    if (Distance4D(temp[ii - 1], temp[jj + 1]) <= TOL)
                        remflag = 1;
                }
                else
                {
                    var alfi = (u - U[i]) / (U[i + ord + t] - U[i]);
                    if (Distance4D(Pw[i], temp[ii + t + 1] * alfi + temp[ii - 1] * (1.0 - alfi)) <= TOL)
                        remflag = 1;
                }
                if (remflag == 0) // cannot remove knots
                    break; // out of for loop
                else
                { // successful removal, save new cont. pts.
                    knotsRemovedCount += 1;
                    i = first;
                    j = last;
                    while (j - i > t)
                    {
                        Pw[i] = temp[i - off];
                        Pw[j] = temp[j - off];
                        i = i + 1;
                        j = j - 1;
                    }
                }
                first = first - 1;
                last = last + 1;
            } // end of for loop
            if (t == 0) return;
            for (var k = r + 1; k <= m; ++k) U[k - t] = U[k]; // shift knots
            j = fout;  // Pi through Pj will be overwritten
            i = j;
            for (var k = 1; k < t; ++k)
            {
                if ((k & 1) == 1) // k mod 2
                    i = i + 1;
                else
                    j = j - 1;
            }
            for (var k = i + 1; k <= n; ++k)
            {
                Pw[j] = Pw[k];
                j = j + 1;
            }
            // dist 4d is the non-homogeneous dist for nurbs, etc..
            static double Distance4D(T a, T b) => T.Distance(a, b);

        }

        /// <summary>
        /// Get all derivatives at point
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public List<T> CurveDerivatives(double u)
        {
            var CK = new T[p + 1];
            CurveDerivsAlg1(n, p, U, P, u, p, CK);
            return CK.ToList();
        }

        /// <summary>
        /// Evaluate curve at parameter
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        public T CurvePoint(double u)
        {
            CurvePoint(n, p, U, P, u, out var C);
            return C;
        }

        /// <summary>
        /// Insert knot at parameter u 
        /// Insert it k times
        /// 
        /// Useful to split curve at a point (insert degree % of knots)
        /// Useful to evaluate at a point
        /// Useful to add control points for interactive use
        /// </summary>
        public void InsertKnot(double parameter, int multiplicity = 1)
        {
            var knot_index = FindSpan(n, p, parameter, U);
            var old_mult = KnotMultiplicity(n, p, parameter, U);

            double[] UQ = new double[U.Length + multiplicity]; // new knots
            List<T> Qw = new List<T>(P); // new control points
            for (var i = 0; i < multiplicity; ++i)
                Qw.Add(P[0]); // add some new space

            CurveKnotIns(n, p, U, P, parameter, knot_index, old_mult, multiplicity, out var nq, UQ, Qw);

            U = UQ; // new knots
            P = Qw; // new points

            Trace.Assert(n == nq);
        }

        /// <summary>
        /// Remove a knot at the given parameter, remove with the given multiplicity
        /// If only lesser multiplicity possible, remove up to that one
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="multiplicity"></param>
        /// <returns>The multiplicity of the knot that could be removed before removal</returns>
        public int RemoveKnot(double parameter, int multiplicity = 1)
        {
            var knot_index = FindSpan(n, p, parameter, U);

            // find first (todo - move to span?)
            //while (knot_index>1 &&  U[knot_index-1] == parameter)
            //    knot_index--;

            var old_mult = KnotMultiplicity(n, p, parameter, U);

            RemoveCurveKnot(n, p, U, P, parameter, knot_index, old_mult, multiplicity, out var removed_count);

            if (removed_count > 0)
            {
                // shrink control points
                for (var i = 0; i < removed_count; ++i)
                    P.RemoveAt(P.Count - 1); // remove last
                // shrink knots
                var newU = new double[U.Length - removed_count];
                Array.Copy(U, newU, U.Length - removed_count);
                U = newU;
            }
            

            return removed_count;
        }



    }


}

