using System;
using static System.Math;
using System.Numerics;
using System.Diagnostics;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 3D vector
    /// </summary>
    public class Vec3 :
        Lomont.Numerical.Vector<double>,
        // basic generic support
        IAdditiveIdentity<Vec3, Vec3>,
        IAdditionOperators<Vec3, Vec3, Vec3>,
        ISubtractionOperators<Vec3, Vec3, Vec3>,
        IMultiplyOperators<Vec3, double, Vec3>,
        IDistance<Vec3, Vec3, double>
    {

        #region Constants
        const int size = 3;

        public static Vec3 Zero { get; } = new Vec3(0, 0, 0);
        public static Vec3 One { get; } = new Vec3(1, 1, 1);
        public static Vec3 Origin { get; } = new Vec3(0, 0, 0);
        public static Vec3 XAxis { get; } = new Vec3(1, 0, 0);
        public static Vec3 YAxis { get; } = new Vec3(0, 1, 0);
        public static Vec3 ZAxis { get; } = new Vec3(0, 0, 1);

        /// <summary>
        /// Vector of min values in each slot
        /// </summary>
        public static Vec3 Min { get; } = new Vec3(double.MinValue, double.MinValue, double.MinValue);

        /// <summary>
        /// Vector of max values in each slot
        /// </summary>
        public static Vec3 Max { get; } = new Vec3(double.MaxValue, double.MaxValue, double.MaxValue);

        #endregion

        #region Properties
        public double X { get => Values[0]; set => Values[0] = value; }
        public double Y { get => Values[1]; set => Values[1] = value; }
        public double Z { get => Values[2]; set => Values[2] = value; }
        #endregion

        #region Constructors, Deconstructor, Set

        public Vec3(double [] values) : base(size,values)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }

        public Vec3(Vec3 v) : base(v)
        {
            System.Diagnostics.Trace.Assert(Dimension == size);
        }

        public Vec3(double x = 0, double y = 0, double z = 0) : base(size)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Deconstruct(out double x, out double y, out double z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public void Set(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(Vec3 vector3) => Set(vector3.X,vector3.Y,vector3.Z);

        #endregion

        #region Functor

        public Vec3 Map(Func<double, double> func) => new Vec3(base.Map(func).Values);

        public static Vec3 Apply(Vec3 a, Vec3 b, Func<double, double, double> func) => new Vec3(Vector<double>.Apply(a,b,func).Values);

        #endregion

        #region Math operators
        public static Vec3 operator +(Vec3 a) => a;
        public static Vec3 operator -(Vec3 a) => new Vec3((-((Vector<double>)a)).Values);
        public static Vec3 operator +(Vec3 a, Vec3 b) => new Vec3(((Vector<double>)a + (Vector<double>)b).Values);
        public static Vec3 operator -(Vec3 a, Vec3 b) => new Vec3(((Vector<double>)a - (Vector<double>)b).Values);
        public static Vec3 operator *(double a, Vec3 b) => new Vec3(b.Map(v=>a*v).Values);
        public static Vec3 operator *(Vec3 b, double a) => a * b;
        public static Vec3 operator /(Vec3 a, double b) => (1.0 / b) * a;
        #endregion

        #region Linear Algebra

        /// <summary>
        /// return unit length in this direction,
        /// or 0,0,0 if already 0
        /// </summary>
        public Vec3 Normalized()
        { // todo - merge with Unit versions
            var d = Length;
            if (Length < 1e-6)
                return new Vec3(0, 0, 0);
            return this * 1.0 / d;
        }

        /// <summary>
        /// make this a unit vector
        /// </summary>
        public Vec3 Normalize()
        {
            if (Length == 0) return this;
            var a = this;
            a /= Length;
            return this;
        }

        /// <summary>
        /// Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public static Vec3 Unit(Vec3 a)
        {
            return a / a.Length;
        }

        /// <summary>
        /// Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public Vec3 Unit()
        {
            return this / Length;
        }


        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            var x = a.Y * b.Z - a.Z * b.Y;
            var y = a.Z * b.X - a.X * b.Z;
            var z = a.X * b.Y - a.Y * b.X;
            return new Vec3(x, y, z);
        }

        /// <summary>
        /// Create a matrix that operates on a vector like the corss product.
        /// Often called hat operator, or cross operator
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static Mat3 CrossOperator(Vec3 v)
        {
            return new Mat3(
                0, -v.Z, v.Y,
                v.Z, 0, -v.X,
                -v.Y, v.X, 0
                );
        }

        /// <summary>
        /// Outer product of vectors
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Mat3 Outer(Vec3 a, Vec3 b)
        {
            return new Mat3(
                a.X * b.X, a.X * b.Y, a.X * b.Z,
                a.Y * b.X, a.Y * b.Y, a.Y * b.Z,
                a.Z * b.X, a.Z * b.Y, a.Z * b.Z
                );
        }

        public double Length => System.Math.Sqrt(LengthSquared);

        #endregion

        #region Geometric

        /// <summary>
        /// Get any vector normal to v
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vec3 GetANormalVector(Vec3 v)
        {
            v = v.Normalized();
            var (x, y, z) = v.Map(System.Math.Abs); // abs values
            // x^2+y^2+z^2==1 => one is >= 1/3 => abs x,y,z will have one >= sqrt(1/3) > 1/2
            // one will be >= 0.5
            Vec3 ans;
            if (x > 0.5)
                ans = new Vec3(y, -x, z);
            else if (y > 0.5)
                ans = new Vec3(-y, x, z);
            else if (z > 0.5)
                ans = new Vec3(x, -z, y);
            else
                throw new ArgumentException($"Logic error in {nameof(GetANormalVector)}");

            Debug.Assert(System.Math.Abs(Vec3.Dot(ans, v)) < 0.0001);
            return ans;

        }

        /// <summary>
        /// Distance between points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Distance(Vec3 a, Vec3 b) => (a - b).Length;

        /// <summary>
        /// Get a unit normal to the given vector, any one will do
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vec3 CreateUnitNormal(Vec3 v, Func<double> randSource)
        {
            // pick random non-parallel vector, compute cross product
            var d1 = v.Unit();
            Vec3 d2;
            do
            {
                d2 = SphericalRandom(randSource(), randSource());
            } while (Dot(d1, d2) < 0.001);

            var d3 = Cross(d1, d2);
            return d3.Unit();
        }

        /// <summary>
        /// Linear interpolation from a to b
        /// TODO - make generic in utility when dotnet adds INumeric
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec3 LinearInterpolation(Vec3 a, Vec3 b, double t) => a + (b - a) * t;

        /// <summary>
        /// Return angle between vectors in radians
        /// </summary>
        /// <returns></returns>
        public static double AngleBetween(Vec3 a, Vec3 b) => Acos(Dot(a, b) / (a.Length * b.Length));

        /// <summary>
        ///     Create a random vector uniformly by area on the unit sphere
        /// Requires two uniformly random values on [0,1]
        /// </summary>
        /// <returns></returns>
        public static Vec3 SphericalRandom(double r1, double r2)
        {
            var u = r1 * 2 - 1;// RandomManager.Random.NextDouble() * 2 - 1; // [-1,1]
            var phi = r2 * 2 * System.Math.PI; // RandomManager.Random.NextDouble() * 2 * System.Math.PI; // [0,2Pi]
            var t = System.Math.Sqrt(1 - u * u);
            var x = t * System.Math.Cos(phi);
            var y = t * System.Math.Sin(phi);
            var z = u;
            return new Vec3(x, y, z);
        }

        #endregion

        public static Vec3 ComponentwiseMin(Vec3 a, Vec3 b) =>
            new Vec3(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Min).Values);

        public static Vec3 ComponentwiseMax(Vec3 a, Vec3 b) =>
            new Vec3(Componentwise((Vector<double>)a, (Vector<double>)b, Math.Max).Values);

        public static Vec3 Componentwise(Vec3 a, Vec3 b, Func<double,double,double> func) =>
            new Vec3(Componentwise((Vector<double>)a, (Vector<double>)b, func).Values);


        #region Generic
        public static Vec3 AdditiveIdentity => Vec3.Zero;

        public static Vec3 operator checked +(Vec3 left, Vec3 right)
        {
            return left + right;
        }

        public static Vec3 operator checked -(Vec3 left, Vec3 right)
        {
            return left - right;
        }

        public static Vec3 operator checked *(Vec3 left, double right)
        {
            return left * right;
        }

        #endregion

    }


}
