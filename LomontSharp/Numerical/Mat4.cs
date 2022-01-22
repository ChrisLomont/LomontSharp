using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static System.Math;

namespace Lomont.Numerical
{
    public class Mat4
    {
        public double[,] Values { get; set; }

        public static Mat4 Identity { get; } =
            new Mat4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            );


        /// <summary>
        /// Matrix. Default to identity
        /// </summary>
        public Mat4()
        {
            Values = new double[4, 4];
            for (var i = 0; i < 4; ++i)
                Values[i, i] = 1; // identity matrix
        }

        public Mat4(params double[] vals)
        {
            if (vals.Length == 0) return;
            var k = 0;
            for (var row = 0; row < 4; ++row)
            for (var col = 0; col < 4; ++col)
                Values[row, col] = vals[k++];
        }

        public Mat4(Mat4 m)
        {
            for (var row = 0; row < 4; ++row)
            for (var col = 0; col < 4; ++col)
                Values[row, col] = m[row, col];
        }

        public Mat4(IEnumerable<double> vals)
        {
            vals.Select((v, i) => Values[i / 4, i % 4] = v).ToList();
        }

        /// <summary>
        /// From 4 columns of 3 vectors
        /// </summary>
        /// <param name="col1"></param>
        /// <param name="col2"></param>
        /// <param name="col3"></param>
        /// <param name="col4"></param>
        public Mat4(Vec3 col1, Vec3 col2, Vec3 col3, Vec3 col4)
        {
            var cols = new Vec3[] {col1,col2,col3,col4 };
            for (var r = 0; r < 4; ++r)
            for (var c = 0; c < 4; ++c)
            {
                if (r < 3)
                    this[r, c] = cols[c][r];
                else
                    this[r, c] = c == 3 ? 1 : 0;
            }
        }


        public Mat4 Inverse()
        {
            var m = new Mat4(this);
            m.Invert();
            return m;
        }

        public static Mat4 operator -(Mat4 a, Mat4 b)
        {
            var m = new Mat4();
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                m[i, j] = a[i, j] - b[i, j];
            return m;
        }

        public double Min => Get(Double.MaxValue, System.Math.Min);
        public double Max => Get(Double.MinValue, System.Math.Max);

        public static Vec3 Apply(Vec3 a, Vec3 b, Func<double, double, double> func)
        {
            return new Vec3(
                func(a.X, b.X),
                func(a.Y, b.Y),
                func(a.Z, b.Z)
            );
        }

        public double Get(double s, Func<double, double, double> f)
        {
            var t = s;
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                t = f(this[i, j], t);
            return t;
        }


        public static Mat4 operator *(Mat4 a, Mat4 b)
        {
            var m = new Mat4();
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
            {
                var s = 0.0;
                for (var k = 0; k < 4; ++k)
                    s += a[i, k] * b[k, j];
                m[i, j] = s;
            }

            return m;
        }

        public static Vec3 operator *(Mat4 a, Vec3 b)
        {
            var v = new Vec3(0, 0, 0);
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                v[i] += a[i, j] * b[j];
            return v;
        }

        /// <summary>
        /// Create a scaling matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static Mat4 Scale(double x, double y, double z)
        {
            var m = new Mat4();
            m[0, 0] = x;
            m[1, 1] = y;
            m[2, 2] = z;
            return m;
        }

        /// <summary>
        ///     Create a matrix that scales x,y,z by the given factor
        /// </summary>
        /// <param name="scale"></param>
        /// <returns></returns>
        public static Mat4 Scale(double scale)
        {
            var m = new Mat4();
            m[0, 0] = m[1, 1] = m[2, 2] = scale;
            return m;
        }

        /// <summary>
        /// Create a translation matrix
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public static Mat4 Translation(double x, double y, double z)
        {
            var m = new Mat4();
            m[0, 3] = x;
            m[1, 3] = y;
            m[2, 3] = z;
            return m;

        }

        /// <summary>
        /// Create a translation matrix
        /// </summary>
        public static Mat4 Translation(Vec3 v) => Translation(v.X, v.Y, v.Z);

