using System;
using System.Text;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 2D transformation as a 3x3 matrix
    /// </summary>
    public class Mat3 // todo : IEquatable<Mat3>
    {

        /// <summary>
        /// Indexed by row, column
        /// </summary>
        public double[,] Values { get; set; }

        public static Mat3 Identity { get; } =
            new Mat3(
                1, 0, 0, 
                0, 1, 0, 
                0, 0, 1
            );

        public static Mat3 Zero { get; } =
            new Mat3(
                0, 0, 0, 
                0, 0, 0, 
                0, 0, 0
            );


        /// <summary>
        /// Get matrix, defaults to identity
        /// </summary>
        public Mat3()
        {
            Values = new double[3, 3];
            for (var i = 0; i < 3; ++i)
                Values[i, i] = 1; // identity matrix
        }

        // from values, row at a time
        public Mat3(params double[] vals)
        {
            Values = new double[3, 3];
            if (vals.Length == 0) return;
            var k = 0;
            for (var row = 0; row < 3; ++row)
                for (var col = 0; col < 3; ++col)
                    Values[row, col] = vals[k++];
        }

        /// <summary>
        /// Constant value matrix
        /// </summary>
        /// <param name="val"></param>
        public Mat3(double val)
        {
            Values = new double[3, 3];
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
                Values[i, j] = val; // constant matrix
        }

        public Mat3(Mat3 m)
        {
            Values = new double[3, 3];
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
                Values[i, j] = m[i, j];
        }

        /// <summary>
        /// get entry (i,j) where i is row, j is column, following standard math notation
        /// </summary>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <returns></returns>
        public double this[int i, int j]
        {
            get => Values[i, j];
            set => Values[i, j] = value;
        }

        public static Mat3 operator *(Mat3 a, Mat3 b)
        {
            var m = new Mat3();
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
            {
                var s = 0.0;
                for (var k = 0; k < 3; ++k)
                    s += a[i, k] * b[k, j];
                m[i, j] = s;
            }

            return m;
        }

        public static Mat3 operator *(Mat3 m, double s) => s * m;

        public static Mat3 operator *(double s, Mat3 m)
        {
            var m2 = new Mat3(m);
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
                m2[i, j] *= s;
            return m2;

        }

        public static Vec2 operator *(Mat3 a, Vec2 b)
        {
            var v = new Vec2(0, 0);
            for (var i = 0; i < 2; ++i)
            for (var j = 0; j < 3; ++j)
                v[i] += a[i, j] * b[j];
            return v;
        }
        
        public static Vec3 operator *(Mat3 a, Vec3 b)
        {
            var v = new Vec3(0, 0,0);
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    v[i] += a[i, j] * b[j];
            return v;
        }



        public static Mat3 operator +(Mat3 a, Mat3 b)
        {
            var m2 = new Mat3(a);
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
                m2[i, j] += b[i, j];
            return m2;
        }

        /// <summary>
        /// Create a scaling matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static Mat3 Scale(double x, double y) => new() { [0, 0] = x, [1, 1] = y };

        /// <summary>
        /// Create a translation matrix
        /// </summary>
        public static Mat3 Translation(Vec2 v) => Translation(v.X, v.Y);

        /// <summary>
        /// Create a translation matrix
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        public static Mat3 Translation(double dx, double dy) => new() { [0, 2] = dx, [1, 2] = dy };

        /// <summary>
        /// Create Z rotation matrix with angle in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Mat3 ZRotation(double angle)
        {
            var c = System.Math.Cos(angle);
            var s = System.Math.Sin(angle);
            var m = new Mat3 { [0, 0] = c, [0, 1] = -s, [1, 0] = s, [1, 1] = c };
            return m;
        }

        public static Mat3 operator -(Mat3 a, Mat3 b)
        {
            var m = new Mat3();
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    m[i, j] = a[i, j] - b[i, j];
            return m;
        }


#if true

        /// <summary>
        /// Create a matrix that rotates the first 3D frame into the second 3D frame.
        /// Each frame is an origin point, and then *directions* for x,y,z from that origin point
        /// </summary>
        /// <returns></returns>
        public static Mat3 CreateRotation(
            Vec2 origin1,
            Vec2 x1AxisDirection,
            Vec2 y1AxisDirection,
            Vec2 z1AxisDirection,
            Vec2 origin2,
            Vec2 x2AxisDirection,
            Vec2 y2AxisDirection,
            Vec2 z2AxisDirection)
        {
            // todo - make a 2d version
            // http://kwon3d.com/theory/transform/transform.html

            // translate to origin, then back out
            var t1 = Translation(-origin1);
            var t2 = Translation(origin2);
            var rot = new Mat3
            {
                [0, 0] = Vec2.Dot(x1AxisDirection, x2AxisDirection),
                [1, 0] = Vec2.Dot(x1AxisDirection, y2AxisDirection),
                [2, 0] = Vec2.Dot(x1AxisDirection, z2AxisDirection),
                [0, 1] = Vec2.Dot(y1AxisDirection, x2AxisDirection),
                [1, 1] = Vec2.Dot(y1AxisDirection, y2AxisDirection),
                [2, 1] = Vec2.Dot(y1AxisDirection, z2AxisDirection),
                [0, 2] = Vec2.Dot(z1AxisDirection, x2AxisDirection),
                [1, 2] = Vec2.Dot(z1AxisDirection, y2AxisDirection),
                [2, 2] = Vec2.Dot(z1AxisDirection, z2AxisDirection)
            };




            rot = rot.Transpose(); // todo - this seems wrong, but works - what is going on?

            return t2 * rot * t1;
        }

        /// <summary>
        /// Convert to rotation vector.
        /// Assumes matrix is a rotation.
        /// Rotation Vector is a vector with axis the axis of rotation
        /// and length the rotation angle in radians.
        /// </summary>
        /// <returns></returns>
        public Vec3 ToRotationVector()
        {
            var q = Quat.FromRotationMatrix(this);
            var (u,a) = q.ToAxisAngle();
            return a * u;
        }

        /// <summary>
        /// Convert a rotation vector to a rotation matrix
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Mat3 FromRotationVector(Vec3 v) =>
            v.Length > 1e-8 ? Quat.FromAxisAngle(v.Unit(), v.Length).ToRotationMatrix() : Mat3.Identity;


        /// <summary>
        /// Get the determinant of this matrix
        /// </summary>
        public double Det
        {
            get
            { // cofactor expansion
                var m = this;
                return
                    m[0, 0] * (m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2]) -
                    m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0]) +
                    m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);
            }
        }
        public Mat3 Invert()
        {
            var m = this;
            return new Mat3(
                m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2],
                m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2],
                m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1],
                m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2],
                m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0],
                m[1, 0] * m[0, 2] - m[0, 0] * m[1, 2],
                m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1],
                m[2, 0] * m[0, 1] - m[0, 0] * m[2, 1],
                m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1]
            ) * (1.0 / Det);
        }

        /// <summary>
        /// Given rotation vector, compute
        /// the rotation matrix for the exponential map
        /// </summary>
        /// <param name="omega"></param>
        /// <returns></returns>
        public static Mat3 RotationExp(Vec3 omega)
        {
            var angle = omega.Length; // Frobenius 2 norm
                                      // near phi==0, use first order Taylor expansion
            if (angle < 1e-10)
                return Identity + Vec3.CrossOperator(omega); // I + {}
            var axis = omega / angle;
            var s = Math.Sin(angle);
            var c = Math.Cos(angle);

            // rotation formula
            return c * Identity + (1 - c) * Vec3.Outer(axis, axis) + s * Vec3.CrossOperator(axis);
        }


        public double MaxNorm()
        {
            var norm = 0.0;
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    norm = System.Math.Max(norm, System.Math.Abs(Values[i, j]));
            return norm;
        }


