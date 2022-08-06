using System;
using static System.Math;
using System.Numerics;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 4D vector
    /// </summary>
    public class Vec4 :
        Lomont.Numerical.Vector<double>,
        // basic generic support
        IAdditiveIdentity<Vec4, Vec4>,
        IAdditionOperators<Vec4, Vec4, Vec4>,
        ISubtractionOperators<Vec4, Vec4, Vec4>,
        IMultiplyOperators<Vec4, double, Vec4>,
        IDistance<Vec4, Vec4, double>
    {

        #region Constants 
        const int size = 4;

        public static Vec4 Zero { get; } = new Vec4(0, 0, 0,0);
        public static Vec4 One { get; } = new Vec4(1, 1, 1,1);
        public static Vec4 Origin { get; } = new Vec4(0, 0, 0,0);
        public static Vec4 XAxis { get; } = new Vec4(1, 0, 0,0 );
        public static Vec4 YAxis { get; } = new Vec4(0, 1, 0,0 );
        public static Vec4 ZAxis { get; } = new Vec4(0, 0, 1,0 );
        public static Vec4 WAxis { get; } = new Vec4(0, 0, 0,1 );

        /// <summary>
        /// Vector of min values in each slot
        /// </summary>
        public static Vec4 Min { get; } = new Vec4(double.MinValue, double.MinValue, double.MinValue, double.MinValue);

        /// <summary>
        /// Vector of max values in each slot
        /// </summary>
        public static Vec4 Max { get; } = new Vec4(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue);

        #endregion

        #region Properties
        public double X { get => Values[0]; set => Values[0] = value; }
        public double Y { get => Values[1]; set => Values[1] = value; }
        public double Z { get => Values[2]; set => Values[2] = value; }
        public double W { get => Values[3]; set => Values[3] = value; }
        #endregion

        #region Constructors, Deconstructor, Set

        public Vec4(double[] values) : base(size, values)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }

        public Vec4(Vec4 v) : base(v)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }



        public Vec4(double x = 0, double y = 0, double z = 0, double w = 0) : base(size)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Deconstruct(out double x, out double y, out double z, out double w)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public void Set(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Set(Vec4 vector3) => Set(vector3.X, vector3.Y, vector3.Z, vector3.W);


        #endregion

        #region Functor
        public Vec4 Map(Func<double, double> func) => new Vec4(base.Map(func).Values);

        public static Vec4 Apply(Vec4 a, Vec4 b, Func<double, double, double> func) => new Vec4(Vector<double>.Apply(a, b, func).Values);
        #endregion

        #region Math operators
        public static Vec4 operator +(Vec4 a) => a;
        public static Vec4 operator -(Vec4 a) => new Vec4((-((Vector<double>)a)).Values);
        public static Vec4 operator +(Vec4 a, Vec4 b) => new Vec4(((Vector<double>)a + (Vector<double>)b).Values);
        public static Vec4 operator -(Vec4 a, Vec4 b) => new Vec4(((Vector<double>)a - (Vector<double>)b).Values);
        public static Vec4 operator *(double a, Vec4 b) => new Vec4(b.Map(v => a * v).Values);
        public static Vec4 operator *(Vec4 b, double a) => a * b;
        public static Vec4 operator /(Vec4 a, double b) => (1.0 / b) * a;
        #endregion

        #region Linear Algebra

        /// <summary>
        /// return unit length in this direction,
        /// or 0,0,0 if already 0
        /// </summary>
        public Vec4 Normalized()
        { // todo - merge with Unit versions
            var d = Length;
            if (Length < 1e-6)
                return new Vec4(0, 0, 0, 0);
            return this * 1.0 / d;
        }

        /// <summary>
        /// make this a unit vector
        /// or 0,0,0 if already 0
        /// </summary>
        public Vec4 Normalize()
        {
            if (Length == 0) return this;
            var a = this;
            a /= Length;
            return a;
        }

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public Vec4 Unit()
        {
            return this / Length;
        }


        public double Length => System.Math.Sqrt(LengthSquared);

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public static Vec4 Unit(Vec4 a)
        {
            return a / a.Length;
        }

        #endregion

        #region Geometric
        /// <summary>
        /// Linear interpolation from a to b
        /// TODO - make generic in utility when dotnet adds INumeric
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec4 LinearInterpolation(Vec4 a, Vec4 b, double t) => a + (b - a) * t;

        public static Vec4 ComponentwiseMin(Vec4 a, Vec4 b) =>
            new Vec4(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Min).Values);

        public static Vec4 ComponentwiseMax(Vec4 a, Vec4 b) =>
            new Vec4(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Max).Values);

        public static Vec4 Componentwise(Vec4 a, Vec4 b, Func<double, double, double> func) =>
            new Vec4(Componentwise((Vector<double>)a, (Vector<double>)b, func).Values);


        #endregion

#if false // todo - make v4
        /// <summary>
        /// Outer product of vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Mat4 Outer(Vec4 a, Vec4 b)
        {
            return new Mat3(
                a.X * b.X, a.X * b.Y, a.X * b.Z,
                a.Y * b.X, a.Y * b.Y, a.Y * b.Z,
                a.Z * b.X, a.Z * b.Y, a.Z * b.Z
                );
        }
#endif

        /// <summary>
        /// Distance between points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Distance(Vec4 a, Vec4 b) => (a - b).Length;



#region Generic
        public static Vec4 AdditiveIdentity => Vec4.Zero;

        public static Vec4 operator checked +(Vec4 left, Vec4 right)
        {
            return left + right;
        }

        public static Vec4 operator checked -(Vec4 left, Vec4 right)
        {
            return left - right;
        }

        public static Vec4 operator checked *(Vec4 left, double right)
        {
            return left * right;
        }

#endregion

    }
}
