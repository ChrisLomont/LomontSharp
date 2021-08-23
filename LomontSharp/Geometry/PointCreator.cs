using System;
using System.Collections.Generic;
using Lomont.Numerical;

namespace Lomont.Geometry
{
    // place to make point generators
    // todo - rethink, add more, rename?
    public class PointCreator
    {
        /// <summary>
        /// Make set of points in 3 space
        /// </summary>
        /// <returns></returns>
        public static List<Vec3> MakeCircle(Vec3 center, double radius, int sides, Vec3 normal)
        {
            var pts = new List<Vec3>();

            var m = Mat4.Translation(center) * Mat4.CreateRotation(new Vec3(0, 0, 1), normal);

            // create in x-y plane, move to location on output
            for (var i = 0; i < sides; ++i)
            {
                var a1 = (double)i / sides * Math.PI * 2;
                var p1 = new Vec3(Math.Cos(a1), Math.Sin(a1), 0);
                p1 *= radius;
                p1 = m * p1;
                pts.Add(p1);
            }

            return pts;
        }



    }
}