#if false
        /// <summary>
        /// Create a matrix that rotates around the given axis by the given number of radians
        /// </summary>
        /// <returns></returns>
        public static Mat3 CreateRotation(Vec2 startAxis, Vec2 endAxis, double radians)
            {
                // from http://inside.mines.edu/fs_home/gmurray/ArbitraryAxisRotation/
                var dir = (endAxis - startAxis).Normalize();
                var u = dir.X;
                var v = dir.Y;
                var w = dir.Z;

                var u2 = u*u;
                var v2 = v*v;
                var w2 = w*w;

                var cos = System.Math.Cos(radians);
                var sin = System.Math.Sin(radians);

                var a = startAxis.X;
                var b = startAxis.Y;
                var c = startAxis.Z;

                var m = new Mat3
                {
                    [0, 0] = u2 + (v2 + w2) * cos,
                    [0, 1] = u * v * (1 - cos) - w * sin,
                    [0, 2] = u * w * (1 - cos) + v * sin,
                    [0, 3] = (a * (v2 + w2) - u * (b * v + c * w)) * (1 - cos) + (b * w - c * v) * sin,
                    [1, 0] = u * v * (1 - cos) + w * sin,
                    [1, 1] = v2 + (u2 + w2) * cos,
                    [1, 2] = v * w * (1 - cos) - u * sin,
                    [1, 3] = (b * (u2 + w2) - v * (a * u + c * w)) * (1 - cos) + (c * u - a * w) * sin,
                    [2, 0] = u * w * (1 - cos) - v * sin,
                    [2, 1] = v * w * (1 - cos) + u * sin,
                    [2, 2] = w2 + (u2 + v2) * cos,
                    [2, 3] = (c * (u2 + v2) - w * (a * u + b * v)) * (1 - cos) + (a * v - b * u) * sin,
                    [3, 0] = 0,
                    [3, 1] = 0,
                    [3, 2] = 0,
                    [3, 3] = 1
                };




                return m;
            }

            /// <summary>
            /// Generate a matrix that rotates the given vector to the given direction
            /// </summary>
            /// <returns></returns>
            public static Mat3 CreateRotation(Vec2 source, Vec2 dest)
            {

                var cross = Vec2.Cross(source, dest);
                if (cross.Length < 0.00001)
                    return new Mat3(); // identity rotation already aligned
                var unit = Vec2.Unit(cross);
                double ux = unit.X, uy = unit.Y, uz = unit.Z;
                var angle = Vec2.AngleBetween(source, dest);
                var c = System.Math.Cos(angle);
                var s = System.Math.Sin(angle);


#if true
                //Matrix S = ZeroMatrix;
                //S[0][1] = -Axis.z;
                //S[0][2] =  Axis.y;
                //S[1][0] =  Axis.z;
                //S[1][2] = -Axis.x;
                //S[2][0] = -Axis.y;
                //S[2][1] =  Axis.x;
                //R = IdentityMatrix + S*sin( Angle ) + (S*S)*(1 - cos( Angle ));

                var S = new Mat3(0)
                {
                    [0, 1] = -uz,
                    [0, 2] = uy,
                    [1, 0] = uz,
                    [1, 2] = -ux,
                    [2, 0] = -uy,
                    [2, 1] = ux
                };

                return new Mat3() + S*s + (S*S)*(1 - c);


#else
                var m = new Matrix2D();


                // top row
                m[0, 0] = c + ux*ux*(1 - c);
                m[1, 0] = ux*uy*(1-c)-uz*s;
                m[2, 0] = ux*uz*(1-c)+uy*s;

                // middle row
                m[0, 1] = uy*ux*(1-c)+uz*s;
                m[1, 1] = c+uy*uy*(1-c);
                m[2, 1] = uy*uz*(1-c)-ux*s;

                // bottom row
                m[0, 2] = uz*ux*(1-c)-uy*s;
                m[1, 2] = uz*uy*(1-c)+ux*s;
                m[2, 2] = c+uz*uz*(1-c);
                return m;
#endif
                

            }

#endif
#endif

        /// <summary>
        /// Matrix transpose
        /// </summary>
        /// <returns></returns>
        public Mat3 Transpose()
        {
            var m = new Mat3();
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
                m[i, j] = this[j, i];
            return m;
        }

        public override string ToString()
        {
            var sb = new StringBuilder("[");
            for (var i = 0; i < 3; ++i)
            for (var j = 0; j < 3; ++j)
            {
                sb.Append($"{this[i, j]} ");
            }

            sb.Append(']');
            return sb.ToString();
        }
    }
}