        public Vec3 ConvertNormal(Vec3 v)
        {
            var zero = new Vec3(0, 0, 0);
            return this * v - this * zero;
        }

        /// <summary>
        ///     Get the determinant of this matrix
        /// </summary>
        public double Det
        {
            get
            {
                // cofactor expansion
                var m = Values; // for brevity
                return m[0, 3] * m[1, 2] * m[2, 1] * m[3, 0] - m[0, 2] * m[1, 3] * m[2, 1] * m[3, 0] -
                    m[0, 3] * m[1, 1] * m[2, 2] * m[3, 0] + m[0, 1] * m[1, 3] * m[2, 2] * m[3, 0] +
                    m[0, 2] * m[1, 1] * m[2, 3] * m[3, 0] - m[0, 1] * m[1, 2] * m[2, 3] * m[3, 0] -
                    m[0, 3] * m[1, 2] * m[2, 0] * m[3, 1] + m[0, 2] * m[1, 3] * m[2, 0] * m[3, 1] +
                    m[0, 3] * m[1, 0] * m[2, 2] * m[3, 1] - m[0, 0] * m[1, 3] * m[2, 2] * m[3, 1] -
                    m[0, 2] * m[1, 0] * m[2, 3] * m[3, 1] + m[0, 0] * m[1, 2] * m[2, 3] * m[3, 1] +
                    m[0, 3] * m[1, 1] * m[2, 0] * m[3, 2] - m[0, 1] * m[1, 3] * m[2, 0] * m[3, 2] -
                    m[0, 3] * m[1, 0] * m[2, 1] * m[3, 2] + m[0, 0] * m[1, 3] * m[2, 1] * m[3, 2] +
                    m[0, 1] * m[1, 0] * m[2, 3] * m[3, 2] - m[0, 0] * m[1, 1] * m[2, 3] * m[3, 2] -
                    m[0, 2] * m[1, 1] * m[2, 0] * m[3, 3] + m[0, 1] * m[1, 2] * m[2, 0] * m[3, 3] +
                    m[0, 2] * m[1, 0] * m[2, 1] * m[3, 3] - m[0, 0] * m[1, 2] * m[2, 1] * m[3, 3] -
                    m[0, 1] * m[1, 0] * m[2, 2] * m[3, 3] + m[0, 0] * m[1, 1] * m[2, 2] * m[3, 3];
            }
        }


