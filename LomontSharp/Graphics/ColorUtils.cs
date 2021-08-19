using System;
using static Lomont.Numerical.Utility;

namespace Lomont.Graphics
{
    /// <summary>
    /// General color utilities
    /// </summary>
    public static class ColorUtils
    {
        // Code to do color transforms
        // See Computer Graphics by Foley and VanDam for details

        // accurate, invertible transforms between color spaces
        // http://www.getreuer.info/home/colorspace


#region Clamping and wrapping

        public enum ClampMode
        {
            Clamp,
            Wrap,
            Unbounded
        }

#if false // todo
        /// <summary>
        /// Clamp the given color component to the requested range using the current clamp mode
        /// </summary>
        /// <param name="component"></param>
        /// <param name="requestedValue"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        static void Clamp(out double component, double requestedValue, double minValue, double maxValue)
        {
            Trace.Assert(minValue <= maxValue, "Invalid condition in Clamp: min value not less than maxVal");
            component = requestedValue;
            switch (ClampMode)
            {
                case ColorClampMode.Clamp:
                    if (component < minValue) component = minValue;
                    if (maxValue < component) component = maxValue;
                    break;
                case ColorClampMode.Unbounded:
                    // do nothing
                    break;
                case ColorClampMode.Wrap:
                    var range = maxValue - minValue;
                    if (range < 0.00001)
                    {
                        component = minValue; // same ends
                    }
                    else
                    {
                        // clamp
                        var mult = (int)(component - minValue) / range;
                        component -= range * mult;
                        while (component < minValue)
                            component += range;
                        while (maxValue < component)
                            component -= range;
                    }

                    break;
                default:
                    throw new ArgumentException("Invalid color clamp mode in Color2.Clamp");
            }
        }

        /// <summary>
        /// Clamp the color to default ranges for the color space
        /// </summary>
        public Color Clamp(Color color, ColorSpace colorspace, ClampMode mode = ClampMode.Clamp)
        {
             todo - hue wraps, others clamp
            double temp;
            Clamp(out temp, R, 0, 1);
            R = temp;
            Clamp(out temp, G, 0, 1);
            G = temp;
            Clamp(out temp, B, 0, 1);
            B = temp;
            Clamp(out temp, A, 0, 1);
            A = temp;
        }
#endif

#endregion



        /// <summary>
        /// Convert color in [0,1] to [0,255]
        /// </summary>
        /// <param name="realColor"></param>
        /// <returns></returns>
        public static byte Upscale(double realColor)
        {
            realColor = Clamp01(realColor); // snap ends to [0,1]

            // 256 bins to fall into, each 1/256 wide, bin # thus Floor(v/(1/256))
            // 0 index by subtracting 1
            var bin = Math.Floor(realColor * 256) - 1;
            if (bin < 0) bin = 0;
            if (bin > 255) bin = 255; // not possible?
            return (byte)bin;
        }
        /// <summary>
        /// Convert color in [0,255] to [0,1]
        /// </summary>
        /// <param name="realColor"></param>
        /// <returns></returns>
        public static double Downscale(byte color)
        {
            // todo - derive this and above carefully to make round tripping robust and accurate
            throw new NotImplementedException("");
        }

        /// <summary>
        /// takes r,g,b in [0,1]
        /// returns value  [0,1]
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double RGBToGrayscale(double r, double g, double b)
        {
            return 0.299 * r + 0.587 * g + 0.114 * b;
        }

        /// <summary>
        /// Convert color to grayscale
        /// Preserves alpha
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color RGBToGrayscale(Color color)
        {
            var (r, g, b, a) = color;
            var val = RGBToGrayscale(r, g, b);
            return new(val,val,val,a);
        }


        /// <summary>
        /// takes value  [0,1]
        /// returns r,g,b in [0,1] (all set to value)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static (double red, double green, double blue) GrayscaleToRGB(double value)
        {
            return (value, value, value);
        }
        public static Color GrayscaleToRGB(Color color)
        {
            var (r, g, b, a) = color;
            var v = (r+b+b)/3; // r,g,b should be equal
            return new(r, g, b, a);
        }

        // returns a RGB color, in [0,1], which is sepia colored
        public static Color RGBToSepia(Color color)
        {
            var (r, g, b, a) = color;
            (r, g, b) = RGBToSepia(r, g, b);
            return new Color(r, g, b, a);
        }

