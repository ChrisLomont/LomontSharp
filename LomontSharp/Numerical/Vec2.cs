using System;
using System.Numerics;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 2D vector
    /// </summary>
    public class Vec2 :
        Lomont.Numerical.Vector<double>,
        // basic generic support
        IAdditiveIdentity<Vec2, Vec2>,
        IAdditionOperators<Vec2, Vec2, Vec2>,
        ISubtractionOperators<Vec2, Vec2, Vec2>,
        IMultiplyOperators<Vec2, double, Vec2>,
        IDistance<Vec2, Vec2, double>

    {

        #region Constants
        const int size = 2;

        public static Vec2 Zero { get; } = new Vec2(0, 0);
        public static Vec2 One { get; } = new Vec2(1, 1);
        public static Vec2 Origin { get; } = new Vec2(0, 0);
        public static Vec2 XAxis { get; } = new Vec2(1, 0);
        public static Vec2 YAxis { get; } = new Vec2(0, 1);

        /// <summary>
        /// Vector of min values in each slot
        /// </summary>
        public static Vec2 Min { get; } = new Vec2(double.MinValue, double.MinValue);

        /// <summary>
        /// Vector of max values in each slot
        /// </summary>
        public static Vec2 Max { get; } = new Vec2(double.MaxValue, double.MaxValue);

        #endregion

        #region Properties
        public double X { get => Values[0]; set => Values[0] = value; }
        public double Y { get => Values[1]; set => Values[1] = value; }
        #endregion


        #region Constructors, Deconstructor, Set

        public Vec2(double[] vals) : base(size,vals)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }

        public Vec2(double x = 0, double y = 0) : base(size)
        {
            X = x;
            Y = y;
        }

        public Vec2(Vec2 v) : base(v)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }

        public void Deconstruct(out double x, out double y)
        {
            x = this.X;
            y = this.Y;
        }

        public void Set(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void Set(Vec2 vector3) => Set(vector3.X, vector3.Y);


        #endregion

        #region Functor 
        public Vec2 Map(Func<double, double> func) => new Vec2(base.Map(func).Values);

        public static Vec2 Apply(Vec2 a, Vec2 b, Func<double, double, double> func) => new Vec2(Vector<double>.Apply(a, b, func).Values);

        #endregion

        #region Math operators
        public static Vec2 operator +(Vec2 a) => a;
        public static Vec2 operator -(Vec2 a) => new Vec2((-((Vector<double>)a)).Values);
        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(((Vector<double>)a + (Vector<double>)b).Values);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(((Vector<double>)a - (Vector<double>)b).Values);
        public static Vec2 operator *(double a, Vec2 b) => new Vec2(b.Map(v => a * v).Values);
        public static Vec2 operator *(Vec2 b, double a) => a * b;
        public static Vec2 operator /(Vec2 a, double b) => (1.0 / b) * a;
        #endregion

        #region Linear Algebra
        /// <summary>
        /// return unit length in this direction,
        /// or 0,0,0 if already 0
        /// </summary>
        public Vec2 Normalized()
        { // todo - merge with Unit versions
            var d = Length;
            if (Length < 1e-6)
                return new Vec2(0, 0);
            return this * 1.0 / d;
        }

        /// <summary>
        /// make this a unit vector
        /// </summary>
        public Vec2 Normalize()
        {
            if (Length == 0) return this;
            var a = this;
            a /= Length;
            return this;
        }

        public static double Cross2D(Vec2 a, Vec2 b)
        {
            return a.X * b.Y - a.Y * b.X;
        }


        public double Length => System.Math.Sqrt(LengthSquared);

        /// <summary>
        /// Distance between points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Distance(Vec2 a, Vec2 b) => (a - b).Length;

        #endregion

        #region Geometric
        /// <summary>
        /// Return angle between vectors in radians
        /// </summary>
        /// <returns></returns>
        public static double AngleBetween(Vec2 a, Vec2 b)
        {
            return System.Math.Acos(Dot(a, b) / (a.Length * b.Length));
        }


        #endregion

        public static Vec2 ComponentwiseMin(Vec2 a, Vec2 b) =>
            new Vec2(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Min).Values);

        public static Vec2 ComponentwiseMax(Vec2 a, Vec2 b) =>
            new Vec2(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Max).Values);

        public static Vec2 Componentwise(Vec2 a, Vec2 b, Func<double, double, double> func) =>
            new Vec2(Componentwise((Vector<double>)a, (Vector<double>)b, func).Values);


        #region Generic
        public static Vec2 AdditiveIdentity => Vec2.Zero;
        public static Vec2 operator checked +(Vec2 left, Vec2 right)
        {
            return left + right;
        }

        public static Vec2 operator checked -(Vec2 left, Vec2 right)
        {
            return left - right;
        }

        public static Vec2 operator checked *(Vec2 left, double right)
        {
            return left * right;
        }

        #endregion


    }
}
