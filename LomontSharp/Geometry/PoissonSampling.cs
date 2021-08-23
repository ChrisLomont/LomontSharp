using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lomont.Numerical;
using static Lomont.Numerical.Utility;

namespace Lomont.Geometry
{
#if true
    /// <summary>
    /// Poisson sample a region
    /// </summary>
    public class PoissonSampling
    {
        private const int dimension = 2; // make 3D for later

        // background n dimensional grid
        // each entry holds index of sample
        private int[,] grid;
        private int gridW, gridH;
        private double maxx, minx, maxy, miny;
        private double cellsize;
        private List<Vec2> points;
        private List<int> activeList;

        /// <summary>
        /// Compute grid index for a point
        /// </summary>
        /// <param name="p"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        (int i, int j) GridIndex(Vec2 p)
        {
            var (x, y) = p;
            var i = (int)Math.Floor((x - minx) / cellsize);
            var j = (int)Math.Floor((y - miny) / cellsize);
            i = Clamp(i, 0, gridW - 1);
            j = Clamp(j, 0, gridH - 1);
            return (i, j);
        }

        // insert in grid, return index
        // throw if error
        int Insert(Vec2 p)
        {
            var index = points.Count;
            points.Add(p);
            var (i, j) = GridIndex(p);
            grid[i, j] = index;
            return index;
        }

        Vec2 ClosestPoint(Vec2 pt)
        {
            var (i, j) = GridIndex(pt);
            if (grid[i, j] != -1)
                return points[grid[i, j]];

            // scan out from grid, finding closest point
            var bestDist = (double)(1 << 25);
            Vec2 bestPt = new Vec2(bestDist, bestDist);
            var maxS = Int32.MaxValue / 10; //largest allowed box

            for (var s = 1; s <= maxS; ++s)
            {
                for (var x = Math.Max(0, i - s); x <= Math.Min(gridW - 1, i + s); ++x)
                {
                    Test(x, j - s, s);
                    Test(x, j + s, s);
                }

                for (var y = Math.Max(0, j - s); y <= Math.Min(gridH - 1, j + s); ++y)
                {
                    Test(i - s, y, s);
                    Test(i + s, y, s);
                }
            }

            return bestPt;

            // local testing function
            void Test(int x, int y, int s)
            {
                if (x < 0 || y < 0 || gridW <= x || gridH <= y)
                    return;
                var gp = grid[x, y];
                if (gp == -1)
                    return;
                var pt1 = points[grid[x, y]];
                var dist = (pt1 - pt).Length;
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPt = pt1;
                    if (maxS > Int32.MaxValue / 20)
                        maxS = 2 * s; // limit search once one found
                }
            }

        }


        // Generate random uniform point from pt in range r to 2r
        Vec2 GeneratePoint(Random rand, Vec2 pt, double r)
        {
            while (true)
            {
                // generate in square, then test to annulus
                var x = pt.X + rand.NextDouble() * 4 * r - 2 * r;
                var y = pt.Y + rand.NextDouble() * 4 * r - 2 * r;
                var newPt = new Vec2(x, y);
                var dist = (newPt - pt).Length;
                if (r <= dist && dist < 2 * r)
                    return newPt;
            }
        }

        // cover edge in sample points
        // avoid corners, filled elsewhere
        void CoverEdge(Vec2 p1, Vec2 p2, double minDist)
        {
            var dist = (p1 - p2).Length;
            var num = (int)Math.Floor(dist / (minDist * 2)); // space them out a bit
            var delta = dist / num; // spacing

            // num is one too many if not hitting corners
            for (var t = 1; t < num; ++t)
                Insert(p1 + t * delta * (p2 - p1));
        }




        /// <summary>
        /// Create samples for the given domain
        /// Samples corners and boundary for solid coverage
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="minDist"></param>
        /// <param name="rejectionCount"></param>
        /// <returns></returns>
        public List<Vec2> CreateSamples(
            List<Vec2> domain,
            double minDist,
            int rejectionCount = 30,
            bool fillToEdge = true
        )
        {
            // Implement: Fast Poisson Disk Sampling in Arbitrary Dimensions, Bridson, 2007
            var k = rejectionCount; // from paper
            var r = minDist;
            var n = dimension;

            // step 0 - cellsize so contains at most one entry
            cellsize = r / Math.Sqrt(n);

            // cover area in rectangle
            minx = domain.Min(p => p.X);
            maxx = domain.Max(p => p.X);
            miny = domain.Min(p => p.Y);
            maxy = domain.Max(p => p.Y);

            gridW = (int)Math.Ceiling((maxx - minx) / cellsize);
            gridH = (int)Math.Ceiling((maxy - miny) / cellsize);
            grid = new int[gridW, gridH];
            // initialize cell indices to -1, unused
            for (var j = 0; j < gridH; ++j)
            for (var i = 0; i < gridW; ++i)
                grid[i, j] = -1;

            points = new List<Vec2>();

            // step 0.5
            // place corners and edge points
            if (fillToEdge)
            {
                for (var i = 0; i < domain.Count; ++i)
                {
                    var p1 = domain[i];
                    var p2 = domain[(i + 1) % domain.Count];

                    // set corners
                    Insert(p1);

                    // todo - add these
                    // CoverEdge(p1, p2, minDist);
                }
            }


            // step 1 - initial point uniformly in domain
            // todo - make random repeatable, global source, make uniform less prone to multiple misses
            Random rand = new Random(1234);

            Vec2 x0;
            do
            {
                x0 = new Vec2(minx + rand.NextDouble() * (maxx - minx), miny + rand.NextDouble() * (maxy - miny));
            } while (!Utility.PointInPolygon(x0, domain));

            // todo - possible x0 too close to edge points - must check that too!

            var index = Insert(x0);
            activeList = new List<int> { index };

            // step 2
            while (activeList.Any())
            {
                // pick one item (basically on boundary)
                var pickIndex = rand.Next(activeList.Count);
                var i = activeList[pickIndex];
                // generate up to k nearby in r to 2r radius
                var pass = 0;
                var pt = points[i];
                while (pass < k)
                {
                    var newPt = GeneratePoint(rand, pt, r);
                    var d = (ClosestPoint(newPt) - newPt).Length;
                    if (d >= r && Utility.PointInPolygon(newPt, domain))
                    {
                        var newIndex = Insert(newPt);
                        activeList.Add(newIndex);
                        break; // successful
                    }

                    pass++;
                }

                if (pass == k) // did not find
                    activeList.Remove(i);
            }

            return points;
        }
    }
#endif
}
