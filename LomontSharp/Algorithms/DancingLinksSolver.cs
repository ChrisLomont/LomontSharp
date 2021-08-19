using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lomont.Algorithms
{
    // copyright Chris Lomont, www.lomont.org
    // Chris Lomont, 2015, 
    //     - added dumping the state, lineEnd, and solutionEnd, UseGolumbHeuristic
    //     - added SolutionRecorderDelegate for nicer solution handling
    // Chris Lomont, 2013, C# ported from earlier Lomont C++ version
    // todo - extend to have a name for each row, and return row names in solution 


    /// <summary>
    /// code for Knuth's Dancing Links algorithm
    /// based on Knuth's paper on Dancing Links algorithm X, hence DLX
    /// To use:
    /// 1. Create a column for each entry with AddColumn
    /// 2. for each row, call NewRow, then call SetColumn for each entry
    /// 3. set the output action
    /// 4. Call Solve
    /// 
    /// The solver picks all subsets of rows that have exactly one entry
    /// in each column. To mark some columns as allowing zero or one entry, 
    /// mark them as secondary columns when adding them by setting the 
    /// "mandatory" boolean to false;
    /// </summary>
    public class DancingLinksSolver
    {
        #region Public interface

        /// <summary>
        /// Create a new Algorithm X solver using Dancing Links
        /// </summary>
        public DancingLinksSolver()
        {
            output = null;
            root = new Header();
            root.Left = root.Right = root;
            row = null;
            column = null;
            SolutionListener = null;
            iterRow = null;

            UseGolumbHeuristic = true;
        }

        /// <summary>
        /// Add a column to the solver. There is usually a column for each puzzle piece
        /// and one for each cell on the board
        /// </summary>
        /// <param name="name">The header name, should be unique</param>
        /// <param name="mandatory">If false, this column is secondary, meaning it does not need to have an entry in the solution</param>
        public void AddColumn(string name, bool mandatory = true)
        {
            var col = new Header { Name = name, Size = 0 };
            col.Up = col.Down = col.Left = col.Right = col.Head = col;
            if (mandatory)
            {
                col.Right = root;
                col.Left = root.Left;
                root.Left.Right = col;
                root.Left = col;
            }
            names[name] = columns.Count;
            columns.Add(col);
        }

        /// <summary>
        /// Call this to start a new row. 
        /// Then use SetColumn to set entries
        /// </summary>
        public void NewRow()
        {
            row = null;
        }

        /// <summary>
        /// Set a column as attached on the current row
        /// </summary>
        /// <param name="name">The name of the column</param>
        public void SetColumn(string name)
        {
            if (!names.ContainsKey(name))
                return;
            SetColumn(names[name]);
        }

        /// <summary>
        /// Set a column as attached on the current row
        /// </summary>
        /// <param name="number">The index of the column</param>
        public void SetColumn(int number)
        {
            if (number >= columns.Count) return;
            var header = columns[number];
            var node = new Node();
            if (row == null)
            {
                row = node;
                rows.Add(node);
                node.Left = node;
                node.Right = node;
            }
            else
            {
                node.Left = row;
                node.Right = row.Right;
                row.Right.Left = node;
                row.Right = node;
            }
            node.Head = header;
            node.Up = header;
            node.Down = header.Down;
            header.Down.Up = node;
            header.Down = node;
            header.Size++;
        }


        /// <summary>
        /// Disconnect a row
        /// </summary>
        /// <param name="number"></param>
        public void DisableRow(int number)
        {
            if (number >= rows.Count) return;
            var i = rows[number];
            if (i.Up == i) return; // already disabled
            var j = i;
            do
            {
                j.Up.Down = j.Down;
                j.Down.Up = j.Up;
                j.Down = j.Up = j;
                j.Head.Size--;
                j = j.Right;
            } while (j != i);
        }

        /// <summary>
        /// Enable a row for usage
        /// </summary>
        /// <param name="number"></param>
        public void EnableRow(int number)
        {
            if (number >= rows.Count) return;
            var i = rows[number];
            if (i.Up != i) return; // already enabled
            var j = i;
            do
            {
                j.Up = j.Head;
                j.Down = j.Head.Down;
                j.Up.Down = j;
                j.Down.Up = j;
                j.Head.Size++;
                j = j.Right;
            } while (j != i);
        }

        /// <summary>
        /// Solve the puzzle. Answers are written to 
        /// wherever output was sent.
        /// </summary>
        public void Solve()
        {
            more = true;
            stack.Clear();
            DequeRemovals = 0;
            NumSolutions = 0;
            Search();
        }

        /// <summary>
        /// Set where the output text goes
        /// Can be used like SetOutput(Console.Out);
        /// </summary>
        /// <param name="os">the text writer where output goes</param>
        /// <param name="newLineEnd">An optional marker to denote line ends in each solution</param>
        /// <param name="newSolutionEnd">An optional marker to denote a solution end</param>
        public void SetOutput(TextWriter os, string newLineEnd = "", string newSolutionEnd = "")
        {
            output = os;
            lineEnd = newLineEnd;
            solutionEnd = newSolutionEnd;
        }

        /// <summary>
        /// Each solution can be captured using this delgate.
        /// NOTE: we avoid using the standard event mechanism so we can return
        /// a value to continue or stop enumeration
        /// </summary>
        /// <param name="solutionNumber">The number of solutions found</param>
        /// <param name="dequeueNumber">How many total dequeues have been done so far</param>
        /// <param name="solution">A list of solution combinations, each combination
        /// is a list of column headers for the given selection</param>
        /// <returns>Return true to continue enumeration, or false to stop</returns>
        public delegate bool SolutionRecorderDelegate(
            long solutionNumber,
            long dequeueNumber,
            List<List<string>> solution
            );

        /// <summary>
        /// Set this to listen to solutions. See SolutionRecorderDelegate
        /// for the meanings of the parameters
        /// </summary>
        public SolutionRecorderDelegate? SolutionListener;

        /// <summary>
        /// Dumps state as test to the given output
        /// </summary>
        public void DumpState(TextWriter outtext)
        {
            if (outtext == null)
                return;

            foreach (var header in columns)
            {
                outtext.Write(header.Name + ",");
            }
            outtext.WriteLine();

            foreach (var node in rows)
            {
                foreach (var header in columns)
                    outtext.Write(RowContains(node, header) ? "1" : "0");
                outtext.WriteLine();
            }
        }

        /// <summary>
        /// Number of solutions found
        /// </summary>
        public long NumSolutions { get; protected set; }

        /// <summary>
        /// Number of item removals performed during the search
        /// </summary>
        public long DequeRemovals { get; protected set; }

        /// <summary>
        /// Use the hueristic Knuth calls S when choosing.
        /// It results in smaller searches at the cost of more
        /// memory access per node. Knuth's tests showed it useful
        /// in all cases he tried. Your mileage may vary.
        /// </summary>
        public bool UseGolumbHeuristic { get; set; }

        /// <summary>
        /// Count of NumColumns so far
        /// </summary>
        public int ColumnCount => columns.Count;

        /// <summary>
        /// Count of NumRows so far
        /// </summary>
        public int RowCount => rows.Count;

        #endregion

        #region Protected region

        protected virtual string? GetSolution()
        {
            if (iterRow != null)
            {
                var ret = iterRow.Head.Name;
                iterRow = iterRow.Right;
                if (iterRow == stack[iterStack])
                    iterRow = null;
                return ret;
            }

            if (iterStack != stack.Count && ++iterStack != stack.Count)
            {
                iterRow = stack[iterStack];
            }
            return null;
        }



        /// <summary>
        /// Record a solution, by sending it to the current text output if asked, 
        /// and to any listeners if present
        /// </summary>
        /// <returns>true to continue search, else false to stop</returns>
        protected virtual bool Record()
        {
            var solutionEventHandler = SolutionListener;
            if (solutionEventHandler == null && output == null)
                return true; // nothing else to do

            var solutionNodes = new List<List<string>>();
            for (var s = GetSolution(); s != null; s = GetSolution())
            {
                var lineNodes = new List<string>();
                for (; s != null; s = GetSolution())
                    lineNodes.Add(s);
                if (lineNodes.Any())
                    solutionNodes.Add(lineNodes);
            }
            var retval = true;
            if (solutionEventHandler != null)
                retval = solutionEventHandler(NumSolutions, DequeRemovals, solutionNodes);
            PrintSolution(solutionNodes);
            return retval;
        }

        #endregion

        #region Private implementation

        // end of a line marker for output
        string lineEnd = "";
        // end of a solution marker for output
        string solutionEnd = "";
        // where to write text output, or null
        TextWriter? output;

        /// <summary>
        /// Write out a solution to the current output
        /// </summary>
        private void PrintSolution(IEnumerable<List<string>> solutionNodes)
        {
            if (output == null)
                return; // nothing to do
            var os = output;
            os.WriteLine("Solution {0} (found after {1} deque removals):\n", NumSolutions, DequeRemovals);
            foreach (var line in solutionNodes)
            {
                foreach (var s in line)
                    os.Write("{0} ", s);
                os.WriteLine(lineEnd);
            }
            os.WriteLine(solutionEnd);
        }


        // return true if row is connected to the given header
        private bool RowContains(Node node, Header header)
        {
            if (node == null || header == null) return false;
            var start = node;
            var cur = node;
            do
            {
                if (cur.Head == header)
                    return true;
                cur = cur.Right;
            } while (cur != start);
            return false;
        }



        // DLX node from Knuth paper
        private class Node
        {
            public Node? Left, Right, Up, Down;
            public Header? Head;
        }

        // DLX header from Knuth paper
        private class Header : Node
        {
            public int Size;
            public string? Name;
        }


        // main 2D doubly-linked structure
        private readonly Header root;

        // list of NumColumns
        private readonly List<Header> columns = new List<Header>();
        private readonly Dictionary<string, int> names = new Dictionary<string, int>();

        // list of non-empty NumRows
        private readonly List<Node> rows = new List<Node>();

        // solution stack, and pointers to browse it
        private readonly List<Node> stack = new List<Node>();
        private int iterStack;
        private Node? iterRow;

        // Search() temporary variables (not local to reduce recursion load)
        private Header? column;
        private Node? row;
        private bool more; // continue search?

        // search methods
        private void Search()
        {
            if (root.Right == root)
            {
                NumSolutions++;
                iterStack = 0;
                iterRow = (iterStack != stack.Count ? stack[iterStack] : null);
                more = Record();
                return;
            }
            Choose();
            Cover(column);
            stack.Add(null);
            for (row = column.Down; row != column; row = row.Down)
            {
                for (var i = row.Right; i != row; i = i.Right)
                {
                    Cover(i.Head);
                }
                stack[stack.Count - 1] = row;
                Search();
                row = stack[stack.Count - 1];
                column = row.Head;
                for (var i = row.Left; i != row; i = i.Left)
                {
                    Uncover(i.Head);
                }
                if (!more) break;
            }
            stack.RemoveAt(stack.Count - 1);
            Uncover(column);
        }

        private void Cover(Header col)
        {
            col.Right.Left = col.Left;
            col.Left.Right = col.Right;
            DequeRemovals++;
            for (var i = col.Down; i != col; i = i.Down)
            {
                for (var j = i.Right; j != i; j = j.Right)
                {
                    j.Down.Up = j.Up;
                    j.Up.Down = j.Down;
                    j.Head.Size--;
                    DequeRemovals++;
                }
            }
        }

        private void Uncover(Header col)
        {
            for (var i = col.Up; i != col; i = i.Up)
            {
                for (var j = i.Left; j != i; j = j.Left)
                {
                    j.Head.Size++;
                    j.Down.Up = j;
                    j.Up.Down = j;
                }
            }
            col.Right.Left = col;
            col.Left.Right = col;
        }

        private void Choose()
        {
            if (UseGolumbHeuristic)
            {
                // implements Heuristic S from Knuth
                var best = int.MaxValue;
                for (var i = (Header)root.Right; i != root; i = (Header)i.Right)
                {
                    if (i.Size >= best) continue;
                    column = i;
                    best = i.Size;
                }
            }
            else
            {
                // take first (is this sufficient?)
                column = (Header)root.Right;
            }
        }
        #endregion
    }
}



