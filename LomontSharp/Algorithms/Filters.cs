using System;
using System.Diagnostics;

namespace Lomont.Algorithms;

/// <summary>
/// A place to put filters
/// </summary>
public static class Filters
{
    /* TODO - many to add
     * - make more abstract: int, float, rgba types, etc
     * - add 2d arbitrary kernel code
     *
     */

    /// <summary>
    /// Apply 1D separable kernel to 2D data with boundary reflection 
    /// </summary>
    /// <param name="kernel"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    static float[,] ApplySeparableKernel(float[] kernel, float[,] data)
    {
        var (w, h) = (data.GetLength(0), data.GetLength(1));
        var tmp = new float[w, h];
        var dst = new float[w, h];
        int sz = (kernel.Length - 1) / 2;

        // horiz to temp
        for (var j = 0; j < h; ++j)
            for (var i = 0; i < w; ++i)
            {
                var s = 0.0f;
                for (var k = -sz; k <= sz; ++k)
                    s += kernel[k + sz] * Get(data, w, h, i + k, j);
                tmp[i,j] = s;
            }
        // vert to final
        for (var j = 0; j < h; ++j)
        for (var i = 0; i < w; ++i)
        {
            var s = 0.0f;
            for (var k = -sz; k <= sz; ++k)
                s += kernel[k + sz] * Get(tmp, w, h, i, j + k);
            dst[i, j] = s;
        }

        return dst;
    }

    /// <summary>
    /// Create a gaussian kernel
    /// </summary>
    /// <param name="sigma"></param>
    /// <returns></returns>
    static (int sz, float[] kernel) GaussianKernel(float sigma)
    {
        // filter from -sz to sz inclusive
        // generally 3 sigma enough for image processing
        // we'll be more costly and use 4 sigma
        int sz = (int)MathF.Ceiling(4 * sigma);

        var kernel = new float[2 * sz + 1];
        var s2 = sigma * sigma;
        var eScale = -1.0f / (2 * s2);
        var ksum = 0.0f;
        for (var i = -sz; i <= sz; ++i)
        {
            var x = MathF.Sqrt(i * i);
            var v = MathF.Exp(x * x * eScale); // can skip 1/2pi sigmaSquared here - normalize below
            kernel[i + sz] = v;
            ksum += v;
        }

        // normalize result to make it nearer to lossless in energy
        var scale = 1.0f / ksum;
        for (var i = -sz; i <= sz; ++i)
            kernel[i + sz] *= scale;
        return (sz,kernel);
    }

    /// <summary>
    /// Gaussian blur
    /// Edges mirrored
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sigma"></param>
    /// <returns></returns>
    public static float[,] Gaussian(float[,] data, float sigma=1.0f)
    {
        var (_ , kernel1D) = GaussianKernel(sigma);
        return ApplySeparableKernel(kernel1D, data);
            
    }

    /// <summary>
    /// Get sample, mirrored edges
    /// </summary>
    /// <param name="data"></param>
    /// <param name="w"></param>
    /// <param name="h"></param>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <returns></returns>
    static float Get(float[,] data, int w, int h, int i, int j)
    {
        if (i < 0 || j < 0 || w <= i || h <= j)
        {
            i = Mirror(i, w);
            j = Mirror(j, h);
        }

        return data[i, j];

        static int Mirror(int v, int del)
        {
            v %= (2 * del); // mirror is 2*width wide
            if (v < 0) v = -v; // mirror on left
            if (v >= del) v = 2 * del - 1 - v; // mirror on right
            return v;
        }
    }

    /// <summary>
    /// Perform bilateral filter on data
    /// spatialSigma is space spread. Larger results in more "gaussian" blur
    /// intensitySigma is intensity spread. Higher keeps edges crisper. 0 results in Gaussian blur
    /// </summary>
    /// <param name="data"></param>
    /// <param name="spatialSigma"></param>
    /// <param name="intensitySigma"></param>
    /// <returns></returns>
    public static float[,] Bilateral(float[,] data, float spatialSigma=1.0f, float intensitySigma=1.0f)
    {
        // nice example 
        // https://www.geeksforgeeks.org/python-bilateral-filtering/
        // https://people.csail.mit.edu/sparis/bf_course/course_notes.pdf

        var (sz, kernel) = GaussianKernel(spatialSigma);


        var (w, h) = (data.GetLength(0), data.GetLength(1));
        var dst = new float[w, h];
        var i2 = intensitySigma * intensitySigma;
        var sc2 = i2 > 1e-10 ? 1.0f / MathF.Sqrt(2 * MathF.PI * i2) : 1;
        var sc = i2 > 1e-10 ? -1.0f / (2 * MathF.PI) : 0;

        for (var j = 0; j < h; ++j)
        for (var i = 0; i < w; ++i)
        {
            var p = Get(data, w, h, i, j); // center intensity
            var s = 0.0f; // sum for final intensity
            var weightSum = 0.0f; // weight
            for (var dj = -sz; dj <= sz; ++dj)
            {
                for (var di = -sz; di <= sz; ++di)
                {
                    var q = Get(data, w, h, i + di, j + dj);
                    var dI = p - q; // intensity difference
                    var weight = sc2*kernel[di + sz] * kernel[dj + sz] * MathF.Exp(sc * Math.Abs(dI));
                    s += weight * q;
                    weightSum += weight;
                }
            }

            dst[i, j] = s / weightSum;
        }

        return dst;
    }


}