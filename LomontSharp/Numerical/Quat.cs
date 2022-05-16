using System;
using Microsoft.VisualBasic.CompilerServices;
using static Lomont.Numerical.Utility;
using static System.Math;

namespace Lomont.Numerical
{
    /// <summary>
    /// Quaternion class. 
    /// </summary>
    public class Quat
    {
        #region Constructors and casting
        /// <summary>
        /// construct from values, default to identity (a,b,c,d) = (w,x,y,z) = (1,0,0,0)
        /// Also w,x,y,z form
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        public Quat(double a = 1.0, double b = 0.0, double c = 0.0, double d = 0.0)
        {
            v = new[] { a, b, c, d };
        }

        public Quat(Quat q) : this(q.A, q.B, q.C, q.D)
        {
        }

        /// <summary>
        /// Cast to rotation axis
        /// </summary>
        /// <param name="q"></param>
        public static explicit operator Vec3(Quat q)
        {
            var (axis, _) = q.ToAxisAngle();
            return axis;
        }

        /// <summary>
        /// Cast to rotation matrix
        /// </summary>
        /// <param name="q"></param>
        public static explicit operator Mat4(Quat q) => new Mat4(q.ToRotationMatrix());


        /// <summary>
        /// Make rotation on the given axis, CCW when axis points to viewer
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="angleRadians"></param>
        /// <returns></returns>
        public static Quat FromAxisAngle(double x, double y, double z, double angleRadians)
        {
            var length = Sqrt(x * x + y * y + z * z);

            if (AreClose(length, 0.0))
                return new();

            var x1 = x / length;
            var y1 = y / length;
            var z1 = z / length;

            var cosA = Cos(angleRadians / 2.0);
            var sinA = Sin(angleRadians / 2.0);

            return new(cosA, sinA * x1, sinA * y1, sinA * z1);
        }

        public static Quat FromAxisAngle(Vec3 v, double angle) => FromAxisAngle(v.X, v.Y, v.Z, angle);

        // convert quaternion to an axis and angle
        // rotation is counter-clockwise when rotation vector is pointing to viewer
        public void ToAxisAngle(out double x, out double y, out double z, out double angle)
        {
            /* If we have the identity quaternion, return zero rotation about Z */
            var length = Sqrt(B * B + C * C + D * D);
            if (AreClose(length, 0.0))
            {
                x = y = angle = 0;
                z = 1;
                return;
            }

            x = B / length;
            y = C / length;
            z = D / length;
            angle = 2 * Acos(A);
        }

        // convert quaternion to an axis and angle
        // rotation is counter-clockwise when rotation vector is pointing to viewer
        public (Vec3 axis, double angleRadians) ToAxisAngle()
        {
            ToAxisAngle(out var x, out var y, out var z, out var angleRadians);
            return (new(x, y, z), angleRadians);
        }

        #endregion

        #region Backing, accessors, indexers
        // W,X,Y,Z order, also a,b,c,d
        double[] v;

        /// <summary>
        /// Formats: W,X,Y,Z = a,b,c,d = 1,i,j,k basis
        /// </summary>
        public double A
        {
            get => v[0];
            set => v[0] = value;
        }

        public double B
        {
            get => v[1];
            set => v[1] = value;
        }

        public double C
        {
            get => v[2];
            set => v[2] = value;
        }

        public double D
        {
            get => v[3];
            set => v[3] = value;
        }

        public double W
        {
            get => v[0];
            set => v[0] = value;
        }

        public double X
        {
            get => v[1];
            set => v[1] = value;
        }

        public double Y
        {
            get => v[2];
            set => v[2] = value;
        }

        public double Z
        {
            get => v[3];
            set => v[3] = value;
        }

