using System;
using static System.Math;
using System.Numerics;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 4D vector
    /// </summary>
    public class Vec4 :
        // basic generic support
        IAdditiveIdentity<Vec4, Vec4>,
        IAdditionOperators<Vec4, Vec4, Vec4>,
        ISubtractionOperators<Vec4, Vec4, Vec4>,
        IMultiplyOperators<Vec4, double, Vec4>,
        IDistance<Vec4, Vec4, double>
    {

        public Vec4 Map(Func<double, double> func)
        {
            return new Vec4(func(X), func(Y), func(Z), func(W));
        }

        public Vec4(Vec4 v) : this(v.X, v.Y, v.Z, v.W)
        {
        }


        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public double W { get; set; }

        public Vec4(double x = 0, double y = 0, double z = 0, double w = 0)
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

        /// <summary>
        /// True if all components exactly zero
        /// </summary>
        bool IsNull => X == 0 && Y == 0 && Z == 0 && W == 0;

        public static Vec4 operator +(Vec4 a, Vec4 b)
        {
            return new Vec4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        }
        public static Vec4 operator -(Vec4 a)
        {
            return new Vec4(-a.X, -a.Y, -a.Z, -a.W);
        }

        public static Vec4 operator -(Vec4 a, Vec4 b)
        {
            return new Vec4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        public static Vec4 operator *(double a, Vec4 b)
        {
            return new Vec4(a * b.X, a * b.Y, a * b.Z, a*b.W);
        }

        public static Vec4 operator *(Vec4 b, double a)
        {
            return new Vec4(a * b.X, a * b.Y, a * b.Z, a*b.W);
        }

        public static Vec4 operator /(Vec4 a, double b)
        {
            return (1.0 / b) * a;
        }


        public static double Dot(Vec4 a, Vec4 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }
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


        public void Set(double x, double y, double z, double w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Set(Vec4 vector4)
        {
            X = vector4.X;
            Y = vector4.Y;
            Z = vector4.Z;
            W = vector4.W;
        }

        public double Length => System.Math.Sqrt(LengthSquared);
        public double LengthSquared => Dot(this, this);

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public static Vec4 Unit(Vec4 a)
        {
            return a / a.Length;
        }

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public Vec4 Unit()
        {
            return this / Length;
        }

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
        public void Normalize()
        {
            var d = Length;
            X /= d;
            Y /= d;
            Z /= d;
        }


        /// <summary>
        /// Linear interpolation from a to b
        /// TODO - make generic in utility when dotnet adds INumeric
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vec4 LinearInterpolation(Vec4 a, Vec4 b, double t) => a + (b - a) * t;

        /// <summary>
        /// Return max abs value of a component
        /// </summary>
        public double MaxNorm => Max(Abs(X), Max(Abs(Y), Abs(Z)));

        public static Vec4 ComponentwiseMin(Vec4 a, Vec4 b)
        {
            return Componentwise(a, b, Math.Min);

        }
        public static Vec4 ComponentwiseMax(Vec4 a, Vec4 b)
        {
            return Componentwise(a, b, Math.Max);
        }

        public static Vec4 Componentwise(Vec4 a, Vec4 b, Func<double, double, double> func)
        {
            return new Vec4(func(a.X, b.X), func(a.Y, b.Y), func(a.Z, b.Z), func(a.W, b.W));
        }


        /// <summary>
        /// Distance between points
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Distance(Vec4 a, Vec4 b) => (a - b).Length;


        public double this[int i]
        {
            get => i switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => W, 
                _ => throw new ArgumentOutOfRangeException()
            };
            set
            {
                switch (i)
                {
                    case 0:
                        X = value;
                        break;
                    case 1:
                        Y = value;
                        break;
                    case 2:
                        Z = value;
                        break;
                    case 3:
                        W = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            }
        }

#region Constants

        public static Vec4 Zero { get; } = new Vec4(0, 0, 0,0);
        public static Vec4 One { get; } = new Vec4(1, 1, 1,1);

        public static Vec4 Origin { get; } = new Vec4(0, 0, 0,0);

        public static Vec4 XAxis { get; } = new Vec4(1, 0, 0,0);

        public static Vec4 YAxis { get; } = new Vec4(0, 1, 0,0);

        public static Vec4 ZAxis { get; } = new Vec4(0, 0, 1,0);

        public static Vec4 WAxis { get; } = new Vec4(0, 0, 0, 1);

        /// <summary>
        /// Vector of min values in each slot
        /// </summary>
        public static Vec4 Min { get; } = new Vec4(double.MinValue, double.MinValue, double.MinValue, double.MinValue);

        /// <summary>
        /// Vector of max values in each slot
        /// </summary>
        public static Vec4 Max { get; } = new Vec4(double.MaxValue, double.MaxValue, double.MaxValue, double.MaxValue);


#endregion


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

        public override string ToString()
        {
            return String.Format("{0},{1},{2},{3}", X, Y, Z, W);
        }

    }


}
