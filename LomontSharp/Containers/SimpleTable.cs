using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Lomont.Graphics;
using Lomont.Stats;

namespace Lomont.Containers
{
    /// <summary>
    /// A simple table of type with row and column names
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SimpleTable<T>
    {
        List<string> colNames = new List<string>();
        List<string> rowNames = new List<string>();

        public int Rows { get; }
        public int Columns { get; }
        public SimpleTable(IList<string> rowNames, IList<string> columnNames)
        {
            this.colNames = columnNames.ToList();
            this.rowNames = rowNames.ToList();
            Rows = rowNames.Count;
            Columns = colNames.Count;

            grid = new T[Rows, Columns];
        }

        /// <summary>
        /// Row, column indexing
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        public T this[int row, int column]
        {
            get => grid[row, column];
            set => grid[row, column] = value;
        }

        public string ColumnName(int column) => colNames[column];
        public string RowName(int column) => rowNames[column];


        /// <summary>
        /// Get value, throw on invalid args
        /// </summary>
        /// <param name="rowName"></param>
        /// <param name="columnName"></param>
        public T Get(string rowName, string columnName)
        {
            var row = rowName.IndexOf(rowName);
            var col = colNames.IndexOf(columnName);
            if (row == -1 || col == -1)
                throw new ArgumentException("Invalid names in SimpleTable");
            return grid[row, col];
        }

        /// <summary>
        /// Set value, throw on invalid args
        /// </summary>
        /// <param name="rowName"></param>
        /// <param name="columnName"></param>
        /// <param name="value"></param>
        public void Set(string rowName, string columnName, T value)
        {
            var row = rowName.IndexOf(rowName);
            var col = colNames.IndexOf(columnName);
            if (row == -1 || col == -1)
                throw new ArgumentException("Invalid names in SimpleTable");
            grid[row, col] = value;
        }

        T[,] grid;


    }
}
