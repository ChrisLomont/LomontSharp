using System;

namespace Lomont.Graphics
{
    /// <summary>
    /// Represent a color. A color has three color components and an alpha
    /// Default colors are in RGB color space. Functions exist to move them to other spaces.
    /// Tracking which space is not done in the Color itself.
    /// </summary>

    public class Color
    {
        #region Component ops

        /// <summary>
        /// Apply to color compoemnts, keep alpha
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public Color Map(Func<double, double> func) => new(func(R), func(G), func(B), A);

        /// <summary>
        /// Return a new color that has the given function applied to each R,G,B component
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="mergeFunction"></param>
        /// <returns></returns>
        public static Color MergeByFunction(Color color1, Color color2, Func<double, double, double> mergeFunction)
        {
            return new Color(
                mergeFunction(color1.R, color2.R),
                mergeFunction(color1.G, color2.G),
                mergeFunction(color1.B, color2.B)
            );
        }

        public (byte r, byte g, byte b, byte a) ToBytes()
        {
            var (r, g, b, a) = this; // double
            return
            (
                ColorUtils.Upscale(r),
                ColorUtils.Upscale(g),
                ColorUtils.Upscale(b),
                ColorUtils.Upscale(a)
            );

        }

        /// <summary>
        /// Return a new color that has the each R,G,Blue component the max of the others
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static Color Max(Color color1, Color color2) => MergeByFunction(color1, color2, Math.Max);

        //todo - blend/Overlapped funcs;

        //todo - Vec3 ops;

        /// <summary>
        /// Add the color to this one. Alpha unchanged
        /// </summary>
        /// <param name="color"></param>
        public void Add(Color color)
        {
            R += color.R;
            G += color.G;
            B += color.B;
        }

        /// <summary>
        /// Subtract the color from this one. Alpha unchanged
        /// </summary>
        /// <param name="color"></param>
        public void Subtract(Color color)
        {
            R -= color.R;
            G -= color.G;
            B -= color.B;
        }


        /// <summary>
        /// Return a new color that has the each R,G,Blue component the max of the others
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <returns></returns>
        public static Color Min(Color color1, Color color2) => MergeByFunction(color1, color2, Math.Min);


        #endregion

        #region Constructors and Properties

        public Color(Color c) : this(c.R, c.G, c.B, c.A)
        {
        }

        public Color(double r = 1.0, double g = 1.0, double b = 1.0, double a = 1.0)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public double R { get; set; } = 1.0;
        public double G { get; set; } = 0.0;
        public double B { get; set; } = 1.0;
        public double A { get; set; } = 1.0;

        /// <summary>
        /// Hue in 0 to 360.0
        /// </summary>
        public double H => R;

        /// <summary>
        /// Saturation in 0-1
        /// </summary>
        public double S => G;

        /// <summary>
        /// Value in 0-1 for HSV
        /// </summary>
        public double V => B;

        /// <summary>
        /// Lightness in 0-1 for HSL
        /// </summary>
        public double L => B;

        #endregion

        public void Deconstruct(out double red, out double green, out double blue)
        {
            red = R;
            green = G;
            blue = B;
        }

        public void Deconstruct(out double red, out double green, out double blue, out double alpha)
        {
            red = R;
            green = G;
            blue = B;
            alpha = A;
        }

        #region Makers

        /// <summary>
        /// Get a random color drawn uniformly from RGB space.
        /// To get a named color at random from Colors, see that class
        /// Alpha is full opacity
        /// </summary>
        /// <returns></returns>
        public static Color Random(Func<double> get) => new(get(), get(), get(), 1.0);

        /// <summary>
        /// Given hue in [0,360), saturation and lightness in [0,1]
        /// create a rgba color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="lightness"></param>
        /// <returns></returns>
        public static Color FromHSL(double hue, double saturation, double lightness, double alpha = 1.0)
        {
            var (r, g, b) = Hue.HSLToRGB(hue, saturation, lightness);
            return new(r, g, b, alpha);
        }

        /// <summary>
        /// Given hue in [0,360), saturation and value in [0,1]
        /// create a rgba color
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Color FromHSV(double hue, double saturation, double value, double alpha = 1.0)
        {
            var (r, g, b) = Hue.HSVtoRGB(hue, saturation, value);
            return new(r, g, b, alpha);
        }

        /// <summary>
        /// Given hue in [0,360], compute full S and V color
        /// </summary>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static Color FromHue(double hue)
        {

            return FromHSV(hue, 1, 1);
        }

        #endregion

        #region Formatting and parsing

// todo - hex formatting and parsing, int formatting and parsing

        public override string ToString()
        {
            return $"{R},{G},{B},{A}";
        }

        /// <summary>
        /// Try to parse the given text into a color.
        /// Return true on success, else false.
        /// On failure, the returned color is black (0,0,0)
        /// 
        public static bool TryParse(string text, out Color color)
        {
            var words = text.Split(new[] { ',' }, StringSplitOptions.None);
            color = new Color(0, 0, 0);
            if (
                words.Length > 3 &&
                double.TryParse(words[0], out var r) &&
                double.TryParse(words[1], out var g) &&
                double.TryParse(words[2], out var b) &&
                double.TryParse(words[3], out var a))
            {
                color = new Color(r, g, b, a);
                return true;
            }

            return false;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Color2 added as RGB, alpha averaged
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Color operator +(Color c1, Color c2)
        {
            // todo - alpha averaged. This good?
            return new Color(c1.R + c2.R, c1.G + c2.G, c1.B + c2.B, (c1.A + c2.A) / 2);
        }

        /// <summary>
        /// Multiply all values by the given scalar, excluding the alpha
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color operator *(double s, Color c)
        {
            return new Color(c.R * s, c.G * s, c.B * s, c.A);
        }

        /// <summary>
        /// Multiply all values by the given scalar, excluding the alpha
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color operator *(Color c, double s)
        {
            return s * c;
        }

        /// <summary>
        /// divide all values by the given scalar, excluding the alpha
        /// </summary>
        /// <param name="s"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color operator /(Color c, double s)
        {
            return c * (1 / s);
        }

        #endregion


        #region Named colors

        public static Color Black { get; } = new Color(0, 0, 0);
        public static Color Blue { get; } = new Color(0, 0, 1);
        public static Color Green { get; } = new Color(0, 1, 0);
        public static Color Cyan { get; } = new Color(0, 1, 1);
        public static Color Red { get; } = new Color(1, 0, 0);
        public static Color Magenta { get; } = new Color(1, 0, 1);
        public static Color Yellow { get; } = new Color(1, 1, 0);
        public static Color White { get; } = new Color(1, 1, 1);
        public static Color Gray { get; } = new Color(0.3, 0.3, 0.3);

        /// <summary>
        /// Eight 3 bit colors, black replaced with gray
        /// </summary>
        public static Color[] ThreeBitColors = { Gray, Blue, Green, Cyan, Red, Magenta, Yellow, White };

        #endregion
    }
}