        /// <summary>
        /// Invert matrix
        /// </summary>
        /// <returns></returns>
        public Mat4 Invert()
        {

            var A2323 = Values[2, 2] * Values[3, 3] - Values[2, 3] * Values[3, 2];
            var A1323 = Values[2, 1] * Values[3, 3] - Values[2, 3] * Values[3, 1];
            var A1223 = Values[2, 1] * Values[3, 2] - Values[2, 2] * Values[3, 1];
            var A0323 = Values[2, 0] * Values[3, 3] - Values[2, 3] * Values[3, 0];
            var A0223 = Values[2, 0] * Values[3, 2] - Values[2, 2] * Values[3, 0];
            var A0123 = Values[2, 0] * Values[3, 1] - Values[2, 1] * Values[3, 0];
            var A2313 = Values[1, 2] * Values[3, 3] - Values[1, 3] * Values[3, 2];
            var A1313 = Values[1, 1] * Values[3, 3] - Values[1, 3] * Values[3, 1];
            var A1213 = Values[1, 1] * Values[3, 2] - Values[1, 2] * Values[3, 1];
            var A2312 = Values[1, 2] * Values[2, 3] - Values[1, 3] * Values[2, 2];
            var A1312 = Values[1, 1] * Values[2, 3] - Values[1, 3] * Values[2, 1];
            var A1212 = Values[1, 1] * Values[2, 2] - Values[1, 2] * Values[2, 1];
            var A0313 = Values[1, 0] * Values[3, 3] - Values[1, 3] * Values[3, 0];
            var A0213 = Values[1, 0] * Values[3, 2] - Values[1, 2] * Values[3, 0];
            var A0312 = Values[1, 0] * Values[2, 3] - Values[1, 3] * Values[2, 0];
            var A0212 = Values[1, 0] * Values[2, 2] - Values[1, 2] * Values[2, 0];
            var A0113 = Values[1, 0] * Values[3, 1] - Values[1, 1] * Values[3, 0];
            var A0112 = Values[1, 0] * Values[2, 1] - Values[1, 1] * Values[2, 0];

            var det = Values[0, 0] * (Values[1, 1] * A2323 - Values[1, 2] * A1323 + Values[1, 3] * A1223)
                      - Values[0, 1] * (Values[1, 0] * A2323 - Values[1, 2] * A0323 + Values[1, 3] * A0223)
                      + Values[0, 2] * (Values[1, 0] * A1323 - Values[1, 1] * A0323 + Values[1, 3] * A0123)
                      - Values[0, 3] * (Values[1, 0] * A1223 - Values[1, 1] * A0223 + Values[1, 2] * A0123);
            det = 1 / det;

            return new Mat4
            (
                /*Values[0,0] = */det * +(Values[1, 1] * A2323 - Values[1, 2] * A1323 + Values[1, 3] * A1223),
                /*Values[0,1] = */det * -(Values[0, 1] * A2323 - Values[0, 2] * A1323 + Values[0, 3] * A1223),
                /*Values[0,2] = */det * +(Values[0, 1] * A2313 - Values[0, 2] * A1313 + Values[0, 3] * A1213),
                /*Values[0,3] = */det * -(Values[0, 1] * A2312 - Values[0, 2] * A1312 + Values[0, 3] * A1212),
                /*Values[1,0] = */det * -(Values[1, 0] * A2323 - Values[1, 2] * A0323 + Values[1, 3] * A0223),
                /*Values[1,1] = */det * +(Values[0, 0] * A2323 - Values[0, 2] * A0323 + Values[0, 3] * A0223),
                /*Values[1,2] = */det * -(Values[0, 0] * A2313 - Values[0, 2] * A0313 + Values[0, 3] * A0213),
                /*Values[1,3] = */det * +(Values[0, 0] * A2312 - Values[0, 2] * A0312 + Values[0, 3] * A0212),
                /*Values[2,0] = */det * +(Values[1, 0] * A1323 - Values[1, 1] * A0323 + Values[1, 3] * A0123),
                /*Values[2,1] = */det * -(Values[0, 0] * A1323 - Values[0, 1] * A0323 + Values[0, 3] * A0123),
                /*Values[2,2] = */det * +(Values[0, 0] * A1313 - Values[0, 1] * A0313 + Values[0, 3] * A0113),
                /*Values[2,3] = */det * -(Values[0, 0] * A1312 - Values[0, 1] * A0312 + Values[0, 3] * A0112),
                /*Values[3,0] = */det * -(Values[1, 0] * A1223 - Values[1, 1] * A0223 + Values[1, 2] * A0123),
                /*Values[3,1] = */det * +(Values[0, 0] * A1223 - Values[0, 1] * A0223 + Values[0, 2] * A0123),
                /*Values[3,2] = */det * -(Values[0, 0] * A1213 - Values[0, 1] * A0213 + Values[0, 2] * A0113),
                /*Values[3,3] = */det * +(Values[0, 0] * A1212 - Values[0, 1] * A0212 + Values[0, 2] * A0112)
            );
        }

