using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;

namespace Lomont.Numerical
{
    /// <summary>
    /// General sized matrix
    /// </summary>
    public class Matrix
    {
        #region Members
        public double[,] Values { get; private set; }
        public int Rows { get; }
        public int Columns { get; }
        #endregion

        #region Constructors
        public Matrix(double[,] values)
        {
            Values = values;
            Rows = values.GetLength(0);
            Columns = values.GetLength(1);
        }

        /// <summary>
        /// Create a rows by columns sized matrix
        /// default to zero matrix
        /// </summary>
        /// <param name="rows"></param>
        /// <param name="columns"></param>
        public Matrix(int rows, int columns, double[]? values = null)
        {
            Rows = rows;
            Columns = columns;
            Values = new double[rows, columns];
            if (values != null)
            {
                System.Diagnostics.Trace.Assert(columns * rows == values.Length);
                var k = 0;
                for (var row = 0; row < Rows; ++row)
                    for (var col = 0; col < Columns; ++col)
                        Values[row, col] = values[k++];
            }
        }

        public Matrix(Matrix m) : this (m.Rows, m.Columns)
        {
            for (var row = 0; row < Rows; ++row)
                for (var col = 0; col < Columns; ++col)
                    Values[row, col] = m[row, col];
        }

        public Matrix(int rows, int columns, IEnumerable<double> vals) : this(rows, columns, vals.ToArray())
        {
        }

        /// <summary>
        /// Constant value matrix
        /// </summary>
        /// <param name="val"></param>
        public Matrix(int rows, int columns, double value) : this(rows, columns)
        {
            for (var row = 0; row < Rows; ++row)
                for (var col = 0; col < Columns; ++col)
                    Values[row, col] = value;
        }


        #endregion

        /// <summary>
        /// Min value in matrix
        /// </summary>
        public double Min => Get(Double.MaxValue, System.Math.Min);
        /// <summary>
        /// Max value in matrix
        /// </summary>
        public double Max => Get(Double.MinValue, System.Math.Max);

        /// <summary>
        /// Given start value, apply functor over all matrix items
        /// </summary>
        /// <param name="s"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public double Get(double s, Func<double, double, double> f)
        {
            var t = s;
            for (var i = 0; i < Rows; ++i)
                for (var j = 0; j < Columns; ++j)
                    t = f(this[i, j], t);
            return t;
        }


        public double this[int row, int col]
            {
            get => Values[row, col]; 
            set => Values[row, col] = value; 
            }

        public double Trace
        {
            get
            {
                var s = 0.0;
                for (var i = 0; i < Math.Min(Rows,Columns); ++i)
                    s += this[i, i];
                return s;
            }
        }


        public static Matrix operator *(Matrix lhs, Matrix rhs)
        {
            System.Diagnostics.Trace.Assert(lhs.Columns == rhs.Rows);

            var m = new Matrix(lhs.Rows, rhs.Columns);
            for (var i = 0; i < lhs.Rows; ++i)
                for (var j = 0; j < rhs.Columns; ++j)
                {
                    var s = 0.0;
                    for (var k = 0; k < lhs.Columns; ++k)
                        s += lhs[i, k] * rhs[k, j];
                    m[i, j] = s;
                }
            return m;
        }

