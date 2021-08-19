using System;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 2D vector
    /// </summary>
    public class Vec2
    {
        public double X { get; set; }
        public double Y { get; set; }

        public Vec2(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public Vec2(Vec2 v) : this(v.X, v.Y)
        {
            X = v.X;
            Y = v.Y;
        }

        public void Deconstruct(out double x, out double y)
        {
            x = this.X;
            y = this.Y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X + b.X, a.Y + b.Y);
        }

        public static Vec2 operator -(Vec2 a)
        {
            return new Vec2(-a.X, -a.Y);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return new Vec2(a.X - b.X, a.Y - b.Y);
        }

        public static Vec2 operator *(double a, Vec2 b)
        {
            return new Vec2(a * b.X, a * b.Y);
        }

        public static Vec2 operator *(Vec2 b, double a)
        {
            return new Vec2(a * b.X, a * b.Y);
        }

        public static Vec2 operator /(Vec2 a, double b)
        {
            var r = 1 / b;
            return new Vec2(a.X * r, a.Y * r);
        }

        public static double Dot(Vec2 a, Vec2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public double Length => System.Math.Sqrt(LengthSquared);
        public double LengthSquared => Dot(this, this);


        /// <summary>
        /// return unit length in this direction,
        /// or 0,0,0 if already 0
        /// </summary>
        public Vec2 Normalize()
        {
            var d = Length;
            if (Length < 0.000001)
                return new Vec2(0, 0);
            return this * 1.0 / d;
        }


        public static Vec2 Unit(Vec2 a)
        {
            return a / a.Length;
        }

        /// <summary>
        /// Return angle between vectors in radians
        /// </summary>
        /// <returns></returns>
        public static double AngleBetween(Vec2 a, Vec2 b)
        {
            return System.Math.Acos(Dot(a, b) / (a.Length * b.Length));
        }

        public double this[int i]
        {
            get
            {
                return i switch
                {
                    0 => X,
                    1 => Y,
                    2 => 1.0, // treat as homogeneous
                    _ => 0.0
                };
            }
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
                }
            }
        }



        public override string ToString()
        {
            return $"{X},{Y}";
        }


        public static Vec2 ComponentwiseMin(Vec2 a, Vec2 b)
        {
            return Componentwise(a, b, System.Math.Min);

        }

        public static Vec2 ComponentwiseMax(Vec2 a, Vec2 b)
        {
            return Componentwise(a, b, System.Math.Max);
        }

        public static Vec2 Componentwise(Vec2 a, Vec2 b, Func<double, double, double> func)
        {
            return new Vec2(func(a.X, b.X), func(a.Y, b.Y));
        }

        #region Constants

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


    }
}
