using System;
using Lomont.Algorithms;
using static System.Math;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 3D vector
    /// </summary>
    public class Vec3
    {

        public static Vec3 Zero { get; } = new Vec3(0, 0, 0);
        public static Vec3 One { get; } = new Vec3(1, 1, 1);

        public static Vec3 Origin { get; } = new Vec3(0, 0, 0);

        public static Vec3 XAxis { get; } = new Vec3(1, 0, 0);

        public static Vec3 YAxis { get; } = new Vec3(0, 1, 0);

        public static Vec3 ZAxis { get; } = new Vec3(0, 0, 1);

        public Vec3 Map(Func<double, double> func)
        {
            return new Vec3(func(X), func(Y), func(Z));
        }



        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        public Vec3(double x = 0, double y = 0, double z = 0)
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

        /// <summary>
        /// True if all components exactly zero
        /// </summary>
        bool IsNull => X == 0 && Y == 0 && Z == 0;

        public static Vec3 operator +(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
        public static Vec3 operator -(Vec3 a)
        {
            return new Vec3(-a.X, -a.Y, -a.Z);
        }

        public static Vec3 operator -(Vec3 a, Vec3 b)
        {
            return new Vec3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vec3 operator *(double a, Vec3 b)
        {
            return new Vec3(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vec3 operator *(Vec3 b, double a)
        {
            return new Vec3(a * b.X, a * b.Y, a * b.Z);
        }

        public static Vec3 operator /(Vec3 a, double b)
        {
            return (1.0 / b) * a;
        }

        public static Vec3 Cross(Vec3 a, Vec3 b)
        {
            var x = a.Y * b.Z - a.Z * b.Y;
            var y = a.Z * b.X - a.X * b.Z;
            var z = a.X * b.Y - a.Y * b.X;
            return new Vec3(x, y, z);
        }

        public static double Dot(Vec3 a, Vec3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }


        public void Set(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public void Set(Vec3 vector3)
        {
            X = vector3.X;
            Y = vector3.Y;
            Z = vector3.Z;
        }

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

        public double Length => System.Math.Sqrt(LengthSquared);
        public double LengthSquared => Dot(this, this);

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public static Vec3 Unit(Vec3 a)
        {
            return a / a.Length;
        }

        /// <summary>
        ///     Return a unit length vector in this direction
        /// </summary>
        /// <returns></returns>
        public Vec3 Unit()
        {
            return this / Length;
        }

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
        public static Vec3 LinearInterpolation(Vec3 a, Vec3 b, double t) => a + (b - a) * t;

        /// <summary>
        /// Return angle between vectors in radians
        /// </summary>
        /// <returns></returns>
        public static double AngleBetween(Vec3 a, Vec3 b) => Acos(Dot(a, b) / (a.Length * b.Length));

        /// <summary>
        /// Return max abs value of a component
        /// </summary>
        public double MaxNorm => Max(Abs(X), Max(Abs(Y),Abs(Z)));

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


        public double this[int i]
        {
            get => i switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                3 => 1, // homogeneous
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            }
        }

        public override string ToString()
        {
            return String.Format("{0},{1},{2}", X, Y, Z);
        }

    }


}
