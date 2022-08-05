using System;

namespace Lomont.Numerical
{
    /// <summary>
    /// General sized matrix
    /// </summary>
    public class Matrix
    {
        public double[,] Values { get;  }
        public int Rows { get;  }
        public int Columns { get; }

        public double this[int i, int j]
            {
            get { return Values[i, j]; }
            set { Values[i, j] = value; }
            }


        /// <summary>
        /// Create a rows by columns sized matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public Matrix(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Values = new double[rows,columns]; 
        }

        /// <summary>
        /// Factors matrix A into lower and upper tri matrices, in place
        /// for solving the lin eqn Ax=b.
        /// 
        /// Follow up with other call LU_solve(m,pivotIndices,b) then b gets replaced with soln x
        /// </summary>
        /// <param name="A"></param>
        /// <param name="pivotIndices"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static bool FactorLU(double [,] A, int[] pivotIndices)
        {
            var M = A.GetLength(0); // # A rows
            var N = A.GetLength(1); // # A cols

            if (M == 0 || N == 0) return false;
            if (pivotIndices.Length != M) throw new Exception("wrong index size");

            var minMN = Math.Min(M, N);

            for (var j = 0; j < minMN; j++)
            {
                // find pivot in column j, test singular
                var pivot = j;
                var t = Math.Abs(A[j, j]);
                for (var i = j + 1; i < M; i++)
                {
                    if (Math.Abs(A[i, j]) > t)
                    {
                        pivot = i;
                        t = Math.Abs(A[i, j]);
                    }
                }

                pivotIndices[j] = pivot;

                // pivot is index of max elt in col j, below diagonal
                if (A[pivot, j] == 0)
                    return false;  // failed - zero pivot


                if (pivot != j) // swap rows j and pivot
                    for (var k = 0; k < N; k++)
                    {
                        t = A[j, k];
                        A[j, k] = A[pivot, k];
                        A[pivot, k] = t;
                    }

                if (j + 1 < M)
                {
                    var recp = 1.0 / A[j, j];

                    for (var k = j + 2; k <= M; k++)
                        A[k - 1, j] *= recp;
                }


                if (j < minMN)
                {
                    // update trailing submatrix
                    for (var ii = j + 1; ii < M; ii++)
                        for (var jj = j + 1; jj < N; jj++)
                            A[ii, jj] -= A[ii, j] * A[j, jj];
                }
            }

            return true; // success
        }





        /// <summary>
        /// Solve Ax=b
        /// </summary>
        /// <param name="A"></param>
        /// <param name="pivotIndices"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void SolveLU(double [,] A, int[] pivotIndices, double[] b)
        {
            var n = b.Length;

            var i2 = -1;

            for (var i = 0; i < n; i++)
            {
                var ip = pivotIndices[i];
                var sum = b[ip];
                b[ip] = b[i];
                if (i2 != -1)
                {
                    for (var j = i2; j < i; j++)
                        sum -= A[i, j] * b[j];
                }
                else if (sum != 0)
                {
                    i2 = i;
                }
                b[i] = sum;
            }

            for (var i = n - 1; i >= 0; i--)
            {
                var sum = b[i];
                for (var j = i + 1; j < n; j++)
                    sum -= A[i, j] * b[j];
                b[i] = sum / A[i, i];
            }
        }

    }
}
