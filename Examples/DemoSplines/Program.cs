// demo of spline points
// make into a path, render it
using Lomont.Geometry;
using Lomont.Numerical;
using SkiaSharp;

var size = 100.0; // side size, play in this grid
var ht = 5.0; // random height of points
var r = new Random(1234); // random source
var num = 20; // number of points

// random point
var pt = () => new Vec3(r.NextDouble() * size, r.NextDouble() * size, r.NextDouble() * ht);

// create spline control points
var points = new List<Vec3>();
for (var i = 0; i < num; ++i)
    points.Add(pt());

var degree = 3; // cubic motion

// create knot vector
// # knots = num pts + deg + 1;
var knots = new double[points.Count() + degree + 1];
for (var i = 0; i < knots.Length; ++i)
    knots[i] = i;

// the spline
var bs = new BSplineT<Vec3>(degree, points, knots);

// the image
var scale = 10.0; // pixels per unit size
Func<Vec3, (int, int)> pt1 = p => ((int)(p.X * scale), (int)(p.Y * scale));

var filename = "SplinePath.png";

using (var bmp = new SkiaSharp.SKBitmap((int)(size * scale), (int)(size * scale), SKColorType.Rgba8888, SKAlphaType.Premul))
{

    // graphics drawing
    using (var canvas = new SKCanvas(bmp))
    {
        canvas.Clear(SKColors.Wheat);
        using (var paint = new SKPaint())
        {
            paint.Style = SKPaintStyle.Stroke;
            paint.Color = SKColors.Red;
            paint.StrokeWidth = 1.0f;
            paint.StrokeCap = SKStrokeCap.Round;
            using (var path = new SKPath())
            {
                var (x, y) = pt1(points[0]);
                path.MoveTo(x, y);
                foreach (var p in points.Skip(1))
                {
                    (x, y) = pt1(p);
                    path.LineTo(x, y);
                }
                canvas.DrawPath(path, paint);
            }

            paint.Color = SKColors.Green;
            using (var path = new SKPath())
            {
                var (x, y) = pt1(points[0]);
                path.MoveTo(x, y);
                var u1 = knots[degree + 1];
                var u2 = knots[knots.Length - degree - 1];
                double u = u1;
                var p = bs.CurvePoint(u);
                (x, y) = pt1(p);
                path.MoveTo(x, y);
                while (u <= u2)
                {
                    p = bs.CurvePoint(u);
                    (x, y) = pt1(p);
                    path.LineTo(x, y);
                    u += (u2 - u1) / 1000;
                }
                canvas.DrawPath(path, paint);
            }
        }
    }

    using (var data = bmp.Encode(SKEncodedImageFormat.Png, 80))
    using (var stream = File.OpenWrite(filename))
    {
        data.SaveTo(stream);
    }
}

Console.WriteLine($"image {filename} saved");