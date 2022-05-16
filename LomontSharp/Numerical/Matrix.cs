using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
