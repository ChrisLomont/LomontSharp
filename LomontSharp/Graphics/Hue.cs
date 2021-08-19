using System;
using System.Diagnostics;
using static Lomont.Numerical.Utility;

namespace Lomont.Graphics
{
    /// <summary>
    /// Helper functions relating to hue based colors
    /// </summary>
    public static class Hue
    {
        #region HSL and HSV
        /// <summary>
        /// Convert floating point red, green, blue values to 
        /// hue, saturation, value
        /// r,g,b values are from 0 to 1
        /// Returns h in [0,360), s in [0,1], v in [0,1]
        ///		if s == 0, then h = v = 0 (both actually undefined)
        /// Preserves color alpha
        /// </summary>
        public static Color RGBtoHSV(Color color)
        {
            var (r, g, b) = RGBtoHSV(color.R, color.G, color.B);
            return new Color(r, g, b, color.A);
        }

        /// <summary>
        /// Convert floating point red, green, blue values to 
        /// hue, saturation, value
        /// r,g,b values are from 0 to 1
        /// Returns h in [0,360), s in [0,1], v in [0,1]
        ///		if s == 0, then h = v = 0 (both actually undefined)
        /// </summary>
        public static (double hue, double saturation, double value) RGBtoHSV(double red, double green, double blue)
        {
            var (h, _, s, _, v) = RGBtoHSLV(red, green, blue);
            return (h, s, v);
        }


        /// <summary>
        /// Convert floating point hue, saturation, value values to 
        /// red, green, blue 
        /// r,g,b values are from 0 to 1
        /// Returns h in [0,360), s in [0,1], v in [0,1]
        ///		if s == 0, r=g=b=value
        /// Preserves color alpha
        /// </summary>
        public static Color HSVtoRGB(Color color)
        {
            var (r, g, b) = HSVtoRGB(color.H, color.S, color.V);
            return new Color(r, g, b, color.A);
        }

        public static Color HSLToRGB(Color color)
        {
            var (h, s, l, a) = color;
            var (r, g, b) = HSLToRGB(h, s, l);
            return new(r,g,b,a);
        }

        public static (double red, double green, double blue) HSLToRGB(double hue, double saturation, double lightness) => HSLVtoRGB(hue, saturation, lightness, false);

        public static Color RGBToHSL(Color color)
        {
            var (r, g, b, a) = color;
            var (h,s,l) = RGBToHSL(r, g, b);
            return new(h, s, l, a);
        }

        public static (double hue, double saturation, double lightness) RGBToHSL(double r, double g, double b)
        {
            var (h, s, _, l, _) = RGBtoHSLV(r, g, b);
            return (h,s,l);
        }


        /// <summary>
        /// Convert floating point hue, saturation, value values to 
        /// red, green, blue 
        /// r,g,b values are from 0 to 1
        /// Returns h in [0,360), s in [0,1], v in [0,1]
        ///		if s == 0, r=g=b=value
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="saturation"></param>
        /// <param name="value"></param>
        public static (double red, double green, double blue) HSVtoRGB(double hue, double saturation, double value)
        {
            return HSLVtoRGB(hue, saturation, value, true);
        }

        #endregion

        #region Normalize

        /// <summary>
        /// Return a hue in [0,360)
        /// </summary>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static double Normalize(double hue) => Wrap(hue,0,360);

        /// <summary>
        /// Normalize hueMin to [0,360), and normalize hueMax to smallest 
        /// equivalent hue strictly greater than hueMin.
        /// </summary>
        /// <param name="hueMin"></param>
        /// <param name="hueMax"></param>
        /// <returns></returns>
        public static (double hueMin, double hueMax) Normalize(double hueMin, double hueMax)
        {
            const double tolerance = 0.0001;
            var delta = hueMax - hueMin;
            hueMin = Normalize(hueMin);
            hueMax = Normalize(hueMax);
            if (hueMax <= hueMin + tolerance)
                hueMax += 360.0;
            if (Math.Abs(hueMax - hueMin) < tolerance && Math.Abs(delta - 360.0) < tolerance)
                hueMax += 360.0; // restore this spacing property
            return (hueMin, hueMax);
        }

        /// <summary>
        /// Normalize hueMin to [0,360), and normalize hueMax to smallest 
        /// equivalent hue strictly greater than hueMin. Finally normalize
        /// hueMid to between hueMin and hueMax, inclusive.
        /// If hueMid cannot be placed there, throw exception
        /// </summary>
        /// <returns></returns>
        public static (double min, double mid, double max) Normalize(double hueMin, double hueMid, double hueMax)
        {
            (hueMin, hueMax) = Normalize(hueMin, hueMax);
            hueMid = Normalize(hueMid);
            if (hueMid < hueMin)
                hueMid += 360.0;
            if (hueMax < hueMid)
            {
                Trace.TraceError("hue normalize error");
                throw new Exception("HueTools middle out of range");
            }
            return (hueMin, hueMid, hueMax);
        }

        #endregion

        #region Distances

