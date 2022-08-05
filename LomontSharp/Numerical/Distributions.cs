using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.Numerical
{
    public static class Distributions
    {


        /// <summary>
        /// Compute two normal (Gaussian) values from two uniform random numbers in 0,1
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static (double,double) Gaussian2(double u, double v, double mean = 0, double stdDev = 1.0)
        { // box-mueller
            var ss = Math.Sqrt(-2 * Math.Log(u));
            var a = Math.PI * 2 * v;
            var x = ss * Math.Cos(a);
            var y = ss * Math.Sin(a);

            // to mean and std dev
            x = x * stdDev + mean;
            y = x * stdDev + mean;

            return (x,y);
        }
        /// <summary>
        /// Compute a normal (Gaussian) value from two uniform random numbers in 0,1
        /// </summary>
        /// <param name="u"></param>
        /// <param name="v"></param>
        /// <param name="mean"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public static double Gaussian(double u, double v, double mean = 0, double stdDev = 1.0)
        {
            var (x, _) = Gaussian2(u, v, mean, stdDev);
            return x;
        }
    }
}