        /// <summary>
        /// Return same color, best that can be done
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color SepiaToRGB(Color color) => color;

        // returns a RGB color, in [0,1], which is sepia colored
        public static (double red, double green, double blue) RGBToSepia(double red, double green, double blue)
        {
            // convert to YIQ space 
            var (Y, I, Q) = RGBToYIQ(red, green, blue);

            I = 0.2; // convert YIQ color to sepia tone 
            Q = 0.0;

            // convert back to RGB 
            var (r2, g2, b2) = YIQToRGB(Y, I, Q);

            // clamp
            r2 = Clamp01(r2);
            g2 = Clamp01(g2);
            b2 = Clamp01(b2);
            return (r2,g2,b2);
        }

        public static Color RGBToYIQ(Color color)
        {
            var (r, g, b, a) = color;
            var (Y,I,Q) = RGBToYIQ(r, g, b);
            return new(Y, I, Q, a);
        }

        public static (double Y, double I, double Q) RGBToYIQ(double red, double green, double blue)
        {
            var Y = 0.299 * red + 0.587 * green + 0.114 * blue;
            var I = 0.596 * red - 0.275 * green - 0.321 * blue;
            var Q = 0.212 * red - 0.523 * green + 0.311 * blue;
            return (Y, I, Q);
        }
        public static Color YIQToRGB(Color color)
        {
            var (Y,I,Q, a) = color;
            var (r, g, b) = YIQToRGB(Y, I, Q);
            return new(r, g, b, a);
        }

        public static (double red, double green, double blue) YIQToRGB(double Y, double I, double Q)
        {
            var r2 = (0.382072*Y + 1.123510*I + 1.019580*Q);
            var g2 = (1.175600*Y - 0.319271*I - 0.760464*Q);
            var b2 = (1.716520*Y - 1.302770*I + 1.241560*Q);
            return (r2,g2,b2);
        }


        /// <summary>
        /// Perform sRGB function on each color component
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static Color LinearRGBToSRGB(Color color) => color.Map(v => LinearRGBToSRGB(v));
        public static Color SRGBToLinearRGB(Color color) => color.Map(v => SRGBToLinearRGB(v));
        public static double SRGBToLinearRGB(double value, double gamma = MonitorGamma) =>
            (value > 0.04045) ? Math.Pow((value + 0.055) / 1.055, gamma) : (value / 12.92);

        public static double LinearRGBToSRGB(double val, double gamma = MonitorGamma) => 
            (val <= 0.0031308) ? Math.Max(0.0, 12.92 * val) : Clamp01(1.055 * Math.Pow(val, (1.0 / gamma)) - 0.055);


        /// <summary>
        /// Gamma we use for WS2812 LEDs
        /// </summary>
        public const double GammaWS2812 = 2.8; // from our LED strands, useful for WS2812

        /// <summary>
        /// Common gamma for monitors
        /// </summary>
        public const double MonitorGamma = 2.4; 


        /// <summary>
        /// Color correction used often by Hypnocube products
        /// </summary>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        public static void ColorCorrect(ref double red, ref double green, ref double blue)
        {
            var (hue,saturation,lightness) = Hue.RGBToHSL(red, green, blue);
            hue = Hue.ScaleHue(hue);
            (red,green,blue) =  Hue.HSLToRGB(hue, saturation, lightness);
            red = GammaCorrect(red);
            green = GammaCorrect(green);
            blue = GammaCorrect(blue);
        }

        public static double GammaCorrect(double component, double gamma = MonitorGamma)
        {
            return Math.Pow(component, gamma);
        }