        /// <summary>
        /// Invert matrix via Gauss Jordan
        /// </summary>
        public void InvertGaussJordan()
        {
            // Gaussian-Jordan with pivot
            var n = 4; // size of matrix

            var m = this; // alias
            var inv = new Mat4(); // identity

            double det = 1; // track determinant to check for singular matrix

            // The current pivot row
            // For each pass, first find the maximum element in the pivot column.
            for (var row = 0; row < n; row++)
            {
                // find max pivot
                var bestRow = row;
                for (var irow = row; irow < n; irow++)
                    if (Abs(m[irow, row]) > Abs(m[bestRow, row]))
                        bestRow = irow;

                // swap rows in both
                if (bestRow != row)
                {
                    for (var col = 0; col < n; col++)
                    {
                        (inv[row,col],inv[bestRow,col]) = (inv[bestRow, col],inv[row, col]);

                        if (col >= row)
                        { // lower cols all zeros
                            (m[row, col], m[bestRow, col]) = (m[bestRow, col], m[row, col]);
                        }
                    }
                }

                // Current pivot is m(row,row).
                // Det is the product of the pivot elts
                var pivot = m[row, row];
                det = det * pivot;
                if (det == 0)
                    throw new ArgumentException("Cannot inverting a singular matrix");

                for (var col = 0; col < n; col++)
                {
                    // divide by pivot to normalize 
                    inv[row, col] = inv[row, col] / pivot;
                    if (col >= row)
                        m[row, col] = m[row, col] / pivot;
                }

                for (var irow = 0; irow < n; irow++)
                {
                    // add multiple of pivot row to cancel terms
                    if (irow != row)
                    {
                        var factor = m[irow, row];
                        for (var icol = 0; icol < n; icol++)
                        {
                            inv[irow, icol] -= factor * inv[row, icol];
                            m[irow, icol] -= factor * m[row, icol];
                        }
                    }
                }
            }

            // copy back
            for (var r = 0; r < n; ++r)
            for (var c = 0; c < n; ++c)
                m[r,c] = inv[r,c];
        }

