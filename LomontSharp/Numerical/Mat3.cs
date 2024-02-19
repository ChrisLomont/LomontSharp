using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Lomont.Numerical
{
    /// <summary>
    /// Represent a 2D transformation as a 3x3 matrix
    /// </summary>
    public class Mat3 :Matrix 
    {
        #region Constants
        const int size = 3;

        /// <summary>
        ///  The identity matrix
        /// </summary>
        public static Mat3 Identity { get; } =
            new Mat3(
                1, 0, 0, 
                0, 1, 0, 
                0, 0, 1
            );

        /// <summary>
        /// The zero matrix
        /// </summary>
        public static Mat3 Zero { get; } =
            new Mat3(
                0, 0, 0, 
                0, 0, 0, 
                0, 0, 0
            );
        #endregion

        #region Constructors

        /// <summary>
        /// Get matrix, defaults to identity
        /// </summary>
        public Mat3() : base(size,size)
        {
            for (var i = 0; i < 3; ++i)
                Values[i, i] = 1; // identity matrix
        }

        // from values, row at a time
        public Mat3(double[,] values) : base(values)
        {
            System.Diagnostics.Trace.Assert(Rows == size);
            System.Diagnostics.Trace.Assert(Columns == size);
        }


        // from values, row at a time
        public Mat3(params double[] values) : base (size,size,values)
        {
            System.Diagnostics.Trace.Assert(Rows == size);
            System.Diagnostics.Trace.Assert(Columns == size);
        }

        public Mat3(Mat3 m) : base(m)
        {
            System.Diagnostics.Trace.Assert(Rows == size);
            System.Diagnostics.Trace.Assert(Columns == size);
        }

        public Mat3(IEnumerable<double> vals) : base(size, size, vals)
        {
            System.Diagnostics.Trace.Assert(Rows == size);
            System.Diagnostics.Trace.Assert(Columns == size);
        }


        /// <summary>
        /// Constant value matrix
        /// </summary>
        /// <param name="val"></param>
        public Mat3(double value) : base(size,size,value)
        {
            System.Diagnostics.Trace.Assert(Rows == size);
            System.Diagnostics.Trace.Assert(Columns == size);
        }

        #endregion

        #region Math operators
        public static Mat3 operator +(Mat3 a) => a;
        public static Mat3 operator -(Mat3 a) => new Mat3((-(Matrix)a).Values);
        public static Mat3 operator +(Mat3 a, Mat3 b) => new Mat3(((Matrix)a + (Matrix)b).Values);
        public static Mat3 operator -(Mat3 a, Mat3 b) => new Mat3(((Matrix)a - (Matrix)b).Values);
        public static Mat3 operator *(Mat3 a, Mat3 b) => new Mat3(((Matrix)a * (Matrix)b).Values);
        public static Mat3 operator *(Mat3 m, double s) => s * m;
        public static Mat3 operator *(double s, Mat3 m) => new Mat3((s * (Matrix)m).Values);
        public static Mat3 operator /(Mat3 m, double s) => (1 / s) * m;
        #endregion

        #region Linear Algebra

        public Mat3 Transposed()
        {
            var m = new Mat3(this);
            m.Transpose();
            return m;
        }

        /// <summary>
        /// transpose in place
        /// </summary>
        public new Mat3 Transpose()
        {
            base.Transpose();
            return this;
        }


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

        /// <summary>
        /// Invert in place
        /// </summary>
        /// <returns></returns>
        public Mat3 Invert()
        {
            var m = this;
            var t = new Mat3(
                m[1, 1] * m[2, 2] - m[2, 1] * m[1, 2],
                m[0, 2] * m[2, 1] - m[0, 1] * m[2, 2],
                m[0, 1] * m[1, 2] - m[0, 2] * m[1, 1],
                m[1, 2] * m[2, 0] - m[1, 0] * m[2, 2],
                m[0, 0] * m[2, 2] - m[0, 2] * m[2, 0],
                m[1, 0] * m[0, 2] - m[0, 0] * m[1, 2],
                m[1, 0] * m[2, 1] - m[2, 0] * m[1, 1],
                m[2, 0] * m[0, 1] - m[0, 0] * m[2, 1],
                m[0, 0] * m[1, 1] - m[1, 0] * m[0, 1]
            );

            t /= Det;
            
            // set values
            Apply(
                (i, j, v) => t[i,j]
                );

            return this;
        }
        
        /// <summary>
        /// Compute matrix inverse
        /// </summary>
        /// <returns></returns>
        public Mat3 Inverse()
        {
            var m = new Mat3(this);
            return m.Invert();
        }

        public Mat3 ToIdentity()
        {
            Apply((i, j, v) => i == j ? 1 : 0);
            return this;
        }

        /// <summary>
        /// Get cofactor matrix
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Mat3 Cofactors(Mat3 m)
        {
            Mat3 ans = new();
            for (var i = 0; i < m.Rows; ++i)
                for (var j = 0; j < m.Columns; ++j)
                {
                    // remove row i, col j
                    var temp = m.Submatrix(i, j);
                    var det = m[0,0] * m[1,1] - m[1,0] * m[0,1];
                    var odd = ((i + j) & 1) == 1;
                    ans[i, j] = det * (odd ? -1 : 1);
                }
            return ans;
        }



        #endregion

        #region Geometric

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

        // manipulate 2 vector, treating mat3 as affine transform
        public static Vec2 operator *(Mat3 a, Vec2 b)
        {
            var v3 = new Vec3(b.X, b.Y, 1.0);
            var a1 = a * v3;
            return new Vec2(a1.X/a1.Z,a1.Y/a1.Z);
        }

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
            var (u, a) = q.ToAxisAngle();
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



        #endregion

        public static Vec3 operator *(Mat3 a, Vec3 b)
        {
            var v = new Vec3(0, 0, 0);
            for (var i = 0; i < 3; ++i)
                for (var j = 0; j < 3; ++j)
                    v[i] += a[i, j] * b[j];
            return v;
        }




    }
}
