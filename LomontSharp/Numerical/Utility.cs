using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static System.Math;

namespace Lomont.Numerical
{
    /// <summary>
    /// Basic numerical utility functions
    /// </summary>
    public static class Utility
    {

        #region tolerance comparisons
        public static bool IsFuzzyClose(double a, double b)
        {
            return Math.Abs(a - b) < 1e-5; // todo make better later
        }
        /// <summary>
        /// Compare doubles for being close
        /// A terrifically complex task. perhaps remove and rethink?
        /// Wants: IsClose(a,b) == IsClose(a-b,0), etc.
        /// 
        /// Idea is that if some value is computed via different methods, there may be error in the low bits.
        /// This compares float up to some multiple of eps.
        /// 
        /// Todo - analyze carefully. 
        /// 
        /// Sources:
        /// Knuth, Semi-numerical Algorithms TAOCP, Section 4.2.2
        /// boost: http://www.boost.org/doc/libs/1_63_0/libs/math/doc/html/math_toolkit/float_comparison.html
        /// https://randomascii.wordpress.com/2012/02/25/comparing-floating-point-numbers-2012-edition/ for nuances
        /// https://news.ycombinator.com/item?id=13998564 for discussion of another post
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool AreClose(double a, double b, int epsMult = 50)
        {
            var eps = Double.Epsilon;
            var aAbs = Abs(a);
            var bAbs = Abs(b);
            var maxval = Max(aAbs, bAbs);
            // check for near zero to near zero compares, in which case don't want relative checks, but absolute
            if (maxval < epsMult * eps)
                return true;
            // NOTE: Need <= not < here to allow zero to match zero if the abs check above removed
            return Abs(a - b) <= eps * maxval * epsMult;
        }
        #endregion

        #region Interpolation

        // as t goes ax to bx, lerp goes ay to by
        public static double Lerp(double ax, double ay, double bx, double by, double t)
        {
            return
                t < ax ? ay : (
                    bx < t ? by : (
                        IsFuzzyClose(bx, ax) ? (ay + by) / 2 : (
                            (by - ay) * (t - ax) / (bx - ax) + ay
                        )));
        }

        public static double LinearInterpolate(double a, double b, double value) => a * value + b * (1 - value);
        #endregion

        #region Mod

        /// <summary>
        /// for b!=0, return c so that there is an integral r such that 
        /// |a| = r |b| +c and 0 le c lt |b|.
        /// If b==0, return 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double PositiveMod(double a, double b)
        {
            if (b == 0) return 0;
            a = Math.Abs(a);
            b = Math.Abs(b);
            var r = Math.Floor(a / b);
            return a - r * b;
        }
        /// <summary>
        /// for b!=0, return c so that there is an integral r such that 
        /// |a| = r |b| +c and 0 le c lt |b|.
        /// If b==0, return 0
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int PositiveMod(int a, int b)
        {
            if (b == 0) return 0;
            a = Math.Abs(a);
            b = Math.Abs(b);
            var r = a / b;
            return a - r * b;
        }
        #endregion

        #region Angles
        /// <summary>
        /// Degrees to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double ToRadians(double degrees) => degrees * Math.PI / 180.0;

        /// <summary>
        /// Convert radians to degrees. Optionally wrap to [0,360)
        /// </summary>
        /// <param name="radians"></param>
        /// <param name="bounds"></param>
        /// <returns></returns>
        public static double ToDegrees(double radians, bool bounds = false)
        {
            var deg = 180 * radians / Math.PI;


            if (bounds)
            {
                // IEEERem(x,y) = x-(yQ) where Q is round to even of x/y
                deg = Math.IEEERemainder(deg, 360.0);
                if (deg == -0) deg = 0; // remove a case that might bite people. IEEE 754 has bit pattern for -0
                if (deg < 0) deg += 360.0;
                Debug.Assert(0<=deg && deg < 360.0);
            }
            return deg;
        }
        #endregion

        #region clamps, wraps

        /// <summary>
        /// Clamp v to [a,b]
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Clamp(double v, double a, double b)
        {
            if (v < a) return a;
            if (b < v) return b;
            return v;
        }
        
        /// <summary>
        /// Clamp value into [0,1]
        /// </summary>
        /// <param name="value"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static double Clamp01(double value, double tolerance = 0.00001)
        {
            if (0 <= value && value <= 1) return value;
            if (value < 0 && -tolerance < value) return 0;
            if (value > 1 && 1 + tolerance > value) return 1;

            Trace.TraceError("Clamp01 color parse error");
            throw new Exception("Value out of tolerance");
        }

        /// <summary>
        /// Clamp v to [a,b]
        /// </summary>
        /// <param name="v"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int Clamp(int v, int a, int b)
        {
            if (v < a) return a;
            if (b < v) return b;
            return v;
        }

        /// <summary>
        /// Return value wrapped into [0,1)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Wrap01(double value)
        {
            // todo - prove correct carefully
            return value - Math.Floor(value);
        }

        /// <summary>
        /// Return value wrapped into [min, max)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static double Wrap(double value ,double min, double max)
        {
            if (max == min) return min; // collapse to point
            // todo - prove correct carefully
            var del = max - min;
            var scaled = (value - min) / del; // shifted and scaled so min = 0, max = 1
            var wrapped = Wrap01(scaled);
            var ans = wrapped * del + min;
            Trace.Assert(min <= ans && ans < max);
            return ans;

        }
        #endregion

        #region special functions and sequences