        public static Color Convert(Color color, ColorSpace srcColorSpace, ColorSpace destColorSpace)
        {
            if (srcColorSpace == destColorSpace)
                return color;

            // convert to RGB color space
            var rgb = srcColorSpace switch
            {
                ColorSpace.sRGB => SRGBToLinearRGB(color),
                ColorSpace.RGB => color,
                ColorSpace.HSV => Hue.HSVtoRGB(color),
                ColorSpace.HSL => Hue.HSLToRGB(color),
                ColorSpace.CIEXYZ => CIEColor.XYZToRGB(color),
                ColorSpace.CIELAB => CIEColor.LabToRGB(color),
                ColorSpace.YIQ => YIQToRGB(color),
                ColorSpace.Grayscale => GrayscaleToRGB(color),
                ColorSpace.Sepia => SepiaToRGB(color),
                _ => throw new NotImplementedException()
            };

            // convert to final color space
            return destColorSpace switch
            {
                ColorSpace.sRGB => LinearRGBToSRGB(rgb),
                ColorSpace.RGB => rgb,
                ColorSpace.HSV => Hue.RGBtoHSV(rgb),
                ColorSpace.HSL => Hue.RGBToHSL(rgb),
                ColorSpace.CIEXYZ => CIEColor.RGBToXYZ(rgb),
                ColorSpace.CIELAB => CIEColor.RGBToLab(rgb),
                ColorSpace.YIQ => RGBToYIQ(rgb),
                ColorSpace.Grayscale => RGBToGrayscale(rgb),
                ColorSpace.Sepia => RGBToSepia(rgb),
                _ => throw new NotImplementedException()
            };
        }

#region Patterns
        /// <summary>
        /// Create space filling plasma color
        /// </summary>
        /// <param name="phase"></param>
        /// <param name="i"></param>
        /// <param name="j"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static Color Plasma(double phase, int i, int j, int k)
        {

            var angle = phase;

            // normalized coords in 0,1
            var x = (double)i / 8;
            var y = (double)j / 8;
            var z = (double)k / 8;
            //double s, t, u;
            var s = (Math.Cos(x * Math.PI / 1.4 + angle - 1.4 * Math.Cos(z * 4.4 - 1.1 * angle) +
                              2.1 * Math.Sin(1.5 * y + 2.1 * z - angle)) + 1) / 2.0;
            var t = (Math.Cos(y * Math.PI / 1.7 + 1.1 * angle - 1.7 * Math.Sin(x * 4.3 - 1.1 * angle) -
                              2.2 * Math.Cos(1.7 * x + 2.5 * z - 1.1 * angle)) + 1) / 2.0;
            var u = (Math.Sin(y * Math.PI * 0.7 - 1.3 * angle - 2.3 * Math.Sin(z * 3.3 + 0.9 * angle) -
                              2.1 * Math.Cos(1.5 * x - 2.4 * y - 0.8 * angle)) + 1) / 2.0;


            s = s * s * s; // some scaling to darken the colors
            t = t * t * t;
            u = u * u * u;

            //var tol = 1 / 100.0;
            //if (s < tol) s = 0;
            //if (t < tol) t = 0;
            //if (u < tol) u = 0;

#if false
            //s = (Math.Cos(phase) + 1) / 2;
            s = (Math.Cos(x + 6.5 * z + angle) + 1) / 2.0;
            //s = (Math.Cos(1.5*x+3*angle) + 1)/2;
            //s = (Math.Cos(x + angle + z + y) + 1) / 2.0;
            t = u = 0;
            //t = y;
            //u = 1;
            //if ((i == 0 && j == 0 && k == 0))
            //    s = t = u = 0;

            if (i == 0 && j == 0)
            {
                t = ((PlasmaPixelCount>>6) & (1 << k)) >> k;
                s = 0;
            }

            if (s < 0.01) s = 0;

            //x = (double)0/8;
            //s = (Math.Cos(x * Math.PI / 1.4 + angle - 1.4 * Math.Cos(z * 4.4 - 1.1 * angle) +
            //                  2.1 * Math.Sin(1.5 * y + 2.1 * z - angle)) + 1) / 2.0;

            //return new Color2(0.0,0,1.0);
            //return Color2.FromHue(123.0);
            //return new Color2(255,255,0);

            //Trace.TraceInformation("Comp: " + s);
            //if (s < 0) s = 0;
            //if (s > 1) s = 1;
            //return new Color2(s, 1.0, 1.0);
            //return new Color2((Math.Cos(phase) + 1) / 2, 1.0, 1.0);
            ++PlasmaPixelCount;
            //if( (PlasmaPixelCount&4095)==0)
            //    Trace.TraceInformation("Plasma count " + PlasmaPixelCount/512);
#endif

            return new Color(s, t, u, 1);
        }
        #endregion

        #region Blending