        public (Mat4 L, Mat4 U) DecomposeLU()
        {
//            todo 
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Indexing 0-3 in each component
        /// </summary>
        /// <param1 name="i"></param1>
        /// <param1 name="j"></param1>
        /// <returns></returns>
        public double this[int row, int col]
        {
            get => Values[row, col];
            set => Values[row, col] = value;
        }

        /// <summary>
        ///     Set this matrix to the identity
        /// </summary>
        public void ToIdentity()
        {
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                if (i == j)
                    Values[i, j] = 1;
                else
                    Values[i, j] = 0;
        }

        /// <summary>
        /// Roll, Pitch, Yaw has many variants. This is right handed coord system, right handed angles, Tait-Bryan intrinsic active z-y'-x'' roll, pitch, yaw
        /// Convention from https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
        /// </summary>
        /// <param name="roll"></param>
        /// <param name="pitch"></param>
        /// <param name="yaw"></param>
        /// <returns></returns>
        public static Mat4 FromRollPitchYaw(double roll, double  pitch, double yaw) => RotationXYZ(roll, pitch, yaw);

    /// <summary>
    /// Create rotation matrix: X, then Y, then Z
    /// Right handed coord, right handed angles, Tait-Bryan intrinsic active z-y'-x'' roll, pitch ,yaw
    /// Multiply object on right of matrix
    /// </summary>
    /// <param name="xRadians"></param>
    /// <param name="yRadians"></param>
    /// <param name="zRadians"></param>
    /// <returns></returns>
    public static Mat4 RotationXYZ(double xRadians, double yRadians, double zRadians)
    {
        // expand out in mathematica:
        var cx = Cos(xRadians);
        var sx = Sin(xRadians);
        var cy = Cos(yRadians);
        var sy = Sin(yRadians);
        var cz = Cos(zRadians);
        var sz = Sin(zRadians);
        return new Mat4( 
            cy * cz, sx * sy * cz - cx * sz, cx * sy * cz + sx * sz, 0,
            cy * sz, sx * sy * sz + cx * cz, cx * sy * sz - sx * cz, 0,
            -sy, sx * cy, cx * cy, 0,
              0, 0, 0, 1
        );
    }

        /// <summary>
        /// Create X rotation matrix with angle in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Mat4 XRotation(double angle)
        {
            var m = new Mat4();
            var c = System.Math.Cos(angle);
            var s = System.Math.Sin(angle);
            m[1, 1] = c;
            m[1, 2] = s;
            m[2, 1] = -s;
            m[2, 2] = c;
            m[3, 3] = 1;
            return m;
        }

        // create matrix to align world as specified
        // matrix takes world frame x,y,z to center eyePoint, frame x',y',z'
        // where z' is the look direction, x' is up, and y' is to right
        public static Mat4 LookAt(Vec3 eyePoint, Vec3 atPoint, Vec3 upDirection)
        {
            var zAxis = (eyePoint - atPoint).Normalized();
            var xAxis = Vec3.Cross(upDirection, zAxis).Normalized();
            var yAxis = Vec3.Cross(zAxis, xAxis).Normalized();
            return new Mat4(xAxis, yAxis, zAxis, eyePoint);
        }

        /// <summary>
        /// Create Y rotation matrix with angle in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Mat4 YRotation(double angle)
        {
            var m = new Mat4();
            var c = System.Math.Cos(angle);
            var s = System.Math.Sin(angle);
            m[0, 0] = c;
            m[2, 2] = c;
            m[0, 2] = s;
            m[2, 0] = -s;
            m[3, 3] = 1;
            return m;
        }

        /// <summary>
        /// Create Z rotation matrix with angle in radians
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static Mat4 ZRotation(double angle)
        {
            var m = new Mat4();
            var c = System.Math.Cos(angle);
            var s = System.Math.Sin(angle);
            m[0, 0] = c;
            m[1, 1] = c;
            m[0, 1] = s;
            m[1, 0] = -s;
            m[3, 3] = 1;
            return m;
        }

        /// <summary>
        /// Orthographic projection.
        /// Identity if zero (or negative) volume
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="bottom"></param>
        /// <param name="top"></param>
        /// <param name="nearPlane"></param>
        /// <param name="farPlane"></param>
        /// <returns></returns>
        public static Mat4 OrthographicProjection(double left, double right, double bottom, double top, double nearPlane, double farPlane)
        {
            var ortho = new Mat4();
            var tol = 1e-6;
            if (right < left + tol || bottom < top + tol || farPlane < nearPlane + tol)
                return ortho;

            var width = right - left;
            var height = bottom - top;
            var clip = farPlane - nearPlane;

            ortho[0, 0] = 2 / width;
            ortho[1, 1] = -2 / height;
            ortho[2, 2] = -2 / clip;

            ortho[0, 3] = -(left + right) / width;
            ortho[1, 3] = (top + bottom) / height;
            ortho[2, 3] = -(nearPlane + farPlane) / clip;

            return ortho;
        }




        /// <summary>
        ///     Create a matrix that represents a rotation by the given angle in
        ///     radians about the axis defined by the two points.
        /// </summary>
        /// <param name="angle">Angle in radians</param>
        /// <param name="start">Start of rotation axis</param>
        /// <param name="end">End of rotation axis</param>
        /// <returns></returns>
        public static Mat4 AxisRotation(double angle, Vec3 start, Vec3 end)
        {
            // rotation matrix from http://en.wikipedia.org/wiki/Rotation_matrix#Rotation_matrix_given_an_axis_and_an_angle
            var c = System.Math.Cos(angle);
            var s = System.Math.Sin(angle);
            var axis = (end - start).Unit();
            double ux = axis.X, uy = axis.Y, uz = axis.Z;
            var m = new Mat4
            {
                [0, 0] = ux * ux + (1 - ux * ux) * c,
                [0, 1] = ux * uy * (1 - c) - uz * s,
                [0, 2] = ux * uz * (1 - c) + uy * s,
                [1, 0] = ux * uy * (1 - c) + uz * s,
                [1, 1] = uy * uy + (1 - uy * uy) * c,
                [1, 2] = uy * uz * (1 - c) - ux * s,
                [2, 0] = ux * uz * (1 - c) - uy * s,
                [2, 1] = uy * uz * (1 - c) + ux * s,
                [2, 2] = uz * uz + (1 - uz * uz) * c
            };

            var p = new Vec3(0, 0, 0);
            m = Translation(start - p) * m * Translation(p - start);
            Debug.Assert(System.Math.Abs(m.Det - 1.0) < 0.0001);
            return m;
        }

        #region IFormattable Members

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < 4; ++i)
                sb.Append($"{Values[i, 0]} {Values[i, 1]} {Values[i, 2]} {Values[i, 3]}\n");
            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Create a matrix that rotates the first 3D frame into the second 3D frame.
        /// Each frame is an origin point, and then *directions* for x,y,z from that origin point
        /// </summary>
        /// <returns></returns>
        public static Mat4 CreateRotation(
            Vec3 origin1,
            Vec3 x1AxisDirection,
            Vec3 y1AxisDirection,
            Vec3 z1AxisDirection,
            Vec3 origin2,
            Vec3 x2AxisDirection,
            Vec3 y2AxisDirection,
            Vec3 z2AxisDirection)
        {
            // http://kwon3d.com/theory/transform/transform.html

            // translate to origin, then back out
            var t1 = Translation(-origin1);
            var t2 = Translation(origin2);
            var rot = new Mat4
            {
                [0, 0] = Vec3.Dot(x1AxisDirection, x2AxisDirection),
                [1, 0] = Vec3.Dot(x1AxisDirection, y2AxisDirection),
                [2, 0] = Vec3.Dot(x1AxisDirection, z2AxisDirection),
                [0, 1] = Vec3.Dot(y1AxisDirection, x2AxisDirection),
                [1, 1] = Vec3.Dot(y1AxisDirection, y2AxisDirection),
                [2, 1] = Vec3.Dot(y1AxisDirection, z2AxisDirection),
                [0, 2] = Vec3.Dot(z1AxisDirection, x2AxisDirection),
                [1, 2] = Vec3.Dot(z1AxisDirection, y2AxisDirection),
                [2, 2] = Vec3.Dot(z1AxisDirection, z2AxisDirection)
            };




            rot = rot.Transposed(); // todo - this seems wrong, but works - what is going on?

            return t2 * rot * t1;
        }

