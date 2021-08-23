using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using static System.Math;

namespace Lomont.Algorithms
{
    /// <summary>
    /// Digital Differential Analyzer
    /// Uses Bresenham-like algorithm to walk lines in pixel (or voxel) spaces
    /// </summary>
    public static class DDA
    {
        // todo - add a 3d specialized one
        // todo - ensure points traversed are direction independent

        /// <summary>
        ///     Given start and end lattice points in n-dimensional space, walk
        ///     start to destination (inclusive) and choose nearest lattice points,
        ///     similar to line drawing algorithms. Iterate over such points
        /// </summary>
        /// <param name="startPoint">The start n-dimensional point</param>
        /// <param name="endPoint">The end n-dimensional point</param>
        public static IEnumerable<List<int>> DimN(List<int> startPoint, List<int> endPoint)
        {
            if ((startPoint == null) || (endPoint == null) || (startPoint.Count != endPoint.Count))
                throw new ArgumentException("start and endpoints cannot be null and must be the same length");
            var del = new List<int>(); // absolute deltas * 2, used in error comparisons
            var sgn = new List<int>(); // signs, used to step the coordinates
            var err = new List<int>(); // error counters
            var cur = new List<int>(); // current point
            var length = -1;
            var size = startPoint.Count;
            var k = -1; // index of the largest delta
            for (var i = 0; i < size; ++i)
            {
                var d = endPoint[i] - startPoint[i];
                if (Abs(d) > length)
                {
                    k = i; // save index of longest delta
                    length = Abs(d);
                }
                del.Add(Abs(d) << 1);
                sgn.Add(Sign(d));
                cur.Add(startPoint[i]);
            }

            // create the ei
            var shift = del[k] >> 1;
            for (var i = 0; i < size; ++i)
            {
                var e = -shift;
                if (sgn[i] > 0) e += 1;
                err.Add(e);
            }
            var delK = del[k]; // keep a copy since used so often
            // walk points (length+1 points)
            while (length-- >= 0)
            {
                yield return cur;
                for (var i = 0; i < size; ++i)
                {
                    err[i] += del[i];
                    if (err[i] > 0)
                    {
                        cur[i] += sgn[i];
                        err[i] -= delK;
                    }
                }
            }
        }


        /// <summary>
        ///     Given start and end lattice points in n-dimensional space, walk
        ///     start to destination (inclusive) and choose nearest lattice points,
        ///     similar to line drawing algorithms. Perform given action on each.
        /// </summary>
        /// <param name="startPoint">The start n-dimensional point</param>
        /// <param name="endPoint">The end n-dimensional point</param>
        /// <param name="actionToPerform">The action to perform on each point walked.</param>
        public static void DimN(List<int> startPoint, List<int> endPoint, Action<List<int>> actionToPerform)
        {
            foreach (var pt in DimN(startPoint, endPoint))
                actionToPerform(pt);
        }


        /// <summary>
        /// Iterate over each x,y pair on the line between (x1,y1) and (x2,y2).
        /// Not symmetric - order matters
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static IEnumerable<(int, int)> Dim2Fast(int x1, int y1, int x2, int y2)
        {/*
          Derivation: Chris Lomont
            line is P: (x-x1)(y2-y1) - (y-y1)(x2-x1) = 0
            Set error = this
            Can increment error in each pixel direction
            each step, choose min error from x dir, y dir, or xy dir
          */
            var dx = Abs(x2 - x1);
            var dy = Abs(y2 - y1);
            int sx = -1, sy = -1;
            if (x1 < x2) sx = 1;
            if (y1 < y2) sy = 1;
            var err = dx - dy;

            do
            {
                yield return (x1, y1);
                var e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            } while (x1 != x2 || y1 != y2);

            yield return (x1, y1);

        }

        /// <summary>
        /// Iterate over each x,y pair on the line between (x1,y1) and (x2,y2)
        /// Does so symmetrically: either direction makes the same line
        /// pixel midpoints are rounded towards positive
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        public static IEnumerable<(int, int)> Dim2(int x1, int y1, int x2, int y2)
        {
            /* Derivation: Chris Lomont
             Do 4 cases to handle slopes -1 to 1, varied on dx, dy, on x major
             Two need "<" error comparisons, two need "<=" for symmetry
             For y major, swap meaning of x,y except on output
             Rewrite terms to get cases to have same internal loops
             Reverse sign of e to get all comparisons in same direction
             offset <= case by one to get to < case (or vice versa)
             Merge cases             
             */

            // todo - clean and simplify more

            var (dx, dy) = (x2 - x1, y2 - y1);

            // swap x,y to make slope in [-1,1]
            var swap = Abs(dy) > Abs(dx);
            if (swap)
            {
                (dx, dy) = (dy, dx);
                (x1, y1) = (y1, x1);
                (x2, y2) = (y2, x2);
            }

            var (sx, sy) = (Sign(dx), Sign(dy));
            var n = Max(Abs(dx), Abs(dy)) + 1;

            var (x, y) = (x1, y1);

            // in x major, stores 2*dx* error of (y-yi). is 0 for first pixel
            // NOTE: while merging cases, meaning of e gets negated for some
            // in y major, roles and x and y swapped
            var e = 0;

            // multiplying dx2 by sx*sy lets the code cases below match better
            var (dx2, dy2) = (2 * dx * sx * sy, 2 * dy);
            
            // error comparison for de <= e cases
            var ec = dx * (sx);

            if (
                (0 < dx && 0 <= dy && Abs(dy) <= Abs(dx)) || // slope 0 <= m <= 1
                (dx < 0 && 0 <= dy && Abs(dy) <= Abs(dx)) // slope -1 <= m <= 0
                )

            {  // slope 0 <= m <= 1
            }
            else if (
                // slope 0 <= m <= 1
                (dx < 0 && dy <= 0 && Abs(dy) <= Abs(dx))
                ||
                // slope -1 <= m <= 0
                (0 < dx && dy <= 0 && Abs(dy) <= Abs(dx))
            )
            {
                // reverse e meaning
                (dx2,dy2) = (-dx2,-dy2);
                
                // shift ec to change "<" to "<="
                ec += 1;
            }

            if (swap)
            {
                for (var i = 0; i < n; ++i)
                {
                    yield return new(y, x);
                    x += sx; // move point in dx direction
                    e += dy2; // updated error from move
                    if (ec <= e) // is error too big?
                    {
                        y += sy;  // move point up/down
                        e -= dx2; // error gets adjusted
                    }
                }
            }
            else
            {
                for (var i = 0; i < n; ++i)
                {
                    yield return new(x, y);
                    x += sx; // move point in dx direction
                    e += dy2; // updated error from move
                    if (ec <= e) // is error too big?
                    {
                        y += sy; // move point up/down
                        e -= dx2; // error gets adjusted
                    }
                }
            }
        }


        /// <summary>
        /// Perform action on each x,y pair on the line between (x1,y1) and (x2,y2)
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="x2"></param>
        /// <param name="y2"></param>
        /// <param name="action"></param>
        public static void Dim2(int x1, int y1, int x2, int y2, Action<int, int> action)
        {
            foreach (var (x, y) in Dim2(x1, y1, x2, y2))
                action(x, y);
        }
    }
}
