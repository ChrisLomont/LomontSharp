using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Numerical
{
    /// <summary>Singular Value Decomposition (SVD) for a rectangular matrix.</summary>
    /// <remarks>
    /// For m x n matrix A, thew singular value decomposition is matrixes U, S, and V
    /// with U and V are are othogonal, and S is diagonal, with non-negative values
    /// S = diag(s1,s2,,,,sN) with s1>=s2>=s3....
    /// The singular value decompostion always exists, so the constructor will
    /// never fail. The matrix condition number and the effective numerical
    /// rank can be computed from this decomposition.
    /// 
    /// Adapted from Mapack library
    /// 
    /// </remarks>
    public class SingularValueDecomposition
    {
        public Matrix U { get; }
        public Matrix V { get;  }
        /// <summary>the one-dimensional array of singular values.</summary>     
        public double[] S { get;  } // singular values
        

        private int m;
        private int n;

        /// <summary>Construct singular value decomposition.</summary>
        public SingularValueDecomposition(Matrix matrix)
        {
            if (matrix == null)
                throw new ArgumentNullException("matrix");

            m = matrix.Rows;
            n = matrix.Columns;

            double[,] a = matrix.Values.Clone() as double[,];
            int nu = Math.Min(m, n);
            S = new double[Math.Min(m + 1, n)];
            U = new Matrix(m, nu);
            V = new Matrix(n, n);
            double[,] u = U.Values;
            double[,] v = V.Values;
            double[] e = new double[n];
            double[] work = new double[m];
            bool wantu = true;
            bool wantv = true;

            // Reduce A to bidiagonal form, storing the diagonal elements in s and the super-diagonal elements in e.
            int nct = Math.Min(m - 1, n);
            int nrt = Math.Max(0, Math.Min(n - 2, m));
            for (int k = 0; k < Math.Max(nct, nrt); k++)
            {
                if (k < nct)
                {
                    // Compute the transformation for the k-th column and place the k-th diagonal in s[k].
                    // Compute 2-norm of k-th column without under/overflow.
                    S[k] = 0;
                    for (int i = k; i < m; i++)
                    {
                        S[k] = Hypotenuse(S[k], a[i,k]);
                    }

                    if (S[k] != 0.0)
                    {
                        if (a[k,k] < 0.0)
                        {
                            S[k] = -S[k];
                        }

                        for (int i = k; i < m; i++)
                        {
                            a[i,k] /= S[k];
                        }

                        a[k,k] += 1.0;
                    }

                    S[k] = -S[k];
                }

                for (int j = k + 1; j < n; j++)
                {
                    if ((k < nct) & (S[k] != 0.0))
                    {
                        // Apply the transformation.
                        double t = 0;
                        for (int i = k; i < m; i++)
                            t += a[i,k] * a[i,j];
                        t = -t / a[k,k];
                        for (int i = k; i < m; i++)
                            a[i,j] += t * a[i,k];
                    }

                    // Place the k-th row of A into e for the subsequent calculation of the row transformation.
                    e[j] = a[k,j];
                }

                if (wantu & (k < nct))
                {
                    // Place the transformation in U for subsequent back
                    // multiplication.
                    for (int i = k; i < m; i++)
                        u[i,k] = a[i,k];
                }

                if (k < nrt)
                {
                    // Compute the k-th row transformation and place the k-th super-diagonal in e[k].
                    // Compute 2-norm without under/overflow.
                    e[k] = 0;
                    for (int i = k + 1; i < n; i++)
                    {
                        e[k] = Hypotenuse(e[k], e[i]);
                    }

                    if (e[k] != 0.0)
                    {
                        if (e[k + 1] < 0.0)
                            e[k] = -e[k];

                        for (int i = k + 1; i < n; i++)
                            e[i] /= e[k];

                        e[k + 1] += 1.0;
                    }

                    e[k] = -e[k];
                    if ((k + 1 < m) & (e[k] != 0.0))
                    {
                        // Apply the transformation.
                        for (int i = k + 1; i < m; i++)
                            work[i] = 0.0;

                        for (int j = k + 1; j < n; j++)
                            for (int i = k + 1; i < m; i++)
                                work[i] += e[j] * a[i,j];

                        for (int j = k + 1; j < n; j++)
                        {
                            double t = -e[j] / e[k + 1];
                            for (int i = k + 1; i < m; i++)
                                a[i,j] += t * work[i];
                        }
                    }

                    if (wantv)
                    {
                        // Place the transformation in V for subsequent back multiplication.
                        for (int i = k + 1; i < n; i++)
                            v[i,k] = e[i];
                    }
                }
            }

            // Set up the final bidiagonal matrix or order p.
            int p = Math.Min(n, m + 1);
            if (nct < n) S[nct] = a[nct,nct];
            if (m < p) S[p - 1] = 0.0;
            if (nrt + 1 < p) e[nrt] = a[nrt,p - 1];
            e[p - 1] = 0.0;

            // If required, generate U.
            if (wantu)
            {
                for (int j = nct; j < nu; j++)
                {
                    for (int i = 0; i < m; i++)
                        u[i,j] = 0.0;
                    u[j,j] = 1.0;
                }

                for (int k = nct - 1; k >= 0; k--)
                {
                    if (S[k] != 0.0)
                    {
                        for (int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for (int i = k; i < m; i++)
                                t += u[i,k] * u[i,j];

                            t = -t / u[k,k];
                            for (int i = k; i < m; i++)
                                u[i,j] += t * u[i,k];
                        }

                        for (int i = k; i < m; i++)
                            u[i,k] = -u[i,k];

                        u[k,k] = 1.0 + u[k,k];
                        for (int i = 0; i < k - 1; i++)
                            u[i,k] = 0.0;
                    }
                    else
                    {
                        for (int i = 0; i < m; i++)
                            u[i,k] = 0.0;
                        u[k,k] = 1.0;
                    }
                }
            }

            // If required, generate V.
            if (wantv)
            {
                for (int k = n - 1; k >= 0; k--)
                {
                    if ((k < nrt) & (e[k] != 0.0))
                    {
                        for (int j = k + 1; j < nu; j++)
                        {
                            double t = 0;
                            for (int i = k + 1; i < n; i++)
                                t += v[i,k] * v[i,j];

                            t = -t / v[k + 1,k];
                            for (int i = k + 1; i < n; i++)
                                v[i,j] += t * v[i,k];
                        }
                    }

                    for (int i = 0; i < n; i++)
                        v[i,k] = 0.0;
                    v[k,k] = 1.0;
                }
            }

            // Main iteration loop for the singular values.
            int pp = p - 1;
            int iter = 0;
            double eps = Math.Pow(2.0, -52.0);
            while (p > 0)
            {
                int k, kase;

                // Here is where a test for too many iterations would go.
                // This section of the program inspects for
                // negligible elements in the s and e arrays.  On
                // completion the variables kase and k are set as follows.
                // kase = 1     if s(p) and e[k-1] are negligible and k<p
                // kase = 2     if s(k) is negligible and k<p
                // kase = 3     if e[k-1] is negligible, k<p, and s(k), ..., s(p) are not negligible (qr step).
                // kase = 4     if e(p-1) is negligible (convergence).
                for (k = p - 2; k >= -1; k--)
                {
                    if (k == -1)
                        break;

                    if (Math.Abs(e[k]) <= eps * (Math.Abs(S[k]) + Math.Abs(S[k + 1])))
                    {
                        e[k] = 0.0;
                        break;
                    }
                }

                if (k == p - 2)
                {
                    kase = 4;
                }
                else
                {
                    int ks;
                    for (ks = p - 1; ks >= k; ks--)
                    {
                        if (ks == k)
                            break;

                        double t = (ks != p ? Math.Abs(e[ks]) : 0.0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0.0);
                        if (Math.Abs(S[ks]) <= eps * t)
                        {
                            S[ks] = 0.0;
                            break;
                        }
                    }

                    if (ks == k)
                        kase = 3;
                    else if (ks == p - 1)
                        kase = 1;
                    else
                    {
                        kase = 2;
                        k = ks;
                    }
                }

                k++;

                // Perform the task indicated by kase.
                switch (kase)
                {
                    // Deflate negligible s(p).
                    case 1:
                        {
                            double f = e[p - 2];
                            e[p - 2] = 0.0;
                            for (int j = p - 2; j >= k; j--)
                            {
                                double t = Hypotenuse(S[j], f);
                                double cs = S[j] / t;
                                double sn = f / t;
                                S[j] = t;
                                if (j != k)
                                {
                                    f = -sn * e[j - 1];
                                    e[j - 1] = cs * e[j - 1];
                                }

                                if (wantv)
                                {
                                    for (int i = 0; i < n; i++)
                                    {
                                        t = cs * v[i,j] + sn * v[i,p - 1];
                                        v[i,p - 1] = -sn * v[i,j] + cs * v[i,p - 1];
                                        v[i,j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Split at negligible s(k).
                    case 2:
                        {
                            double f = e[k - 1];
                            e[k - 1] = 0.0;
                            for (int j = k; j < p; j++)
                            {
                                double t = Hypotenuse(S[j], f);
                                double cs = S[j] / t;
                                double sn = f / t;
                                S[j] = t;
                                f = -sn * e[j];
                                e[j] = cs * e[j];
                                if (wantu)
                                {
                                    for (int i = 0; i < m; i++)
                                    {
                                        t = cs * u[i,j] + sn * u[i,k - 1];
                                        u[i,k - 1] = -sn * u[i,j] + cs * u[i,k - 1];
                                        u[i,j] = t;
                                    }
                                }
                            }
                        }
                        break;

                    // Perform one qr step.
                    case 3:
                        {
                            // Calculate the shift.
                            double scale = Math.Max(Math.Max(Math.Max(Math.Max(Math.Abs(S[p - 1]), Math.Abs(S[p - 2])), Math.Abs(e[p - 2])), Math.Abs(S[k])), Math.Abs(e[k]));
                            double sp = S[p - 1] / scale;
                            double spm1 = S[p - 2] / scale;
                            double epm1 = e[p - 2] / scale;
                            double sk = S[k] / scale;
                            double ek = e[k] / scale;
                            double b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2.0;
                            double c = (sp * epm1) * (sp * epm1);
                            double shift = 0.0;
                            if ((b != 0.0) | (c != 0.0))
                            {
                                shift = Math.Sqrt(b * b + c);
                                if (b < 0.0)
                                    shift = -shift;
                                shift = c / (b + shift);
                            }

                            double f = (sk + sp) * (sk - sp) + shift;
                            double g = sk * ek;

                            // Chase zeros.
                            for (int j = k; j < p - 1; j++)
                            {
                                double t = Hypotenuse(f, g);
                                double cs = f / t;
                                double sn = g / t;
                                if (j != k)
                                    e[j - 1] = t;
                                f = cs * S[j] + sn * e[j];
                                e[j] = cs * e[j] - sn * S[j];
                                g = sn * S[j + 1];
                                S[j + 1] = cs * S[j + 1];
                                if (wantv)
                                {
                                    for (int i = 0; i < n; i++)
                                    {
                                        t = cs * v[i,j] + sn * v[i,j + 1];
                                        v[i,j + 1] = -sn * v[i,j] + cs * v[i,j + 1];
                                        v[i,j] = t;
                                    }
                                }

                                t = Hypotenuse(f, g);
                                cs = f / t;
                                sn = g / t;
                                S[j] = t;
                                f = cs * e[j] + sn * S[j + 1];
                                S[j + 1] = -sn * e[j] + cs * S[j + 1];
                                g = sn * e[j + 1];
                                e[j + 1] = cs * e[j + 1];
                                if (wantu && (j < m - 1))
                                {
                                    for (int i = 0; i < m; i++)
                                    {
                                        t = cs * u[i,j] + sn * u[i,j + 1];
                                        u[i,j + 1] = -sn * u[i,j] + cs * u[i,j + 1];
                                        u[i,j] = t;
                                    }
                                }
                            }

                            e[p - 2] = f;
                            iter = iter + 1;
                        }
                        break;

                    // Convergence.
                    case 4:
                        {
                            // Make the singular values positive.
                            if (S[k] <= 0.0)
                            {
                                S[k] = (S[k] < 0.0 ? -S[k] : 0.0);
                                if (wantv)
                                    for (int i = 0; i <= pp; i++)
                                        v[i,k] = -v[i,k];
                            }

                            // Order the singular values.
                            while (k < pp)
                            {
                                if (S[k] >= S[k + 1])
                                    break;

                                double t = S[k];
                                S[k] = S[k + 1];
                                S[k + 1] = t;
                                if (wantv && (k < n - 1))
                                    for (int i = 0; i < n; i++)
                                    {
                                        t = v[i,k + 1];
                                        v[i,k + 1] = v[i,k];
                                        v[i,k] = t;
                                    }

                                if (wantu && (k < m - 1))
                                    for (int i = 0; i < m; i++)
                                    {
                                        t = u[i,k + 1];
                                        u[i,k + 1] = u[i,k];
                                        u[i,k] = t;
                                    }

                                k++;
                            }

                            iter = 0;
                            p--;
                        }
                        break;
                }
            }
        }

        /// <summary>Returns the condition number <c>max(S) / min(S)</c>.</summary>
        public double Condition
        {
            get
            {
                return S[0] / S[Math.Min(m, n) - 1];
            }
        }

        /// <summary>Returns the Two norm.</summary>
        public double Norm2
        {
            get
            {
                return S[0];
            }
        }

        /// <summary>Returns the effective numerical matrix rank.</summary>
        /// <value>Number of non-negligible singular values.</value>
        public int Rank
        {
            get
            {
                double eps = Math.Pow(2.0, -52.0);
                double tol = Math.Max(m, n) * S[0] * eps;
                int r = 0;
                for (int i = 0; i < S.Length; i++)
                {
                    if (S[i] > tol)
                    {
                        r++;
                    }
                }

                return r;
            }
        }


        private static double Hypotenuse(double a, double b)
        {
            if (Math.Abs(a) > Math.Abs(b))
            {
                double r = b / a;
                return Math.Abs(a) * Math.Sqrt(1 + r * r);
            }

            if (b != 0)
            {
                double r = a / b;
                return Math.Abs(b) * Math.Sqrt(1 + r * r);
            }

            return 0.0;
        }
    }

}