        /// <summary>
        /// Compute min distance between two hues, allowing wrapping
        /// Always nonnegative
        /// Compute cyclical distance between two hues 0-360
        /// value in 0-180
        /// </summary>
        /// <param name="hue1"></param>
        /// <param name="hue2"></param>
        /// <returns></returns>
        public static double HueDistance(double hue1, double hue2)
        {
            hue1 = Normalize(hue1);
            hue2 = Normalize(hue2);
            var dh = Math.Abs(hue1 - hue2);
            if (dh > 360 / 2.0) // other direction shorter
                dh = 360 - dh;
            return dh;
        }

        /// <summary>
        /// Color distance, hue wrapped and taken as closest direction
        /// Optional weights per component. Default treats all as equal
        /// </summary>
        /// <param name="color1"></param>
        /// <param name="color2"></param>
        /// <param name="wt1"></param>
        /// <param name="wt2"></param>
        /// <param name="wt3"></param>
        /// <returns></returns>
        public static double HslvColorDistance(Color color1, Color color2, double wt1 = 1/360.0, double wt2 = 1.0, double wt3 = 1.0)
        {
            var (h1, s1, lv1, a1) = color1;
            var (h2, s2, lv2, a2) = color2;

            var dh = wt1*(HueDistance(h1, h2));
            var ds = wt2*(color1.S - color2.S);
            var dl = wt3*(color1.L - color2.L);

            return Math.Sqrt(dh * dh + ds * ds + dl * dl)/3; // /3 to make default in 0-1
        }

        #endregion

        #region Scaling

        /*

            Scaling: from our mathematica analysis, we took a parabola, rotated it, and
            used it to scale. The result is a function f(x,h) with stretch parameter h, which is

            f[x_, h_] := (-(1/Sqrt[2]) + 2 h - 2 h x + Sqrt[1/2 - 2 Sqrt[2] h + 4 h^2 + 4 Sqrt[2] h x])/(2 h)

            h can be in 0 to 0.35 or so.

            Then we map a hue in [0,1) to [0,1) via

            scale[hue_, t_] := Module[{ht, hi, hf, ha},
              hf = FractionalPart[hue*6];
              hi = Floor[hue*6 - hf];
              hi = Mod[hi, 6];
              ha = If[OddQ[hi], 1 + hi - f[1 - hf, t], hi + f[hf, t]];
              Return[ha/6]
              ]

            for Christmas lights we used h=0.10.

        */

        /// <summary>
        /// Default power for hue scaling
        /// Makes the size of the various color variations more visually pleasing.
        /// </summary>
        public const double DefaultHueScalePower = 1.6;

        /// <summary>
        /// Given a hue in 0-360, scales internal values such that
        /// colors are visually more evenly represented.
        /// Power 1.6 seems to be a good value. Power 1 is normal hue mapping
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static double ScaleHue(double hue, double power = DefaultHueScalePower, bool oldStyle = false)
        {
            // the idea is to keep the 6 base colors at 0,1/6,2/6,etc fixed, and make 
            // the odd value more represented, since they are not well represented now.
            // For example, in a hue cycle, yellow is much shorter than red

            // make sure h in [0,360). Also returns value back in same phase as it came in
            hue /= 360; // scale to 0-1
            var floor = Math.Floor(hue);
            hue -= floor;

            if (oldStyle)
            {

                var hInt = (int)Math.Floor(6 * hue);
                var hFrac = hue * 6 - hInt;
                var hOdd = (hInt & 1) != 0;
                var hNew = hInt + (hOdd ? Math.Pow(hFrac, power) : Math.Pow(hFrac, 1.0 / power));
                return 360*hNew / 6 + floor;
            }
            else
            {
                // this function seems better - matches values AND first derivative at each end
                power = 1.0 / power;
                // scale to [0,6)
                var hInt = (int)Math.Floor(6 * hue);
                var hFrac = hue * 6 - hInt;

                var hNew = (hInt & 1) == 0
                    ? Math.Pow(hFrac, power)
                    : 1 - Math.Pow(2 - (hFrac + 1), power);
                return 360*(hNew + hInt) / 6 + floor;
            }
        }

        #endregion

        #region Misc
        /// <summary>
        /// Return true if hue is in [min,max)
        /// </summary>
        /// <param name="hueMin"></param>
        /// <param name="hueMax"></param>
        /// <param name="hue"></param>
        /// <returns></returns>
        public static bool HueContained(double hueMin, double hueMax, double hue)
        {
            (hueMin, hueMax) = Normalize(hueMin, hueMax);
            hue = Normalize(hue);
            if (hue < hueMin)
            { // NOTE: wrapping hue up one fails due to numerical instabilities
                hueMax -= 360.0;
                hueMin -= 360.0;
                //hue += 360.0; // wrap up one
            }
            return hueMin <= hue && hue < hueMax;
        }

