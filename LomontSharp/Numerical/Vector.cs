// todo - use this when .net 6.0+ has proper support for the INumber interface

#if false

Edit proj file , check these lines

  <PropertyGroup>
    <EnablePreviewFeatures>true</EnablePreviewFeatures>
    <LangVersion>preview</LangVersion>
    <TargetFramework>net6.0</TargetFramework>
    <Nullable>enable</Nullable>
    <AssemblyName>LomontSharp</AssemblyName>
    <RootNamespace>Lomont</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.Runtime.Experimental" Version="6.0.0-preview.7.21377.19" />
  </ItemGroup>


// one class for all
namespace Lomont
{
    public class Vector<T> where T : INumber<T>
    {
        protected T[] vals;

        public int Dimension => vals.Length;
        public Vector(int n)
        {
            vals = new T[n];
        }
        public Vector(int n, params T[] values)
        {
            vals = new T[n];
            Array.Copy(values, vals, n);
        }

        public T this[int index]
        {
            get => vals[index];
            set => vals[index] = value;
        }
        public static Vector<T> operator +(Vector<T> a, Vector<T> b)
        {
            var v = new Vector<T>(a.Dimension);
            for (var i = 0; i < a.Dimension; ++i)
                v[i] = a[i] + b[i];
            return v;
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"({vals[0]}");
            for (var i = 1; i < Dimension; ++i)
                sb.Append($",{vals[i]}");
            sb.Append(")");

            return sb.ToString();
        }
    }

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
        public T X { get => vals[0]; set => vals[0] = value; }
        public T Y { get => vals[1]; set => vals[1] = value; }
        public T Z { get => vals[2]; set => vals[2] = value; }

        public Vec3(T x, T y, T z) : base(3, x, y, z)
        {
        }
    }

    public class Vec4<T> : Vector<T> where T : INumber<T>
    {
        public T X { get => vals[0]; set => vals[0] = value; }
        public T Y { get => vals[1]; set => vals[1] = value; }
        public T Z { get => vals[2]; set => vals[2] = value; }
        public T W { get => vals[3]; set => vals[3] = value; }

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
        public T R { get => vals[0]; set => vals[0] = value; }
        public T G { get => vals[1]; set => vals[1] = value; }
        public T B { get => vals[2]; set => vals[2] = value; }
        public T A { get => vals[4]; set => vals[4] = value; }

        public Color(T r, T g, T b, T a) : base(4)
        {
            R = r; G = g; B = b;
        }
    }

}
#endif