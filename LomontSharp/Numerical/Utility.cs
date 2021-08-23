using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lomont.Numerical
{
    /// <summary>
    /// todo - oraganize these better
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Perform Kahan summation, which is more accurate than naive summation
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double KahanSum(IEnumerable<double> values)
        {
            var sum = 0.0; // the accumulator.
            var c   = 0.0; // a running compensation for lost low-order bits.

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

        static public bool isFuzzyClose(double a, double b)
        {
            return Math.Abs(a - b) < 1e-5; // todo make better later
        }

        // as t goes ax to bx, lerp goes ay to by
        public static double Lerp(double ax, double ay, double bx, double by, double t)
        {
            return
                t < ax ? ay : (
                    bx < t ? by : (
                        isFuzzyClose(bx, ax) ? (ay + by) / 2 : (
                            (by - ay) * (t - ax) / (bx - ax) + ay
                        )));
        }

        public static double LinearInterpolate(double a, double b, double value)
        {
            return a * value + b * (1 - value);
        }


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


        // todo - move elsewhere;

        /// <summary>
        /// compute the nth term in a sub-random sequence in [min,max] given
        /// the initial value. A sub-random sequence covers the space nicely and uniformly,
        /// and looks better for generating colors, for example.
        /// http://en.wikipedia.org/wiki/Low-discrepancy_sequence
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

        public static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double ToDegrees(double radians, bool bounds)
        {
            var deg = 180 * radians / Math.PI;
            if (bounds)
            {
                while (360 <= deg) deg -= 360;
                while (deg < 0) deg += 360;
            }
            return deg;
        }


        public static double Clamp(double v, double a, double b)
        {
            if (v < a) return a;
            if (b < v) return b;
            return v;
        }
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



    }
}