        public double this[int index]
        {
            get => index switch
            {
                0 => A,
                1 => B,
                2 => C,
                3 => D,
                _ => throw new IndexOutOfRangeException()
            };
            set
            {
                switch (index)
                {
                    case 0:
                        A = value;
                        break;
                    case 1:
                        B = value;
                        break;
                    case 2:
                        C = value;
                        break;
                    case 3:
                        D = value;
                        break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public void Deconstruct(out double w, out double x, out double y, out double z)
        {
            w = A;
            x = B;
            y = C;
            z = D;
        }

        #endregion

        #region Simple arithmetic
        public static Quat operator *(Quat a, double s) => new(a.A * s, a.B * s, a.C * s, a.D * s);
        public static Quat operator *(double s, Quat a) => a * s;
        public static Quat operator /(Quat a, double s) => new(a.A / s, a.B / s, a.C / s, a.D / s);


        public static Quat operator +(Quat q) => q;
        public static Quat operator -(Quat q) => new(-q.A, -q.B, -q.C, -q.D);

        public static Quat operator +(Quat a, Quat b) => new(a.A + b.A, a.B + b.B, a.C + b.C, a.D + b.D);
        public static Quat operator -(Quat a, Quat b) => new(a.A - b.A, a.B - b.B, a.C - b.C, a.D - b.D);

        public static Quat operator *(Quat a, Quat b) =>
            new(
                a.A * b.A - a.B * b.B - a.C * b.C - a.D * b.D,
                a.A * b.B + a.B * b.A + a.C * b.D - a.D * b.C,
                a.A * b.C + a.C * b.A + a.D * b.B - a.B * b.D,
                a.A * b.D + a.D * b.A + a.B * b.C - a.C * b.B);

        public static Quat operator /(Quat a, Quat b) => a * b.Inverse();
        #endregion

        #region Equality
        public static bool operator ==(Quat q1, Quat q2)
        {
            // todo - make equality if they are same rotation? Make function for that? very tricky
            return AreClose(q1.A, q2.A) && AreClose(q1.B, q2.B) && AreClose(q1.C, q2.C) && AreClose(q1.D, q2.D);
        }

        public static bool operator !=(Quat q1, Quat q2) => !(q1 == q2);

        /// <summary>
        /// See if two quats represent the same rotation
        /// </summary>
        /// <returns></returns>
        public static bool SameRotation(Quat q1, Quat q2)
        {
            // Notes:
            // 1. Cannot do easily from Euler angles, since edge cases become a mess.
            // 2. Thus convert rotations to quats, and see if q1 ~ q2 or q1 ~ -q2
            var d1 = (q1 - q2).Length; // is q1 ~ +q2?
            var d2 = (q1 + q2).Length; // is q1 ~ -q2?
            var tolerance = 0.000001; 
            return d1 < tolerance || d2 < tolerance;


        }

        #endregion

        #region Norms and lengths 

        public double LengthSquared => A * A + B * B + C * C + D * D;

        public double Length => Sqrt(LengthSquared);

        /// <summary>
        /// normalize this quaternion
        /// </summary>
        /// <returns></returns>
        public void Normalize()
        {
            A /= Length;
            B /= Length;
            C /= Length;
            D /= Length;
        }

        /// <summary>
        /// normalized copy of quaternion
        /// </summary>
        /// <returns></returns>
        public Quat Normalized() => new Quat(this)/Length;

        #endregion

        #region Quat algebra

        /// <summary>
        /// Quat inverse (two sided)
        /// </summary>
        /// <returns></returns>
        public Quat Inverse() => Conjugated() / LengthSquared;

        /// <summary>
        /// invert this quaternion
        /// </summary>
        /// <returns></returns>
        public void Inverted() => (A, B, C, D) = Inverse();

        /// <summary>
        /// conjugated copy of quaternion
        /// </summary>
        /// <returns></returns>
        public Quat Conjugated() => new(A, -B, -C, -D);

        /// <summary>
        /// conjugate this quaternion
        /// </summary>
        /// <returns></returns>
        public void Conjugate()
        {
            B = -B;
            C = -C;
            D = -D;
        }
        
        /// <summary>
        /// Dot product of components
        /// </summary>
        /// <param name="q1"></param>
        /// <param name="q2"></param>
        /// <returns></returns>
        public static double Dot(Quat q1, Quat q2) => q1.A * q2.A + q1.B * q2.B + q1.C * q2.C + q1.D * q1.D;


        #endregion

        #region Transcendental

        /// <summary>
        /// Exponential
        /// </summary>
        /// <returns></returns>
        public Quat Exp()
        {
            // q      = a + bi+cj+dk = a + v
            // exp q  = e^a(cos|v| + v/|v|*sin|v|)
            // ln q   = ln|q| + v/|v| * acos(a/|q|)
            var vNorm = Sqrt(B * B + C * C + D * D);
            var scale = Sin(vNorm) / vNorm;
            return Math.Exp(A) * new Quat(Cos(vNorm), scale * B, scale * C, scale * D);
        }

        /// <summary>
        /// Natural log
        /// </summary>
        /// <returns></returns>
        public Quat Log()
        {
            // q = a + bi+cj+dk = a + v
            // exp(q) = e^a(cos|v| + v/|v|*sin|v||)
            // ln q = ln|q| + v/|v| * arccos(a/|q|)
            var scale = Acos(A / Length) / Sqrt(B * B + C * C + D * D);
            return new(Math.Log(Length), scale * B, scale * C, scale * D);
        }
        #endregion

        #region Rotations

        /// <summary>
        /// Rotate vector by this quat
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Vec3 Rotate(Vec3 v)
        {
            Quat qv = new(0, v[0], v[1], v[2]);
            Quat qm = this * qv * Inverse();
            return new(qm.B, qm.C, qm.D);
        }

        /// <summary>
        /// Spherically linear interpolate unit quaternions from start to end as t goes 0 to 1
        /// Often called Slerp
        /// </summary>
        /// <param name="startQuat"></param>
        /// <param name="endQuat"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Quat SphericallyLinearInterpolate(Quat startQuat, Quat endQuat, double t)
        {
            Quat startQ = startQuat.Normalized(); // local copy for modifications
            Quat endQ = endQuat.Normalized();

            var cosOmega = Dot(startQ, endQ);

            // if dot product negative, take the shorter path.
            if (cosOmega < 0.0)
            {
                cosOmega = -cosOmega;
                startQ = -startQ;
            }

            var dotThreshold = 0.99995;
            if (cosOmega > dotThreshold)
            {
                // If the inputs are too close for comfort, linearly interpolate
                // and normalize the result.

                var result = startQ + t * (endQ - startQ);
                result.Normalize();
                return result;
            }

            // acos safe if dot in [0, dotThreshold]
            var theta0 = Acos(cosOmega); // theta0 = angle between input vectors
            var theta = theta0 * t;      // theta = angle between v0 and result
            var sinTheta = Sin(theta);   
            var sinTheta0 = Sin(theta0); 

            var s0 = Cos(theta) - cosOmega * sinTheta / sinTheta0; 
            var s1 = sinTheta / sinTheta0;

            return s0 * startQ + s1 * endQ;
        }

        /// <summary>
        /// Quat that rotates v1 into v2 around perp axis
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static Quat RotateBetween(Vec3 v1, Vec3 v2)
        {
            // normalize both vectors to make math cleaner
            var u1 = v1.Normalized();
            var u2 = v2.Normalized();
            // cross product to get unit rotation axis perpendicular to u1,u2
            var axis = Vec3.Cross(u1, u2);

            // to get angle between vectors, note
            // | u1 X u2 | = |u1||u2|sin(theta)
            var sinAngle = axis.Length;

            // clamp in case of numerical error
            if (sinAngle > 1.0)
                sinAngle = 1.0;

            // get rotation angle. Note sinAngle in [0,1] => theta in [0,pi/2]
            var theta = Asin(sinAngle);
            var otherTheta = Math.PI - theta;

            // if cos(theta) < 0, use other theta as rotation angle.
            if (Vec3.Dot(u1, u2) < 0)
            {
                theta = otherTheta;
                otherTheta = Math.PI - theta;
            }

            if (AreClose(theta, 0.0)) // todo - tolerance?
                return new(); // return identity if angle is approximately zero

            if (AreClose(otherTheta, 0.0))
            {
                // u1 = -u2, select one perpendicular vector
                // u1 and u2 are ~ opposites. Rotate on any fixed perpendicular
                if (!AreClose((u1.Y * u1.Y + u1.Z * u1.Z), 0.0))
                {
                    // first try x-axis if u1 not parallel
                    axis.X = 0;
                    axis.Y = u1.Z;
                    axis.Z = -u1.Y;
                }
                else
                {
                    // u1 parallel to x-axis. Use z-axis for rotation
                    axis.Y = axis.Y = 0;
                    axis.Z = 1.0;
                }
            }

            return FromAxisAngle(axis.Normalized(), theta).Normalized();
        }

        /// <summary>
        /// Quat from roll, pitch, and yaw, of which there are many versions.
        /// This is from the convention https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles,
        /// which is a right handed coord, right handed angles, Tait-Bryan intrinsic active z-y'-x''
        /// </summary>
        /// <param name="roll">in -pi to pi</param>
        /// <param name="pitch">in -pi/2 to -pi/2</param>
        /// <param name="yaw">in -pi to pi</param>
        /// <returns></returns>
        public static Quat FromRollPitchYaw(double roll, double pitch, double yaw)
        {
            // Abbreviations for the various angular functions
            var cy = Cos(yaw * 0.5);
            var sy = Sin(yaw * 0.5);
            var cr = Cos(roll * 0.5);
            var sr = Sin(roll * 0.5);
            var cp = Cos(pitch * 0.5);
            var sp = Sin(pitch * 0.5);

            return new(
                cy * cr * cp + sy * sr * sp, // a or w
                cy * sr * cp - sy * cr * sp, // b or x
                cy * cr * sp + sy * sr * cp, // c or y
                sy * cr * cp - cy * sr * sp // d or z
            );
        }

        /// <summary>
        /// Quat from roll, pitch, and yaw, of which there are many versions.
        /// This is from the convention https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles,
        /// which is a right handed coord, right handed angles, Tait-Bryan intrinsic active z-y'-x''
        /// </summary>
        /// <param name="roll">in -pi to pi</param>
        /// <param name="pitch">in -pi/2 to -pi/2</param>
        /// <param name="yaw">in -pi to pi</param>
        public void ToRollPitchYaw(out double roll, out double pitch, out double yaw)
        {
            // roll (x-axis rotation)
            var sinr = 2.0 * (A * B + C * D);
            var cosr = 1.0 - 2.0 * (B * B + C * C);
            if (AreClose(sinr, 0) && AreClose(cosr, 0))
            {
                throw new ArgumentException(""); // invalid inputs
                roll = pitch = yaw = 0;
                return;
            }

            roll = Atan2(sinr, cosr);

            // pitch (y-axis rotation)
            var sinp = +2.0 * (A * C - D * B);
            if (Abs(sinp) >= 1)
                pitch = CopySign(Math.PI / 2, sinp); // use 90 degrees if out of range
            else
                pitch = Asin(sinp);

            // yaw (z-axis rotation)
            var siny = +2.0 * (A * D + B * C);
            var cosy = +1.0 - 2.0 * (C * C + D * D);
            if (AreClose(siny, 0) && AreClose(cosy, 0))
            {
                throw new ArgumentException(""); // invalid inputs
                roll = pitch = yaw = 0;
                return;
            }

            yaw = Atan2(siny, cosy);
        }

        /// <summary>
        /// Convert quat to rotation matrix
        /// </summary>
        /// <returns></returns>
        public Mat3 ToRotationMatrix()
        {
            var u = Normalized();
            return new Mat3(
                1 - 2 * u.Y * u.Y - 2 * u.Z * u.Z, 2 * u.X * u.Y - 2 * u.Z * u.W, 2 * u.X * u.Z + 2 * u.Y * u.W,
                2 * u.X * u.Y + 2 * u.Z * u.W, 1 - 2 * u.X * u.X - 2 * u.Z * u.Z, 2 * u.Y * u.Z - 2 * u.X * u.W,
                2 * u.X * u.Z - 2 * u.Y * u.W, 2 * u.Y * u.Z + 2 * u.X * u.W, 1 - 2 * u.X * u.X - 2 * u.Y * u.Y
                );
        }
        /// <summary>
        /// Convert quat to rotation matrix as 4x4
        /// </summary>
        /// <returns></returns>
        public Mat4 ToRotationMatrix4() => new Mat4(ToRotationMatrix());

        [Obsolete]
        public static Quat GetRotation(Mat4 m) => FromRotationMatrix(m);

        public static Quat FromRotationMatrix(Mat4 m) => FromRotationMatrix(m.ToMat3());

        /// <summary>
        /// Get rotation from matrix
        /// Assumes orthogonal
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Quat FromRotationMatrix(Mat3 m)
        {
            double m00 = m[0, 0], m01 = m[0, 1], m02 = m[0, 2];
            double m10 = m[1, 0], m11 = m[1, 1], m12 = m[1, 2];
            double m20 = m[2, 0], m21 = m[2, 1], m22 = m[2, 2];

            // some tricks to ensure divisors are large.
            // based on unit vector having at least one large component
            double t;
            Quat q;
            if (m22 < 0) // iff x^2+y^2 > 1/2
            {
                if (m00 > m11) // x^2 > y^2
                {
                    t = 1 + m00 - m11 - m22; // 4x^2
                    q = new(m21 - m12, t, m01 + m10, m20 + m02);
                }
                else // work with y as divisor
                {
                    t = 1 - m00 + m11 - m22; // 4y^2
                    q = new(m02 - m20, m01 + m10, t, m12 + m21);
                }
            }
            else // z^2+w^2 > 1/2
            {
                if (m00 < -m11)
                {
                    t = 1 - m00 - m11 + m22; // 4z^2
                    q = new(m10 - m01, m20 + m02, m12 + m21, t);
                }
                else
                {
                    t = 1 + m00 + m11 + m22; // 4-4(x^2+y^2+z^2)=4w^2
                    q = new(t, m21 - m12, m02 - m20, m10 - m01);
                }
            }

            q *= 0.5 / Sqrt(t);
            return q.Normalized();
        }
        #endregion

        #region Formatting
        public override string ToString() => $"({A},{B},{C},{D})";

        #endregion

    }
}




