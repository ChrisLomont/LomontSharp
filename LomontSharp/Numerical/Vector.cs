using System;
using static System.Math;
using System.Numerics;
using System.Diagnostics;
using System.Text;

#if true

//Edit proj file , check these lines
//
//  <PropertyGroup>
//    <EnablePreviewFeatures>true</EnablePreviewFeatures>
//    <LangVersion>preview</LangVersion>
//    <TargetFramework>net6.0</TargetFramework>
//    <Nullable>enable</Nullable>
//    <AssemblyName>LomontSharp</AssemblyName>
//    <RootNamespace>Lomont</RootNamespace>
//  </PropertyGroup>
//
//  <ItemGroup>
//    <PackageReference Include="System.Runtime.Experimental" Version="6.0.0-preview.7.21377.19" />
//  </ItemGroup>


// one class for all
namespace Lomont.Numerical
{
    public class Vector<T> where T : INumber<T>
    {
        public T[] Values;

        public int Dimension => Values.Length;

        #region Constructors
        /// <summary>
        /// Zero vector
        /// </summary>
        /// <param name="n"></param>
        public Vector(int n)
        {
            Values = new T[n];
        }

        /// <summary>
        /// set values
        /// </summary>
        /// <param name="n"></param>
        /// <param name="values"></param>
        public Vector(int n, params T[] values)
        {
            Values = new T[n];
            Array.Copy(values, Values, n);
        }

        /// <summary>
        /// copy constructot
        /// </summary>
        /// <param name="n"></param>
        /// <param name="values"></param>
        public Vector(Vector<T> v)
        {
            Values = new T[v.Dimension];
            Array.Copy(v.Values, Values, Dimension);
        }
        #endregion

        public T LengthSquared => Dot(this, this);

        public static T Dot(Vector<T> a, Vector<T> b)
        {
            Trace.Assert(a.Dimension == b.Dimension);
            T sum = T.Zero;
            for (var i = 0; i < a.Dimension; ++i)
                sum += a[i] * b[i];
            return sum;
        }

        public T this[int index]
        {
            get => Values[index];
            set => Values[index] = value;
        }

        public static Vector<T> operator +(Vector<T> a) => a;

        public static Vector<T> operator -(Vector<T> a) => a.Map(v=>-v);

        public static Vector<T> operator +(Vector<T> a, Vector<T> b)
        {
            var v = new Vector<T>(a.Dimension);
            for (var i = 0; i < a.Dimension; ++i)
                v[i] = a[i] + b[i];
            return v;
        }
        public static Vector<T> operator -(Vector<T> a, Vector<T> b)
        {
            var v = new Vector<T>(a.Dimension);
            for (var i = 0; i < a.Dimension; ++i)
                v[i] = a[i] - b[i];
            return v;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"({Values[0]}");
            for (var i = 1; i < Dimension; ++i)
                sb.Append($",{Values[i]}");
            sb.Append(")");

            return sb.ToString();
        }

        public static Vector<T> ComponentwiseMin(Vector<T> a, Vector<T> b)
        {
            return Componentwise(a, b, (a, b) => a < b ? a : b);

        }
        public static Vector<T> ComponentwiseMax(Vector<T> a, Vector<T> b)
        {
            return Componentwise(a, b, (a, b) => a < b ? b : a);
        }

        #region Functor
        public Vector<T> Map(Func<T, T> func)
        {
            var v = new Vector<T>(Dimension);
            for (var i = 0; i < Dimension; ++i)
                v[i] = func(this[i]);
            return v;
        }

        public static Vector<T> Apply(Vector<T> a, Vector<T> b, Func<T, T, T> func)
        {
            Trace.Assert(a.Dimension == b.Dimension);
            var v = new Vector<T>(a.Dimension);
            for (var i = 0; i < a.Dimension; ++i)
                v[i] = func(a[i], b[i]);
            return v;
        }

        #endregion

