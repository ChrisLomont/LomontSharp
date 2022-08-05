using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace Lomont.Numerical
{
    /// <summary>
    /// Class to find roots of small degree polynomials
    /// </summary>
    public class SmallDegreePolynomialRoots
    {
        // todo - implement Blinn (graphics guy) papers - 5 or 6 of them
        // todo - characterize error versus accuracy see tests, write up

        // todo - use herbie automatic float code analyser to help
        // https://herbie.uwplse.org/
        // todo - leverage System.Math.FusedMultiplyAdd()

        // see 
        // http://marc-b-reynolds.github.io/math/2020/01/10/Quadratic.html
        // for fused multiply, leveraging kahan 2x2 determinant
        // 



        static double eps = 1e-12; // todo input this?

        // return x*y+z using fma for low error
        static double fma(double x, double y, double z) => System.Math.FusedMultiplyAdd(x, y, z);

        /// <summary>
        /// Kahan algo for low error 2x2 det ab-cd
        /// +/- 3/2 ulp
        /// +/- 1 ulp if sign ab != sign cd
        /// </summary>
        static double det2(double a, double b, double c, double d)
        {
            var t = c * d; // one product
            var e = fma(c, d, -t); // some extra bits
            var f = fma(a, b, -t); // ab - cd
            return f - e; // ab-cd and error fixup
        }

        /// <summary>
        /// Solve quadratic a*x^2 + b*x + c == 0
        /// 
        /// Uses method of Blinn, returning homogeneous coords x:w for each, handling degenerate cases better.
        /// Also is designed to move roots smoothly as parameters change, which many algorithms do not.
        /// Better for animation purposes, for example.
        /// 
        /// if roots real, roots is [x1,w1,x2,w2] where xi:wi are homogeneous coords
        /// if roots complex, roots is [x1,w1,x2,w2] where xi:wi are homogeneous coords and roots are 
        /// (x1:w1) ± i* (x2:w2)
        /// 
        /// 2 real roots (possibly double), return 2
        /// 2 complex roots return -2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="roots"></param>
        /// <returns></returns>
        public static int SolveQuadraticBlinn(double a, double b, double c, double[] roots)
        {
            // todo - does not handle the numerical issue of cancellation correctly!
            // todo - or does it? :)

            b = b / 2; // Blinn uses format ax^2 + 2bx + c

            var d = det2(b, b, a, c); // compute b*b-a*c (Note Blinn b rescaling removed the normal 4 in 4ac)
            var r = Math.Sqrt(Math.Abs(d)); // positive root
            double x1, w1, x2, w2; // homog coords
            if (d >= 0)
            {
                if (b > 0)
                {
                    (x1, w1) = (-c, b + r);
                    (x2, w2) = (-b - r, a);
                }
                else if (b < 0)
                {
                    (x1, w1) = (-b + r, a);
                    (x2, w2) = (c, -b + r);
                }
                else // b == 0, todo - check close to zero and hit?
                {
                    if (Math.Abs(a) >= Math.Abs(c))
                    {
                        (x1, w1) = (r, a);
                        (x2, w2) = (-r, a);
                    }
                    else
                    {
                        (x1, w1) = (-c, r);
                        (x2, w2) = (c, r);
                    }
                }
            }
            else
            { // d < 0 case, imaginary roots
                // todo - ensure root movement smooth on parameters?
                if (b > 0)
                {
                    // (x1, w1) = (-c, b + r);
                    // (x2, w2) = (-b - r, a);
                    (x1, w1) = (-b, a);
                    (x2, w2) = (r, a);
                }
                else if (b < 0)
                {
                    //(x1, w1) = (-b + r, a);
                    //(x2, w2) = (c, -b + r);
                    (x1, w1) = (-b, a);
                    (x2, w2) = (r, a);
                }
                else // b == 0, todo - check close to zero and hit?
                {
                    if (Math.Abs(a) >= Math.Abs(c))
                    {
                        (x1, w1) = (0, 1);
                        (x2, w2) = (r, a);
                    }
                    else
                    {
                        (x1, w1) = (0, 1);
                        (x2, w2) = (c, r);
                    }
                }
            }

            (roots[0], roots[1], roots[2], roots[3]) = (x1, w1, x2, w2);

            return (d < 0) ? -2 : 2;
        }


        /// <summary>
        /// Solve quadratic a*x^2 + b*x + c == 0
        /// 2 real roots (possibly double), return 2
        /// 2 complex roots roots[0] ± i*roots[1], return -2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="roots"></param>
        /// <returns></returns>
        public static int SolveQuadratic(double a, double b, double c, double[] roots)
        {
            // numerical stability
            // https://math.stackexchange.com/questions/866331/numerically-stable-algorithm-for-solving-the-quadratic-equation-when-a-is-very
            // Kahan notes "On the Cost of Floating-Point Computation Without Extra-Precise Arithmetic"
            // https://people.eecs.berkeley.edu/~wkahan/Qdrtcs.pdf
            // todo - incorporate Kahan stuff, but would make code very complex....

            // todo - handle:
            // a = 0 - linear
            //   sub cases b=0, b=c=0
            // c = 0, pull out x=0 root, other -b/a
            // overflow on disc computing
            // infinite roots (no answer, such as a=b=c=0) return 3
            // no answer (a=b=0,c!=0) return 0;

            var r4 = new double[4];
            var retval = SolveQuadraticBlinn(a, b, c, r4);
            roots[0] = r4[0] / r4[1]; // todo - handle degenerate cases here, output retrn values?
            roots[1] = r4[2] / r4[3];
            return retval;
        }

        /// <summary>
        /// Solve cubic ax^3 + bx^2 + cx + d == 0
        /// 3 real distinct roots => roots[0], roots[1], roots[2], return 3
        /// 3 real roots: a single root and a double root => roots[0], roots[1] = roots[2] = double root, return 2
        /// 1 real (triple) root  => roots[0] = roots[1] = roots[2] = 0, return 1
        /// 1 real root & 2 complex roots => roots[0], roots[1] ± roots[2], return -2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="roots"></param>
        /// <returns></returns>
        public static int SolveCubic(double a, double b, double c, double d, double[] roots)
        {

            // todo - check things, throw errors, is not numerically optimized by far

            // x -> t-b/3a gives "depressed" cubic t^3+p*t+q
            // p = ...
            // q = ...

            // discriminant of depressed cubic
            // D = -(4p^3+27q^2)
            // D > 0 is 3 real roots
            // D < 0 is 1 real, 2 complex roots
            // D = 0 is duplicate roots (p=0 => q = 0 and is triple root, if p != 0 then a simple root)

            // viete method : https://en.wikipedia.org/wiki/Cubic_equation
            // 3 real roots
            // tk (for k=0,1,2) = 2sqrt(-p/3)cos(acos((3q/2p)sqrt(-3/p))/3 - 2pi k/3)
            // one real root 

            // todo - remove ops in computation

            // original p,q meaning, (a,b,c,d)->(1,a,b,c):
            var p = (3 * a * c - b * b) / (3 * a * a);
            var q = (2 * b * b * b - 9 * a * b * c + 27 * a * a * d) / (27 * a * a * a);

            // discriminant of depressed cubic
            var D = -(4 * p * p * p + 27 * q * q);

            // todo: for large values of p & q, should get better comparison for D....

            var (a2, a1, a0) = (b / a, c / a, d / a);
            q = a1 / 3 - a2 * a2 / 9;
            var r = (a1 * a2 - 3 * a0) / 6 - a2 * a2 * a2 / 27;

            // these fail sometimes.... hard to tell which side of zero for large values, if identically 0, etc...
            if (r * r + q * q * q <= 0) // (D > 0)
            { // three distinct real roots
              // viete method
                var th = q == 0 ? 0 : Math.Acos(r / (Math.Pow(-q, 3.0 / 2.0))); // angle in 0 to pi
                var th1 = th / 3;
                var th2 = th1 + Math.PI * 2 / 3;
                var th3 = th1 - Math.PI * 2 / 3;
                roots[0] = 2 * Math.Sqrt(-q) * Math.Cos(th1) - a2 / 3;
                roots[1] = 2 * Math.Sqrt(-q) * Math.Cos(th2) - a2 / 3;
                roots[2] = 2 * Math.Sqrt(-q) * Math.Cos(th3) - a2 / 3;
                return 3;
                // ordered? roots[2] <= roots[1] <= roots[0]?
            }
            else
            { // one real root, two complex conjugate roots
                // numerical recipes form
                var A = Math.Pow(Math.Abs(r) + Math.Sqrt(r * r + q * q * q), 1.0 / 3.0);
                var t1 = r >= 0 ? A - q / A : q / A - A;
                roots[0] = t1 - a2 / 3; // real
                var xi = -t1 / 2 - a2 / 3;
                var yi = Math.Sqrt(3.0) / 2.0 * (A + q / A);
                roots[1] = xi; // real part
                roots[2] = yi; // imaginary part
                return -2;
            }

            throw new NotImplementedException("Invalid position");
#if false // todo - remove

            // simplify all this... relabel p,q meaning

            var a2 = a * a;
            var p = (a2 - 3 * b) / 9;
            var p3 = p * p * p;
            var q = (a * (2 * a2 - 9 * b) + 27 * c) / 54;
            var q2 = q * q;
            if (
                (Math.Abs(p3) > eps && Math.Abs(q2 / p3 - 1) < 1e-10) // todo - tolerance here too hueristic-y
                || (Math.Abs(p3 - q2) < eps)
                )
            {
                // q2 really close to p3, both may be large in value
                // disc = 0, multiple roots, necessarily real
                // if p = 0, then q = = 0 and triple root
                if (p == 0)
                {
                    // roots all 0 for depressed
                    roots[0] = roots[1] = roots[2] = 0;
                    return 1;
                }
                else
                { // one real distinct, double real


                    var tt = a / (3 * 1);

                    roots[0] = 3 * q / p - tt;
                    roots[1] = roots[2] = -3 * q / (2 * p) - tt;
                    return 2;
                }
            }
            else if (q2 < p3) // check discriminant
            { // real distinct root case
                var s0 = Math.Acos(Math.Clamp(q / Math.Sqrt(p3), -1, 1)) / 3.0;
                var s1 = Math.PI * 2 / 3;
                a /= 3; p = -2 * Math.Sqrt(p);
                roots[0] = p * Math.Cos(s0) - a;
                roots[1] = p * Math.Cos(s0 + s1) - a;
                roots[2] = p * Math.Cos(s0 - s1) - a;
                return 3;
            }
            else
            {   // vieta substitution to avoid computing multiple cube roots
                // W = -(q/2) ± sqrt(p^3/27 + q^2/4)
                // w1,w2,w3 cube roots of W, original depressed roots then wj-p/(3wj) for j=0,1,2

                var s0 = -Math.Pow(Math.Abs(q) + Math.Sqrt(q2 - p3), 1.0 / 3);
                if (q < 0) s0 = -s0;
                var s1 = 0 == s0 ? 0 : p / s0;
                var s2 = s0 + s1;
                a /= 3;

                roots[0] = s2 - a;
                roots[1] = -0.5 * s2 - a;
                roots[2] = 0.5 * Math.Sqrt(3.0) * (s0 - s1);
                if (Math.Abs(roots[2]) < eps)
                {
                    roots[2] = roots[1];
                    return 2;
                }

                return -2; // 1 real, 2 complex
            }
#endif
        }

        /// <summary>
        /// Solve quartic a*x^4 + b*x^3 + c*x^2 + d*x + e
        /// Returns 4 complex numbers
        /// returns int, 
        /// bit0 = 0 => real roots in roots[0,1], else complex roots in [0,1]
        /// bit1 = 0 => real roots in roots[2,3], else complex roots in [2,3]
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="roots"></param>
        /// <returns></returns>
        public static int SolveQuartic(double a, double b, double c, double d, double e, Complex[] roots)
        {
            // https://en.wikipedia.org/wiki/Quartic_equation

            // Ferrari method,. modified

            // sub x = t-B/(4A)
            // gives depressed quartic t^4 + p t^2 + q t + r  (no cubic term)
            //var p = -3 * b * b / (8 * a * a) + (c / a);
            //var q = (b * b * b) / (8 * a * a * a) + (-b * c) / (2 * a * a) + (d / a);
            //var r = (-3 * b * b * b * b) / (256 * a * a * a * a) + (c * b * b) / (16 * a * a * a) + (-b * d) / (4 * a * a) + (e / a);

            var (A3, A2, A1, A0) = (b / a, c / a, d / a, e / a);
            var C = A3 / 4;
            var b2 = A2 - 6 * C * C;
            var b1 = A1 - 2 * A2 * C + 8 * C * C * C;
            var b0 = A0 - A1 * C + A2 * C * C - 3 * C * C * C * C;

            var cubicRoots = new double[3];
            var cubicType = SolveCubic(1.0, b2, b2 * b2 / 4 - b0, -b1 * b1 / 8.0, cubicRoots);
            // get largest root
            var m = cubicRoots[0];
            if (cubicType > 0)
            {
                m = Math.Max(m, cubicRoots[1]);
                m = Math.Max(m, cubicRoots[2]);
            }
            if (m < 0) m = 0;

            var E = b1 > 0 ? 1.0 : -1.0;

            var d2 = m * m + b2 * m + b2 * b2 / 4.0 - b0;
            if (-eps < d2 && d2 < 0) // need to chop
                d2 = 0;
            Trace.Assert(d2 >= 0);
            var R = E * Math.Sqrt(d2);

            var retval = Roots(m, b2, R, C, -1, roots, 0);
            retval |= Roots(m, b2, R, C, 1, roots, 2) << 1;

            // return 0 if both real, else 1 if both complex
            static int Roots(double m, double b2, double R, double C, int s, Complex[] roots, int start)
            {
                Trace.Assert(m >= 0);
                var mr = s * Math.Sqrt(m / 2) - C;
                var d = -m / 2 - b2 / 2 - s * R;
                if (d >= 0)
                { // real
                    var rr = Math.Sqrt(d);
                    roots[0 + start] = mr + rr;
                    roots[1 + start] = mr - rr;
                    return 0;
                }
                else
                { // 2 complex
                    var rr = Math.Sqrt(-d);
                    roots[0 + start] = new Complex(mr, rr);
                    roots[1 + start] = new Complex(mr, -rr);
                    return 1;
                }
            }

            return retval;
        }

        public static void Test()
        {
            // kahan examples: quadratic - see paper above
#if false
A B C true D true roots computed D computed roots
10.27 29.61 85.37 0.0022 2.88772… 2.87859… 0.1000 2.914 2.852
10.28 29.62 85.34 0.0492 2.90290… 2.86075… 0 2.881 2.881
Changes in roots: +0.01518… –0.01784… –0.033 +0.029
            
A B C true D true roots computed D computed roots
94906265.625 94906267.000 94906268.375 1.89… 1.000000028975958…
1.0
0.0 1.000000014487979
1.000000014487979
94906266.375 94906267.375 94906268.375 1.0 1.000000021073424…
1.0
2.0 1.000000025437873
0.999999995635551
Changes in roots: -0.000000007902534…
0
+0.000000010949894
–0.000000018852428
            
#endif

            var r = new Random(1234);
            var range = double.MaxValue; // roots in ± this, coeff a in this
            Func<double> v = () => r.NextDouble() * 2 * range - range;

            TestQuadratic(100000);
            TestCubic(10000);
            TestQuartic(10000);
            TestBlinnQuadratic();

            void TestQuartic(int passes)
            {
                range = 1e6;

                var err = 1e-5; // error to allow
                var roots = new Complex[4];
                for (int i = 0; i < passes; ++i)
                {
                    // todo - test complex roots, bad cases from elsewhere

                    // roots 
                    var r0 = v();
                    var r1 = v();
                    var r2 = v();
                    var r3 = v();
                    var a = v();

                    double b = 0, c = 0, d = 0, e = 0;

                    var type = r.Next(2);
                    //type = 0; // todo
                    switch (type)
                    {
                        case 0: // 4 real roots
                            // symmetric polynomials
                            (a, r0, r1, r2, r3) = (1, 1, 2, 3, 4); // todo
                            b = -(r0 + r1 + r2 + r3);
                            c = (r0 * r1 + r0 * r2 + r0 * r3 + r1 * r2 + r1 * r3 + r2 * r3);
                            d = -(r1 * r2 * r3 + r0 * r2 * r3 + r0 * r1 * r3 + r0 * r1 * r2);
                            e = (r0 * r1 * r2 * r3);
                            break;
                        case 1: // 2 complex r0 ± i*r1, 2 real r3 r4
                            b = -2 * r0 - r2 - r3;
                            c = r0 * r0 + 2 * r0 * r2 + 2 * r0 * r3 + r1 * r1 + r2 * r3;
                            d = -r0 * r0 * r2 - r0 * r0 * r3 - 2 * r0 * r2 * r3 - r1 * r1 * r2 - r1 * r1 * r3;
                            e = r0 * r0 * r2 * r3 + r1 * r1 * r2 * r3;

                            // x ^ 4 +
                            // x ^ 3 (-2 r0 - r2 - r3) +
                            // x ^ 2 (r0 ^ 2 + 2 r0 r2 + 2 r0 r3 + r1 ^ 2 + r2 r3) +
                            // x (r0 ^ 2(-r2) - r0 ^ 2 r3 - 2 r0 r2 r3 - r1 ^ 2 r2 - r1 ^ 2 r3) +
                            // r0 ^ 2 r2 r3 + r1 ^ 2 r2 r3

                            break;

                    }


                    var returnedType = SolveQuartic(a, a * b, a * c, a * d, a * e, roots);

                    var found = new List<double>();
                    if (returnedType == 0)
                    {
                        // 4 real
                        found.AddRange(roots.Select(r => r.Real));
                    }
                    else if (returnedType == 1)
                    {
                        // 2 complex, 2 real
                        found.Add(roots[0].Real);
                        found.Add(Math.Sign(r1) * Math.Abs(roots[0].Imaginary)); // need sign to match from conjugate
                        found.Add(roots[2].Real);
                        found.Add(roots[3].Real);
                    }
                    else if (returnedType == 2)
                    {
                        // 2 real, 2 complex
                        found.Add(roots[0].Real);
                        found.Add(roots[1].Real);
                        found.Add(roots[2].Real);
                        found.Add(Math.Sign(r1) * Math.Abs(roots[2].Imaginary)); // need sign to match from conjugate
                        returnedType = 1;
                    }
                    else throw new NotImplementedException("quartic case not impl");



                    Trace.Assert(type == returnedType);
                    CheckError("quartic", i, new List<double> { r0, r1, r2, r3, }, found, err);
                }

            }

            void TestCubic(int passes)
            {
                range = 1e5;

                var err = 1e-5; // error to allow
                var roots = new double[3];
                for (int i = 0; i < passes; ++i)
                {
                    // todo - test complex roots, bad cases from elsewhere

                    // roots 
                    var r0 = v();
                    var r1 = v();
                    var r2 = v();
                    var a = v();
                    double b = 0, c = 0, d = 0;

                    var type1 = r.Next(2);
                    switch (type1)
                    {
                        case 0: // three real
                            type1 = 3; // 3 real distinct
                                       // todo - fails on double root!?
                                       //if (r.NextDouble() < 0.02)
                                       //{
                                       //    r2 = r1; // double root sometimes
                                       //type1 = 2; 
                                       //}
                                       //if (r.NextDouble() < 0.02)
                                       //{
                                       //    r2 = r1 = r0 = 0; // triple real root (must be 0)
                                       //    type1 = 1;
                                       //}
                            b = -(r0 + r1 + r2);
                            c = (r0 * r1 + r1 * r2 + r2 * r0);
                            d = -(r0 * r1 * r2);
                            break;

                        case 1: // one real r2, conjugate pair r0 ± i*r1
                            // x^3
                            // x^2 (-2 r0 - r2)
                            // x (r0^2 + 2 r0 r2 + r1^2)
                            // - r0^2 r2 - r1^2 r2
                            b = -2 * r0 - r2;
                            c = r0 * r0 + 2 * r0 * r2 + r1 * r1;
                            d = -r0 * r0 * r2 - r1 * r1 * r2;
                            type1 = -2;
                            break;
                    }

                    var type = SolveCubic(a, a * b, a * c, a * d, roots);
                    if (type == -2)
                    {
                        roots[2] = Math.Sign(r1) * Math.Abs(roots[2]); // ensure correct conjugate sign
                    }

                    Trace.Assert(type == type1); // real roots

                    CheckError("cubic", i, new List<double> { r0, r1, r2 }, roots.ToList(), err);

                }
            }

            static double CheckError(string label, int index, List<double> truth, List<double> found, double errBound)
            {
                truth.Sort();
                found.Sort();

                var error = Enumerable.Range(0, truth.Count).Sum(i => Math.Pow(truth[i] - found[i], 2));

                if (error > errBound)
                {
                    Console.WriteLine($"{label} error {index} error {error:F4}");
                    for (var j = 0; j < truth.Count; ++j)
                        Console.WriteLine($"  {truth[j]:F4} -> {found[j]:F4}");
                }
                return error;
            }

            void TestQuadratic(int passes)
            {
                // specific tests, a,b,c,r1,r2, type listed (on complex, r1,r2 are r1 +- i r2)
                var specificTests = new double[]
                {
                    // some b = 0 tests
                    4,0,1,0,0.5, -2,  // 4x^2==-1 => x = +- i / 2
                    1,0,-9,3,-3,  2,  // (x+3)(x-3)
                    1,0,9,0,3,   -2,  // x^2==-9

                };
                var rpow = 8;
                //for (var rpow = -5; rpow <= 50; ++rpow)
                {

                    range = Math.Pow(10.0, rpow);

                    var err = 1e-5; // error to allow
                    var roots = new double[2];
                    var maxError = 0.0;

                    void RunOne(int i, double a, double b, double c, double r0, double r1, int type)
                    {
                        var type2 = SolveQuadratic(a, b, c, roots);

                        if (type == -2)
                            roots[1] = Math.Sign(r1) * Math.Abs(roots[1]); // proper conjugate stored

                        Trace.Assert(type2 == type); // same root style

                        var err1 = CheckError("quadratic", i, new List<double> { r0, r1 }, roots.ToList(), err);

                        maxError = Math.Max(err1, maxError);
                    }

                    for (var i = 0; i < specificTests.Length; i += 6)
                    {
                        RunOne(-1 - i,
                            specificTests[i],
                            specificTests[i + 1],
                            specificTests[i + 2],
                            specificTests[i + 3],
                            specificTests[i + 4],
                            (int)specificTests[i + 5]
                            );
                    }

                    for (int i = 0; i < passes; ++i)
                    {
                        // todo - test complex roots, bad cases from elsewhere

                        // roots r1,r2;
                        var r0 = v();
                        var r1 = v();
                        var a = v();

                        double b = 0, c = 0;

                        var type = r.Next(2);
                        switch (type)
                        {
                            case 0: // 2 real
                                b = -(r0 + r1);
                                c = (r0 * r1);
                                type = 2;
                                break;
                            case 1: // complex conjug
                                b = -2 * r0;
                                c = r0 * r0 + r1 * r1;
                                type = -2;
                                break;

                                // todo  - pure complex, linear, invalid...
                        }
                        RunOne(i, a, a * b, a * c, r0, r1, type);
                    }
                    Console.WriteLine($"Quad root range {range:E} max error {maxError:E}");
                }

            }

            void TestBlinnQuadratic()
            {
                // check smoothness, want two large distinct roots, perturb through all cases, ensure smooth

                // d from > 0, = 0, < 0
                // a from + to -
                // c from + to -
                // |a| > |c| to = c to < |c|
                // b from + to -

                // todo

            }


        }
    }
}