        /// <summary>
        /// Perform Kahan summation, which is more accurate than naive summation
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double KahanSum(IEnumerable<double> values)
        {
            var sum = 0.0; // the accumulator.
            var c = 0.0; // a running compensation for lost low-order bits.

            foreach (var v in values)
            {
                var y = v - c;     // c is zero the first time around.
                var t = sum + y;   // Alas, sum is big, y small, so low-order digits of y are lost.
                c = (t - sum) - y; // (t - sum) cancels the high-order part of y; subtracting y recovers negative (low part of y)
                sum = t;           // Algebraically, c should always be zero. Beware overly-aggressive optimizing compilers!
                // Next time around, the lost low part will be added to y in a fresh attempt.
            }

            return sum;
        }

        /// <summary>
        /// compute the nth term in a sub-random sequence in [min,max] given
        /// the initial value. A sub-random sequence covers the space nicely and uniformly,
        /// and looks better for generating colors, for example.
        /// http://en.wikipedia.org/wiki/Low-discrepancy_sequence
        ///
        /// TODO - move elsewhere, to random areas?
        /// </summary>
        /// <param name="startValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static double LowDiscrepancySequence(double startValue, double minValue, double maxValue, int term)
        {
            // 2/(1+sqrt(5)) = 1-phi = 1/phi = 0.61803.... 
            const double goldenMean = 0.618033988749894848204586834366;
            var delta = maxValue - minValue;
            return PositiveMod(goldenMean * term * delta + startValue - minValue, delta) + minValue;
        }



        /// <summary>
        /// compute log(1+x) without losing precision for small values of x
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double LogOnePlusX(double x)
        {
        // https://www.johndcook.com/blog/2012/07/25/trick-for-computing-log1x/
        // https://docs.oracle.com/cd/E19957-01/806-3568/ncg_goldberg.html
        // https://www.johndcook.com/blog/2010/06/07/math-library-functions-that-seem-unnecessary/

            // todo - analyze properly

            if (x <= -1.0)
            {
                string msg = String.Format("Invalid input argument: {0}", x);
                throw new ArgumentOutOfRangeException(msg);
            }

            if (Math.Abs(x) > 1e-4)
            {
                // x is large enough that the obvious evaluation is OK
                return Math.Log(1.0 + x);
            }

            // Use Taylor approx. log(1 + x) = x - x^2/2 with error roughly x^3/3
            // Since |x| < 10^-4, |x|^3 < 10^-12, relative error less than 10^-8

            return (-0.5 * x + 1.0) * x;
        }

        #endregion

        #region Combinatorial

        static List<double> Logarithms = new List<double>();

        /// <summary>
        /// Compute Multinomial
        /// (n1+n2+...)!/ n1! n2! n3!...
        /// </summary>
        /// <param name="ni"></param>
        /// <returns></returns>
        public static ulong Multinomial(params long [] ni)
        {
            var totalNi = ni.Sum();

            double sum = 0;
            for (var i = 2; i <= totalNi; i++)
            {
                if (i - 2 < Logarithms.Count)
                    sum += Logarithms[i - 2];
                else
                {
                    var log = Math.Log(i);
                    Logarithms.Add(log);
                    sum += log;
                }
            }

            foreach (var number in ni)
                for (int i = 2; i <= number; i++)
                    sum -= Logarithms[i - 2];

            // todo - rounding like this is problematic for floats - can get errors...
            return (ulong)(Math.Exp(sum)+0.5);
        }

        /// <summary>
        /// Compute factorial, up to around 20!
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static long Factorial(int n)
        {
            long v = 1;
            for (long nn = 1; nn <= n; ++nn)
                v *= nn;
            return v;
        }

        /// <summary>
        /// Binomial coefficient nCk, computed with doubles
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static double BinomialDouble(long n, long k)
        {
            // todo - extend to negative k
            if (n == k || k == 0) return 1;
            if (0 <= n && n < k) return 0;

            double result = 1, sign = 1;

            if (n < 0)
            {
                sign = ((k & 1) == 0) ? 1 : -1;
                n = k - n - 1;
            }

            // C(n, k) = C(n, n-k)
            if (k > n - k)
                k = n - k;

            // Calculate value of [n * ( n - 1) *---* (
            // n - k + 1)] / [k * (k - 1) *----* 1]
            // n*(n-1)*(n-2)*...(n-k+1) / (k*(k-1)*(k-2)*...*2*1)
            // Note that the numerator, when dividing by i+1, has i+1 terms, so always works
            for (var i = 0; i < k; ++i)
            {
                result *= n - i;
                result /= i + 1;
            }
            return result * sign;
        }


        /// <summary>
        /// Binomial coefficient nCk
        /// </summary>
        /// <param name="n"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static long Binomial(long n, long k)
        {
            // todo - extend to negative k
            if (n == k || k == 0) return 1;
            if (0 <= n && n < k) return 0;

            long result = 1, sign=1;

            if (n < 0)
            {
                sign = ((k&1)==0)?1:-1;
                n = k - n - 1;
            }

            // C(n, k) = C(n, n-k)
            if (k > n - k)
                k = n - k;

                // Calculate value of [n * ( n - 1) *---* (
                // n - k + 1)] / [k * (k - 1) *----* 1]
                // n*(n-1)*(n-2)*...(n-k+1) / (k*(k-1)*(k-2)*...*2*1)
                // Note that the numerator, when dividing by i+1, has i+1 terms, so always works
                for (var i = 0; i < k; ++i)
                {
                    result *= n - i;
                    result /= i + 1;
                }
                return result * sign;
        }
        #endregion

    }
}