        /// <summary>
        /// True if all components exactly zero
        /// </summary>
        public bool IsNull
        {
            get
            {
                for (var i = 0; i < Dimension; ++i)
                {
                    if (this[i] != T.Zero)
                        return false;
                }
                return true;
            }
        }


        public static Vector<T> Componentwise(Vector<T>  a, Vector<T>  b, Func<T, T, T> func)
        {
            Trace.Assert(a.Dimension == b.Dimension);

            var vals = new T[a.Dimension];
            for (var i = 0; i < a.Dimension; ++i)
                vals[i] = func(a[i], b[i]);
            return new Vector<T>(a.Dimension,vals);
        }
        /// <summary>
        /// Return max abs value of a component
        /// </summary>
        public T MaxNorm => Aggregate((v1, v2) => v1 < v2 ? v2 : v1);


        public T Aggregate(Func<T,T,T> func)
        {
            var v1 = this[0];
            for (var i = 1; i < Dimension; ++i)
                v1 = func(v1, this[i]);
            return v1;
        }

    }

}

#if false
    public class Matrix<T> where T : INumber<T>
    {
        T[,] vals;

        public int Rows { get; }
        public int Columns { get; }
        public Matrix(int rows, int columns)
        {
            Rows = rows; this.Columns = columns;
            vals = new T[rows, columns];
        }

        public Matrix(int rows, int columns, params T[] values)
        {
            Rows = rows; this.Columns = columns;
            vals = new T[rows, columns];
            for (var i = 0; i < Rows; ++i)
                for (var j = 0; j < this.Columns; ++j)
                    vals[i, j] = values[i * this.Columns + j];

        }

        public T this[int row, int column]
        {
            get => vals[row, column];
            set => vals[row, column] = value;
        }

        public static Vector<T> operator *(Matrix<T> m, Vector<T> v)
        {
            Trace.Assert(m.Columns == v.Dimension);
            var v2 = new Vector<T>(v.Dimension);
            for (var i = 0; i < m.Rows; ++i)
                for (var j = 0; j < m.Columns; ++j)
                    v2[i] += m[i, j] * v[j];
            return v2;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < Rows; ++i)
            {
                sb.Append("[");
                for (var j = 0; j < Columns; ++j)
                    sb.Append($"{vals[i, j]},");
                sb.AppendLine("]");
            }
            return sb.ToString();
        }

    }

    public class Vec3<T> : Vector<T> where T : INumber<T>
    {
        public T X { get => Values[0]; set => Values[0] = value; }
        public T Y { get => Values[1]; set => Values[1] = value; }
        public T Z { get => Values[2]; set => Values[2] = value; }

        public Vec3(T x, T y, T z) : base(3, x, y, z)
        {
        }
    }

    public class Vec4<T> : Vector<T> where T : INumber<T>
    {
        public T X { get => Values[0]; set => Values[0] = value; }
        public T Y { get => Values[1]; set => Values[1] = value; }
        public T Z { get => Values[2]; set => Values[2] = value; }
        public T W { get => Values[3]; set => Values[3] = value; }

        public Vec4(T x, T y, T z, T w /* how to do default parameters? = T.One*/) : base(4, x, y, z, w)
        {
        }

        public Vec3<T> Homogenize() => new(X / W, Y / W, Z / W);
    }

    public class Mat4<T> : Matrix<T> where T : INumber<T>
    {
        public Mat4() : base(4, 4)
        {
        }
        public Mat4(params T[] values) : base(4, 4, values)
        {
        }

    }


    // can do this :)
    public class Color<T> : Vector<T> where T : INumber<T>
    {
        public T R { get => Values[0]; set => Values[0] = value; }
        public T G { get => Values[1]; set => Values[1] = value; }
        public T B { get => Values[2]; set => Values[2] = value; }
        public T A { get => Values[4]; set => Values[4] = value; }

        public Color(T r, T g, T b, T a) : base(4)
        {
            R = r; G = g; B = b;
        }
    }

}
#endif

#endif