using System;
using System.Diagnostics;
using System.Linq;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Solves the maximal (or minimal) matching problem via the Hungarian algorithm
    /// Given an array of costs for workers to tasks, find the optimal assignment
    /// Kuhn-Munkres algorithm?
    ///
    /// TODO - remove allocations where possible
    /// 
    /// </summary>
    public static class OptimalMatching
    {
        /// <summary>
        /// Find optimal assignment of workers to tasks, given a cost matrix, and optimal cost is the minimum cost.
        /// To use for maximum, negate all costs. (todo - does this work? - could invert order of costs by MAX-cost at each spot)
        ///
        /// Modifies costs - todo - clone it? todo - also return best cost?
        /// </summary>
        /// <param name="costs">cost[i,j] is cost of worker (row) i doing task (column) j</param>
        /// <returns>array, index i is column of task for worker i</returns>
        public static int[] Find(int[,] costs)
        {
            // implemented from https://en.wikipedia.org/wiki/Hungarian_algorithm and
            // https://users.cs.duke.edu/~brd/Teaching/Bio/asmb/current/Handouts/munkres.html

            if (costs == null)
                throw new ArgumentNullException(nameof(costs));

            // add dummy rows, column if needed to make square, fill with largest value
            // https://study.com/academy/lesson/using-the-hungarian-algorithm-to-solve-assignment-problems.html
            // https://www.wikihow.com/Use-the-Hungarian-Algorithm
            costs = EnsureSquare(costs);

            var (rows, cols) = (costs.GetLength(0), costs.GetLength(1));
            Trace.Assert(rows == cols);
            var sz = rows; // same as cols, use where square matrix assumed

            // reduce costs in each row
            for (var r = 0; r < rows; r++)
            {
                // row min
                var min = int.MaxValue;
                var r1 = r; // prevent lambda capture
                Apply(sz, c => min = Math.Min(min, costs[r1, c]));
                Apply(sz, c => costs[r1, c] -= min);
            }

            // create state, initialized
            var state = new State(costs);

            while(true)
            {
                if (DoStep1(state))
                    break;
                while (DoStep2(state))
                { // loop
                }
            }

            var assignments = new int[rows];
            ForEach(state.masks, (v, r, c) => { if (v == MS.Starred) assignments[r] = c; });

            return assignments;
        }

        // return true if finished 
        static bool DoStep1(State state)
        {
            ForEach(state.masks, (v, _, c) => { if (v == MS.Starred) state.coveredCols[c] = true; });

            // process finished if all columns covered
            return state.coveredCols.All(c => c);
        }

        // return true if repeat step
        static bool DoStep2( State state)
        {
            while (true)
            {
                // look for zero
                Location loc = Invalid;

                ForEach(state.costs, (v, r, c) =>
                {
                        if (v == 0 && !state.coveredRows[r] && !state.coveredCols[c])
                            loc = new(r, c);
                });

                if (loc.Row == -1)
                {   // always to step 4, then 2. Just do that here
                    //return Step.Step4;
                    DoStep4(state);
                    return true;
                }

                state.masks[loc.Row, loc.Col] = MS.Primed;

                // find star in row
                var starCol = -1;
                Apply(state.sz, c =>
                {
                    if (state.masks[loc.Row, c] == MS.Starred)
                        starCol = c;
                });


                if (starCol != -1)
                {
                    state.coveredRows[loc.Row] = true;
                    state.coveredCols[starCol] = false;
                }
                else
                {
                    state.pathStart = loc;

                    // always to step 3, then 1. Just do that here
                    DoStep3(state);
                    return false;
                }
            }
        }

        static void DoStep3(State state)
        {
            var pathIndex = 0;
            state.path[0] = state.pathStart;

            while (true)
            {
                // find star in column
                var row = -1;
                Apply(state.sz, r => {
                    if (state.masks[r, state.path[pathIndex].Col] == MS.Starred)
                        row = r;
                });
                if (row == -1) break;

                pathIndex++;
                state.path[pathIndex] = new Location(row, state.path[pathIndex - 1].Col);

                // find prime in row
                var col = -1;
                Apply(state.sz, c =>
                {
                    if (state.masks[state.path[pathIndex].Row, c] == MS.Primed)
                        col = c;
                });

                pathIndex++;
                state.path[pathIndex] = new Location(state.path[pathIndex - 1].Row, col);
            }

            // convert path
            for (var i = 0; i <= pathIndex; i++)
            {
                if (state.masks[state.path[i].Row, state.path[i].Col] == MS.Starred)
                    state.masks[state.path[i].Row, state.path[i].Col] = 0;
                else if (state.masks[state.path[i].Row, state.path[i].Col] == MS.Primed)
                    state.masks[state.path[i].Row, state.path[i].Col] = MS.Starred;
            }

            state.ClearCovers();

            // clear primes
            ForEach(state.masks, (v, r, c) => { if (v == MS.Primed) state.masks[r, c] = MS.None; });

            // always goes to step 1
        }
        static void DoStep4(State state)
        {
            // min cost for non-covered entries
            var minValue = int.MaxValue;
            
            ApplyAll(state.sz, (r, c) =>
            {
                if (!state.coveredRows[r] && !state.coveredCols[c])
                    minValue = Math.Min(minValue, state.costs[r, c]);
            });

            // adjust them
            ApplyAll(state.sz, (r,c) =>
            {
                if (state.coveredRows[r])
                    state.costs[r, c] += minValue;
                if (!state.coveredCols[c])
                    state.costs[r, c] -= minValue;

            });
            // always returns to step 2 next
        }
        class State
        {
            public State(int[,] costs)
            {
                this.costs = costs;
                sz = costs.GetLength(0);
                pathStart = Invalid;
                path = new Location[sz];
                coveredCols = new bool[sz];
                coveredRows = new bool[sz];
                masks = new MS[sz, sz];

                // init
                ApplyAll(sz, (r, c) =>
                {
                    if (costs[r, c] == 0 && !coveredRows[r] && !coveredCols[c])
                    {
                        masks[r, c] = MS.Starred;
                        coveredRows[r] = true;
                        coveredCols[c] = true;
                    }
                });
                ClearCovers();

            }
            public void ClearCovers()
            {
                for (var i = 0; i < sz; i++)
                {
                    coveredRows[i] = false;
                    coveredCols[i] = false;
                }
            }

            public int sz;
            public int[,] costs;
            public bool[] coveredCols, coveredRows;
            public Location pathStart;
            public Location[] path;
            public MS[,] masks;
        }

        enum MS // mask state
        {
            None = 0,
            Starred = 1,
            Primed = 2
        }
        // perform action over loop
        static void Apply(int sz, Action<int> action)
        {
            for (var i = 0; i < sz; i++)
                action(i);
        }

        // apply action over square grid
        static void ApplyAll(int sz, Action<int, int> action)
        {
            for (var r = 0; r < sz; r++)
            for (var c = 0; c < sz; c++)
                action(r, c);
        }

        static void ForEach<T>(T[,] mat, Action<T, int, int> action)
        {
            var (rows, cols) = (mat.GetLength(0), mat.GetLength(1));
            for (var r = 0; r < rows; ++r)
            for (var c = 0; c < cols; ++c)
                action(mat[r, c], r, c);
        }


        // simple 2d lattice point
        record Location(int Row, int Col);
        static readonly Location Invalid = new(-1, -1);

        /// <summary>
        /// Ensure matrix square, add max cost to square it up
        /// </summary>
        /// <param name="mat"></param>
        /// <returns></returns>
        static int[,] EnsureSquare(int[,] mat)
        {
            var (rows,cols) = (mat.GetLength(0),mat.GetLength(1));
            if (rows == cols) return mat; // nothing to do
            var size = Math.Max(rows,cols);
            var sq = new int[size,size];

            // find max for replacement
            var maxVal = Int32.MinValue;
            for (var i = 0; i < rows; ++i)
            for (var j = 0; j < cols; ++j)
                maxVal = Math.Max(maxVal, mat[i, j]);

            // copy over
            for (var i = 0; i < size; ++i)
            for (var j = 0; j < size; ++j)
                sq[i, j] = i < rows && j < cols ? mat[i, j] : maxVal;
            return sq;
        }
    }
}
