using Lomont.Graphics;
using Lomont.Algorithms;

var p = @"LomontSharp\TestData\Lenna.png";
while (p.Length < 100 && !File.Exists(p))
    p = @"..\" + p;

Console.WriteLine($"{p} {File.Exists(p)}");

// get image to test
var src = new SimpleBitmap(p);

Filter(src, 1.0f).Save("gauss1.png");
Filter(src, 3.0f).Save("gauss3.png");
Filter(src, 5.0f).Save("gauss5.png");
Filter(src, 1.0f, 0.0f).Save("bilat1_0.png");
Filter(src, 3.0f, 0.0f).Save("bilat3_0.png");
Filter(src, 5.0f, 0.0f).Save("bilat5_0.png");
Filter(src, 1.0f, 1.0f).Save("bilat1_1.png");
Filter(src, 3.0f, 1.0f).Save("bilat3_1.png");
Filter(src, 5.0f, 1.0f).Save("bilat5_1.png");
Filter(src, 1.0f, 3.0f).Save("bilat1_3.png");
Filter(src, 3.0f, 3.0f).Save("bilat3_3.png");
Filter(src, 5.0f, 3.0f).Save("bilat5_3.png");
Filter(src, 1.0f, 5.0f).Save("bilat1_5.png");
Filter(src, 3.0f, 5.0f).Save("bilat3_5.png");
Filter(src, 5.0f, 5.0f).Save("bilat5_5.png");
Filter(src, 1.0f, 7.0f).Save("bilat1_7.png");
Filter(src, 3.0f, 7.0f).Save("bilat3_7.png");
Filter(src, 5.0f, 7.0f).Save("bilat5_7.png");

SimpleBitmap Filter(SimpleBitmap src, float spatialSigma = 3.0f, float intensitySigma = -1.0f)
{
    // split into float channels
    var (rf, gf, bf, af) = Split(src);

    // filter each channel independently
    if (intensitySigma >= 0.0f)
    { // bilateral
        rf = Filters.Bilateral(rf, spatialSigma, intensitySigma);
        gf = Filters.Bilateral(gf, spatialSigma, intensitySigma);
        bf = Filters.Bilateral(bf, spatialSigma, intensitySigma);
        af = Filters.Bilateral(af, spatialSigma, intensitySigma);

    }
    else
    { // gaussian
        rf = Filters.Gaussian(rf, spatialSigma);
        gf = Filters.Gaussian(gf, spatialSigma);
        bf = Filters.Gaussian(bf, spatialSigma);
        af = Filters.Gaussian(af, spatialSigma);
    }

    // repack layers
    return Merge(rf, gf, bf, af);


    (float[,] r, float[,] g, float[,] b, float[,] a) Split(SimpleBitmap src)
    {
        var (w, h) = (src.Width, src.Height);
        var rf = new float[w, h];
        var gf = new float[w, h];
        var bf = new float[w, h];
        var af = new float[w, h];
        for(var j =0; j < h; ++j)
        for (var i = 0; i < w; ++i)
        {
            // we even ignore scaling - filters should still work 0-255
            var (r,g,b,a) = src.GetPixel(i, j);
            rf[i, j] = r;
            gf[i, j] = g;
            bf[i, j] = b;
            af[i, j] = a;
        }
        return (rf, gf, bf, af);
    }

    SimpleBitmap Merge(float[,] rf, float[,] gf, float[,] bf, float[,] af)
    {

        var (w, h) = (rf.GetLength(0), rf.GetLength(1));
        var dst = new SimpleBitmap(w, h);
        for (var j = 0; j < h; ++j)
        for (var i = 0; i < w; ++i)
        {
            var r = Clamp(rf[i, j]);
            var g = Clamp(gf[i, j]);
            var b = Clamp(bf[i, j]);
            var a = Clamp(af[i, j]);
            dst.SetPixel(i, j, r, g, b, a);
        }

        return dst;

        byte Clamp(float v)
        {
            if (v < 0) return 0;
            if (255 < v) return 255;
            return (byte) MathF.Round(v);
        }
    }
}
