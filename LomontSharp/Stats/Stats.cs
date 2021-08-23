using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Math;

namespace Lomont.Stats
{
    public static class Stats
    {
        // Some algorithm in here follow ideas from Higham, Accuracy and Stability of Numerical Algorithms
        // todo - redo with kahan summation
        // todo - add knuth incremental versions?
        // todo - see Higham, section 1.9, for options 
        
        /// <summary>
        /// The arithmetic mean of a sample
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Mean(IList<double> values)
        {
            var n = values.Count;
            if (n==0) return 0;
            var sum = Numerical.Utility.KahanSum(values);
            return sum / n;

        }

        /// <summary>
        /// Compute the mean. Sorts the list.
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double Median(List<double> values)
        {
            values.Sort();
            var n = values.Count;
            return values[n/2];
        }

        /// <summary>
        /// Compute sample standard deviation.
        /// Used when the data is a subsample of a population
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double SampleStdDev(IList<double> values) => Sqrt(SampleVariance(values));
        
        /// <summary>
        /// Compute population standard deviation.
        /// Used when the data is a complete population
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double PopulationStdDev(IList<double> values) => Sqrt(PopulationVariance(values));
        
        /// <summary>
        /// Compute population variance.
        /// Used when the data is a subsample of a population
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double SampleVariance(IList<double> values)
        {
            // sigma^2 = 1/N * Sum(xi-u)^2
            var N = values.Count;
            var u = Mean(values);
            var sum = 0.0;
            for (var i = 0; i < N; ++i)
            { // todo - use higham, or kahan summation
                var dx = values[i] - u;
                sum += dx * dx;
            }
            return sum / (N-1); // note N-1 here for sample variance
        }
        
        /// <summary>
        /// Compute population variance.
        /// Used when the data is a complete population
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double PopulationVariance(IList<double> values)
        {
            // sigma^2 = 1/N * Sum(xi-u)^2
            var N = values.Count;
            var u = Mean(values);
            var sum = 0.0;
            for (var i = 0; i < N; ++i)
            { // todo - use higham, or kahan summation
                var dx = values[i] - u;
                sum += dx * dx;
            }
            return sum / N;
        }

    }
}
