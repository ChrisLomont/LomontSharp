using System;
using System.Drawing;
using System.Drawing.Imaging;
using Lomont.Graphics.Fonts;

namespace Lomont.Graphics;

/// <summary>
/// Simple RGBA bitmap of bytes
/// </summary>
public class SimpleBitmap
{
    byte [] image;

    public int Width { get;  }
    public int Height { get; }

    const int channels = 4; // RGBA

    public SimpleBitmap(string filename)
    {
        var bmp = new Bitmap(filename);
        //Console.WriteLine(bmp.PixelFormat);
        (Width, Height) = (bmp.Width, bmp.Height);
        image = new byte[Width*Height*channels];
        // todo  - much faster to get pixel type, make some per type blitters
        for (var j = 0; j < Height; ++j)
        for (var i = 0; i < Width; ++i)
        {
            var c = bmp.GetPixel(i, j);
            SetPixel(i,j,c.R,c.G,c.B,c.A);
        }
    }

    public SimpleBitmap(int width, int height)
    {
        image = new byte[width * height * channels];
        Width = width;
        Height = height;
    }

    bool Legal(int i, int j) => 0 <= i && i < Width && 0 <= j && j < Height;
    int Index(int i, int j) => (i + j * Width) * channels;

    public void SetPixel(int i, int j, int red, int green, int blue, int alpha = 255)
    {
        if (!Legal(i, j)) return;
        var t = Index(i,j);
        image[t] = (byte)red;
        image[t+1] = (byte)green;
        image[t+2] = (byte)blue;
        image[t+3] = (byte)alpha;
    }
    public (int red, int green, int blue, int alpha) GetPixel(int i, int j)
    {
        if (!Legal(i, j)) return (0, 0, 0, 0);
        var t = Index(i, j);
        return (image[t], image[t + 1], image[t + 2], image[t+3]);
    }

    /// <summary>
    /// RGBA row major byte data
    /// </summary>
    public byte[] Data => image;

    public void DrawText(int i, int j, string text, BitmapFont? font = null)
    {
        font ??= new BitmapFont();
        font.Draw(i,j,text,
            (i1,j1,r,g,b)=>SetPixel(i1,j1,r,g,b)
        );
    }

    public void Fill(Color color)
    {
        var (r, g, b, a) = color.ToBytes();
        for (var j = 0; j < Height; ++j)
        for (var i = 0; i < Width; ++i)
        {
            SetPixel(i, j, r, g, b, a);
        }

    }

    public void Save(string filename)
    {
        var bmp = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

        // slow - make different? derive from this?
        for (var j = 0; j < Height; ++j)
        for (var i = 0; i < Width; ++i)
        {
            var (r, g, b, a) = GetPixel(i, j);
            var c = System.Drawing.Color.FromArgb(a, r, g, b);
            bmp.SetPixel(i, j,c);
        }

        bmp.Save(filename);
    }

}