        /* Implement methods from
          1.  "Interpreting OpenEXR Deep Pixels," 
              Florian Kainz, Industrial Light & Magic, Updated November 13, 2013 
              http://www.openexr.com/documentation/InterpretingDeepPixels.pdf
          2.  "A Proposal for OpenEXR Color Management"
              Florian Kainz, Industrial Light & Magic
              http://www.openexr.com/documentation/OpenEXRColorManagement.pdf
          3.  "The Theory of OpenEXR Deep Samples," 
              Peter Hillman Weta Digital Ltd, November 10, 2013 
              http://www.openexr.com/documentation/TheoryDeepPixels.pdf
         
	        Discussion on alpha
            http://community.foundry.com/discuss/topic/104758/rendering-deep-images?mode=Post&postID=899117

            log1p https://www.johndcook.com/blog/csharp_log_one_plus_x/
		*/




        // this is when samples exactly overlap
        // algorithm to merge samples from document #1
        // modified to return new color sample
        static (double c3, double a3)  // Opacity and color of merged sample 
            AlphaBlend(
            double a1, double c1,  // Opacity and color of first sample      
            double a2, double c2   // Opacity and color of second sample      
        )

        {
            // todo - this is for depth samples in Deep Pixels, needs redone for normal alpha blending
            throw new NotImplementedException("");

            //    
            // This function merges two perfectly overlapping volume or point     
            // samples.  Given the color and opacity of two samples, it returns     
            // the color and opacity of the merged sample.     
            //    
            // The code below is written to avoid very large rounding errors when     
            // the opacity of one or both samples is very small:    
            //    
            // * The merged opacity must not be computed as 1 - (1-a1) * (1-a2).     
            //   If a1 and a2 are less than about half a floating-point epsilon,     
            //   the expressions (1-a1) and (1-a2) evaluate to 1.0 exactly, and the    
            //   merged opacity becomes 0.0.  The error is amplified later in the     
            //   calculation of the merged color.    
            //     
            //   Changing the calculation of the merged opacity to a1 + a2 - a1*a2    
            //   avoids the excessive rounding error.    
            //    

            // * For small x, the logarithm of 1+x is approximately equal to x,     
            //   but log(1+x) returns 0 because 1+x evaluates to 1.0 exactly.     
            //   This can lead to large errors in the calculation of the merged     
            //   color if a1 or a2 is very small.     
            //     
            //   The math library function log1p(x) returns the logarithm of    
            //   1+x, but without attempting to evaluate the expression 1+x     
            //   when x is very small.     
            // 


            /* compositing over operation: Co = C1 over C2

            if channel not premultiplied by alpha ai:
            Co = (C1 a1 + C2 a2 (1-a1))/(a1+a2(1-a1))

            if channel premultiplied by alpha: ci = ai Ci
            co = c1 + c2(1-a1)

            new alpha:
            ao = co/Co = a1 + a2 - a1*a2
            */


            a1 = Clamp01(a1);
            a2 = Clamp01(a2);

            var am = a1 + a2 - a1 * a2;

            if (a1 == 1 && a2 == 1) { return ((c1 + c2) / 2,am); }
            else if (a1 == 1) { return (c1,am); }
            else if (a2 == 1) { return (c2,am); }
            else
            {
                var MAX = double.MaxValue;
                var u1 = -LogOnePlusX(-a1);
                var v1 = (u1 < a1 * MAX) ? u1 / a1 : 1;
                
                var u2 = -LogOnePlusX(-a2);
                var v2 = (u2 < a2 * MAX) ? u2 / a2 : 1;

                var u = u1 + u2;
                var w = (u > 1 || am < u * MAX) ? am / u : 1;

                return ((c1 * v1 + c2 * v2) * w,am);
            }

        }

        /// <summary>
        /// Blend colors
        /// Colors should NOT be premultiplied by alpha
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <returns></returns>
        public static Color AlphaBlend(Color color1, Color color2)
        {
            var (r1, g1, b1, a1) = color1;
            var (r2, g2, b2, a2) = color2;


            // premultiply and blend
            var (r3, a3) = AlphaBlend(a1, r1*a1, a2, r2*a2);
            var (g3, _)  = AlphaBlend(a1, g1*a1, a2, g2*a2);
            var (b3, _)  = AlphaBlend(a1, b1*a1, a2, b2*a2);

            // un premultiply and return
            return new(r3/a3,g3/a3,b3/a3,a3);
        }



    #endregion

}
}