        /// <summary>
        /// Interpolater color from src (ratio = 0.0) to dst color (ratio = 1.0)
        /// Also blends alpha
        /// </summary>
        /// <param name="srcColor"></param>
        /// <param name="dstColor"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static Color Interpolate(Color srcColor, Color dstColor, double ratio)
        {
            var saturation = srcColor.S * (1 - ratio) + dstColor.S * ratio;
            var lightness = srcColor.L * (1 - ratio) + dstColor.L * ratio;

            // hue is tricky, move along the shortest direction hSrc to hDst
            var hSrc = srcColor.H;
            var hDst = dstColor.H;

            var hDelta = hDst - hSrc;
            if (Math.Abs(hDelta) > 180)
                hDelta = -Math.Sign(hDelta) * (360 - Math.Abs(hDelta));

            var hue = hSrc + ratio * hDelta;

            return new (hue, saturation, lightness,(srcColor.A + dstColor.A)/2);
        }
        #endregion

        #region Implementation

        // Given RGB in [0,1], compute HSL and HSV 
        static (double hue, double saturationL, double saturationV, double lightness, double value) RGBtoHSLV(double red, double green, double blue)
        {
            const double tolerance = 0.0001; // values this close to 0 are treated as equal

            red = Clamp01(red);
            green = Clamp01(green);
            blue = Clamp01(blue);

            double hue, sHsl, sHsv, value, lightness;

            var min = Math.Min(Math.Min(red, green), blue);
            var max = Math.Max(Math.Max(red, green), blue);
            value = max;                // v 
            lightness = (min + max) / 2.0;
            var chroma = max - min;     // chroma in [0,1]
            if (max < tolerance)
            {
                // r = g = b = 0		// s = 0, v is undefined, we choose 0?
                sHsl = sHsv = 0;
                value = 0;
                hue = 0;
            }
            else
            {

                sHsv = Clamp01(chroma / max); // s
                sHsl = Clamp01(chroma / (1 - Math.Abs(2 * lightness - 1)));

                if (Math.Abs(red - max) < tolerance)
                    hue = (green - blue) / chroma; // between yellow & magenta, h = (R-G)/C + 4 in [4,5]
                else if (Math.Abs(green - max) < tolerance)
                    hue = 2 + (blue - red) / chroma; // between cyan & yellow, h = (B-R)/C + 2 in [1,3]
                else
                    hue = 4 + (red - green) / chroma; // between magenta & cyan, h = (R-G)/C + 4 in [4,5]
                hue *= 60; // degrees
                if (hue < 0)
                    hue += 360;
            }

            return (hue, sHsl, sHsv, lightness, value);
        }

        static (double red, double green, double blue) HSLVtoRGB(
            double hue, double saturation, double lightnessOrValue, bool useValue
        )
        {
            hue = Wrap(hue, 0, 360);
            var lv = lightnessOrValue;
            var s = saturation;

            double red, green, blue;
            const double tolerance = 0.0001; // values this close to 0 are treated as equal
            if (saturation <= tolerance)
            {
                // achromatic (grey)
                red = green = blue = lightnessOrValue;
            }
            else
            { // chromatic case
                double c, m;
                if (useValue)
                {
                    c = lv * s;
                    m = lv - c;
                }
                else
                {
                    c = (1 - Math.Abs(2 * lv - 1)) * s; // chroma
                    m = lv - c * 0.5;
                }

#if true
                var hp = hue / 60; // sector 0 to 5
                double ab = (hp / 2 - Math.Floor(hp / 2)) * 2; // hp mod 2
                double x = c * (1 - Math.Abs(ab - 1));
                red = green = blue = 0;
                if (hp < 1)
                {
                    red = c;
                    green = x;
                }
                else if (hp < 2)
                {
                    red = x;
                    green = c;
                }
                else if (hp < 3)
                {
                    green = c;
                    blue = x;
                }
                else if (hp < 4)
                {
                    green = x;
                    blue = c;
                }
                else if (hp < 5)
                {
                    blue = c;
                    red = x;
                }
                else if (hp < 6)
                {
                    blue = x;
                    red = c;
                }
                red += m;
                green += m;
                blue += m;
#else

                var tempHue = hue / 60; // sector 0 to 5
                var i = (int)Math.Floor(tempHue); // todo - redundant?
                var f = tempHue - i; // fractional part of h
                var p = value * (1 - saturation);
                var q = value * (1 - saturation * f);
                var t = value * (1 - saturation * (1 - f));
                switch (i % 6)
                {
                    case 0:
                        red = value;
                        green = t;
                        blue = p;
                        break;
                    case 1:
                        red = q;
                        green = value;
                        blue = p;
                        break;
                    case 2:
                        red = p;
                        green = value;
                        blue = t;
                        break;
                    case 3:
                        red = p;
                        green = q;
                        blue = value;
                        break;
                    case 4:
                        red = t;
                        green = p;
                        blue = value;
                        break;
                    default: // case 5:
                        red = value;
                        green = p;
                        blue = q;
                        break;
                }
#endif
            }
            // sanity check
            double tolerance2 = 0.005;
            Trace.Assert(0 - tolerance2 <= red && red <= 1.0 + tolerance2);
            Trace.Assert(0 - tolerance2 <= green && green <= 1.0 + tolerance2);
            Trace.Assert(0 - tolerance2 <= blue && blue <= 1.0 + tolerance2);
            red = Clamp01(red);
            green = Clamp01(green);
            blue = Clamp01(blue);

            return (red, green, blue);
        }

#endregion
    }
}