        /// <summary>
        /// Return transposed matrix
        /// </summary>
        /// <returns></returns>
        public Mat4 Transposed()
        {
            var m = new Mat4();
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                m[i, j] = this[j, i];
            return m;
        }

        /// <summary>
        /// Transpose in place
        /// </summary>
        public void Transpose()
        {
            for (var i = 0; i < 4; i++)
            for (var j = i + 1; j < 4; j++)
            { // swap
                (this[i, j], this[j,i]) = (this[j,i], this[i, j]);
            }
        }



        /// <summary>
        /// Create a matrix that rotates around the given axis by the given number of radians
        /// </summary>
        /// <returns></returns>
        public static Mat4 CreateRotation(Vec3 startAxis, Vec3 endAxis, double radians)
        {
            // from http://inside.mines.edu/fs_home/gmurray/ArbitraryAxisRotation/
            var dir = (endAxis - startAxis).Normalized();
            var u = dir.X;
            var v = dir.Y;
            var w = dir.Z;

            var u2 = u * u;
            var v2 = v * v;
            var w2 = w * w;

            var cos = System.Math.Cos(radians);
            var sin = System.Math.Sin(radians);

            var a = startAxis.X;
            var b = startAxis.Y;
            var c = startAxis.Z;

            var m = new Mat4
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
        /// Generate a matrix that rotates the given vector to the given direction
        /// </summary>
        /// <returns></returns>
        public static Mat4 CreateRotation(Vec3 source, Vec3 dest)
        {
#if false
                var cross = Vec3.Cross(source, dest);
                if (cross.Length < 0.00001)
                {
                    // axes aligned, could point in different directions....
                    todo
                    return new Mat4(); // identity rotation already aligned
                }

                var unit = Vec3.Unit(cross);
                double ux = unit.X, uy = unit.Y, uz = unit.Z;
                var angle = Vec3.AngleBetween(source, dest);
                var c = Math.Cos(angle);
                var s = Math.Sin(angle);


#if true
                //Matrix S = ZeroMatrix;
                //S[0][1] = -Axis.z;
                //S[0][2] =  Axis.y;
                //S[1][0] =  Axis.z;
                //S[1][2] = -Axis.x;
                //S[2][0] = -Axis.y;
                //S[2][1] =  Axis.x;
                //R = IdentityMatrix + S*sin( Angle ) + (S*S)*(1 - cos( Angle ));

                var S = new Mat4(0)
                {
                    [0, 1] = -uz,
                    [0, 2] = uy,
                    [1, 0] = uz,
                    [1, 2] = -ux,
                    [2, 0] = -uy,
                    [2, 1] = ux
                };

                return new Mat4() + S*s + (S*S)*(1 - c);


#else
                var m = new Mat4();


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
#endif
            // based on derivation that removes all trig and roots
            // https://iquilezles.org/www/articles/noacos/noacos.htm
            // expanded explanation https://gist.github.com/kevinmoran/b45980723e53edeb8a5a43c49f134724
            // these still had singularities at cosA = -1
            // also fails on axis = 0,0,0


            var v1 = source.Normalized();
            var v2 = dest.Normalized();
            var axis = Vec3.Cross(v1, v2);
            var cosA = Vec3.Dot(v1, v2);

            if (axis.LengthSquared < 0.001)
            {
                // special case - vectors parallel

                if (cosA > 0)
                    return new Mat4(); // same dir, identity

                // opposite dir
                // get any normal, and rotate 180 around it
                // todo - we cheat, combine two rotations, make faster/simpler someday
                var perp = GetANormalVector(source);
                var m1 = CreateRotation(source, perp);
                var m2 = CreateRotation(perp, dest);
                return m2 * m1;
            }

            var k = 1.0f / (1.0f + cosA);

            // todo - need better constructor
            Mat4 result = new Mat4
            (
                (axis.X * axis.X * k) + cosA,
                (axis.Y * axis.X * k) - axis.Z,
                (axis.Z * axis.X * k) + axis.Y,
                0,

                (axis.X * axis.Y * k) + axis.Z,
                (axis.Y * axis.Y * k) + cosA,
                (axis.Z * axis.Y * k) - axis.X,
                0,

                (axis.X * axis.Z * k) - axis.Y,
                (axis.Y * axis.Z * k) + axis.X,
                (axis.Z * axis.Z * k) + cosA,
                0,

                0, 0, 0, 1
            );
            return result;
        }

        public bool Equals(Mat4 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var diff = other - this;
            var n = diff.MaxNorm();

            return n < 0.0001; // todo- smarter zero compare?
        }

        public double MaxNorm()
        {
            var norm = 0.0;
            for (var i = 0; i < 4; ++i)
            for (var j = 0; j < 4; ++j)
                norm = System.Math.Max(norm, System.Math.Abs(Values[i, j]));
            return norm;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Mat4)obj); // calls above
        }

        public override int GetHashCode()
        {
            // todo?
            return (Values != null ? Values.GetHashCode() : 0);
        }

        public static bool operator ==(Mat4 a, Mat4 b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Mat4 a, Mat4 b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Given three non-collinear points, create a mapping that takes them into
        /// the xy plane of the standard frame.
        /// Points are treated as x axis, then origin, then y axis is created in plane with p2 in top half plane
        /// Returns forward map (item to xy plane) and inverse map (plane x to item)
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static (Mat4, Mat4) MapFrame(Vec3 p0, Vec3 p1, Vec3 p2)
        {
            var x = (p0 - p1).Normalized();
            var t = (p2 - p1).Normalized(); // place in upper half plane
            var z = Vec3.Cross(x, t).Normalized();
            var y = Vec3.Cross(z, x).Normalized();

            // xy plane to item....
            var q = new Mat4();
            (q[0, 0], q[1, 0], q[2, 0]) = x;
            (q[0, 1], q[1, 1], q[2, 1]) = y;
            (q[0, 2], q[1, 2], q[2, 2]) = z; // rotation only

            // column matrix x y z p1 is transform, we want inverse
            // cheat by inverting rotation via transpose, and inverting translation....
            var p = q.Transposed(); // transpose inverts rotation

            (p[0, 0], p[0, 1], p[0, 2]) = x;
            (p[1, 0], p[1, 1], p[1, 2]) = y;
            (p[2, 0], p[2, 1], p[2, 2]) = z;

            q = Translation(p1) * q; // translate to item
            p = p * Translation(-p1); // translate back

            // better be inverses
            Debug.Assert((p * q - Identity).MaxNorm() < 0.001);
            return (p, q);
        }

    }

}