        public static Matrix operator+(Matrix lhs, Matrix rhs)
        {
            System.Diagnostics.Trace.Assert(lhs.Rows == rhs.Rows && lhs.Columns == rhs.Columns);
            var m = new Matrix(lhs.Rows,lhs.Columns);
            for (var i = 0; i < lhs.Rows; ++i)
                for (var j = 0; j < lhs.Columns; ++j)
                    m[i, j] = lhs[i, j] + rhs[i, j];
            return m;
        }
        public static Matrix operator -(Matrix lhs, Matrix rhs)
        {
            System.Diagnostics.Trace.Assert(lhs.Rows == rhs.Rows && lhs.Columns == rhs.Columns);
            var m = new Matrix(lhs.Rows, lhs.Columns);
            for (var i = 0; i < lhs.Rows; ++i)
                for (var j = 0; j < lhs.Columns; ++j)
                    m[i, j] = lhs[i, j] - rhs[i, j];
            return m;
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

        /// <summary>
        /// Submatrix by removing one row and column
        /// </summary>
        /// <returns></returns>
        public Matrix Submatrix(int row, int column)
        {
            return Submatrix(new int[] { row }, new int[] { column });
        }

        /// <summary>
        /// Submatrix by removing rows and columns
        /// </summary>
        /// <returns></returns>
        public Matrix Submatrix(int [] rows, int [] columns)
        {
            // quick lookups - todo - make whole thing more efficient?
            var hi = new HashSet<int>(rows);
            var hj = new HashSet<int>(columns);

            var m = new Matrix(Rows-rows.Length, Columns-columns.Length);
            
            var si = 0; // source index
            for (var i = 0; i < m.Rows; ++i)
            {
                while (hi.Contains(si))
                    ++si;

                var sj = 0; // source index
                for (var j = 0; j < m.Columns; ++j)
                {
                    while (hj.Contains(sj))
                        ++sj;

                    m[i, j] = this[si,sj];
                    ++sj;
                }
                ++si;
            }

            return m;
        }

        /// <summary>
        /// Transpose in place, return self
        /// </summary>
        public Matrix Transpose()
        {
            if (Rows == Columns)
            {
                //in place
                for (var i = 0; i < Rows; i++)
                    for (var j = i + 1; j < Columns; j++)
                    { // swap
                        (this[i, j], this[j, i]) = (this[j, i], this[i, j]);
                    }
            }
            else
            {
                var dest = new double[Columns, Rows];
                for (var i = 0; i < Rows; i++)
                    for (var j = Columns; j < Columns; j++)
                        dest[j,i] = this[i,j];
                Values = dest;
            }
            return this;
        }

        public static Matrix operator /(Matrix m, double s) => (1 / s) * m;

        public static Matrix operator *(Matrix m, double s) => s * m;

        public static Matrix operator *(double s, Matrix m)
        {
            var m2 = new Matrix(m.Rows,m.Columns);
            for (var i = 0; i < m.Rows; ++i)
                for (var j = 0; j < m.Columns; ++j)
                    m2[i, j] = m[i,j]*s;
            return m2;
        }

        public bool Equals(Matrix other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.Columns != Columns || other.Rows != Rows)
                return false;
            
            var diff = other - this;
            var n = diff.MaxNorm();

            return n < 0.0001; // todo- smarter zero compare?
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Matrix)obj); // calls above
        }

        public override int GetHashCode()
        {
            // todo?
            return (Values != null ? Values.GetHashCode() : 0);
        }

        public static bool operator ==(Matrix a, Matrix b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Matrix a, Matrix b)
        {
            return !(a == b);
        }

        public static Matrix operator +(Matrix a) => a;
        public static Matrix operator -(Matrix a)
        {
            var m = new Matrix(a);
            m.Apply((i, j, v) => -v);
            return m;
        }


        /// <summary>
        /// Functor - apply fun to i,j,v
        /// </summary>
        public void Apply(Func<int, int, double, double> func)
        {
            for (var i = 0; i < Rows; ++i)
                for (var j = 0; j < Columns; ++j)
                    this[i, j] = func(i, j, this[i, j]);
        }


        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Rows; ++i)
            {
                for (var j = 0; j < Columns; ++j)
                    sb.Append($"{Values[i, j]} ");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #region Norms
        public double MaxNorm()
        {
            var norm = 0.0;
            for (var i = 0; i < Rows; ++i)
                for (var j = 0; j < Columns; ++j)
                    norm = Math.Max(norm, Math.Abs(Values[i, j]));
            return norm;
        }

        #endregion


    }
}
