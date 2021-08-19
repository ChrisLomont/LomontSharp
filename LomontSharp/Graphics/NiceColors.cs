#define SCALE // scale hue stuff in correct manner
#define COMPLETE // new derivation
namespace Lomont.Graphics
{
#if false

    public class NColor
    {
        public int R, G, B;
    }


    // Links: 
    // 1. NASA site http://colorusage.arc.nasa.gov/
    // 2. Color schemes: http://www.brandigirlblog.com/2012/11/why-do-some-color-schemes-work-and-others-dont.html
    //
    // TODO
    //  1. Add linear blend - both RGB and HSL spaces to get N colors between two endpoints
    //      DONE somewhat - using uniform spacing in randomness, close enough?
    //  2. Implement seven color relationships – monochrome, analogous, complementary, triad, tetrad, neutral, and random
    //     ColorScheme type: use to select a first hue, then others based on first, small ranges?
    
    //  3. DONE: All "luminance" references to "lightness" which is correct term for HSL
    //  4. DONE: Change from delta to max/min formulation for all ranges
    //  5. DONE: make internals based on doubles? But then hard to port to PIC. So have double and int versions/paths?
    //     This is because the 0-360,0-100,0-100 integer interface results in about 1/5 as many colors as 24 bit color.
    //     Better yet - make internals work with 32 bit ints, fixed point? Then best of both worlds
    //     
    //  6. Final pass to clean code, unify interfaces
    //  7. Each interface should support native calls and select option based calls.
    //  8. Each interface should have RGB and HSL versions
    //  9. Each interface should have single and counted versions.
    // 10. Each interface should have random and linear versions.
    // 11. check TODOs
    // 12. DONE: use hue scaling on any output
    // 13. make monochrome work - same hue, vary sat/bright
    // 


    /// <summary>
    /// Class to provide some color utilities, such as pleasing color selection
    /// and colorspace conversions.
    /// </summary>
    public static class NiceColors
    {

#region support classes
        /// <summary>
        /// How to pick each color. 
        /// </summary>
        [Flags]
        public enum SelectFlags
        {
            /// <summary>
            /// No special changes. All parameters random within specified ranges
            /// </summary>
            None = 0,
            /// <summary>
            /// biases range to black
            /// </summary>
            Dark = 0x01,
            /// <summary>
            /// biases range to white
            /// </summary>
            Light = 0x02,
            /// <summary>
            /// Biases range to more color
            /// </summary>
            Bright = 0x04,
            /// <summary>
            /// Biases range to less color
            /// </summary>
            Flat = 0x08,
        }

        /// <summary>
        /// Options for selecting colors. Options are for HSL space.
        /// The main color is picked, and then the rest are picked
        /// accoding to the color scheme.
        /// </summary>
        public class SelectOptions
        {
            public SelectOptions()
            {
                HueMinimum = 0.0;
                HueMaximum = 1.0;

                SaturationMinimum = 0.8;
                SaturationMaximum = 1.0;
                LightnessMinimum = 0.45;
                LightnessMaximum = 0.55;
                HueScalePower = DefaultHueScalePower;
                SelectFlags = SelectFlags.None;
                Scheme = ColorSchemes.Random;
                Count = 1;
                UniformSpacing = true;
            }
            public double HueMinimum { get; set; }
            public double HueMaximum { get; set; }
            public double SaturationMinimum { get; set; }
            public double SaturationMaximum { get; set; }
            public double LightnessMinimum { get; set; }
            public double LightnessMaximum { get; set; } 
            public double HueScalePower { get; set; } 
            public SelectFlags SelectFlags { get; set; }
            public ColorSchemes Scheme { get; set; }

            public bool UniformSpacing { get; set; }

            /// <summary>
            /// Total count of colors desired
            /// </summary>
            public int Count { get; set;  }
        }

        /// <summary>
        /// How to pick colors
        /// </summary>
        public enum ColorSchemes
        {
            /// <summary>
            /// Same hue, vary saturation, lightness
            /// </summary>
            Monochrome,

            /// <summary>
            /// Hue varies 1/12, 2/2, or 3/12 of the way around, in 1/12 steps
            /// </summary>
            Analogous,

            /// <summary>
            /// Hue varies -2,0,2  12ths of the way around
            /// </summary>
            SplitAnalogous,

            /// <summary>
            /// Hues opposite, 6/12th away around circle
            /// </summary>
            Complementary,

            /// <summary>
            /// Hue varies +-5/12 of the way around (i.e., complementary, then off one)
            /// </summary>
            SplitComplementary,

            /// <summary>
            /// Three hues equally spaced around circle
            /// </summary>
            Triad,

            /// <summary>
            /// Four hues equally spaced around circle
            /// </summary>
            Tetrad,

            /// <summary>
            /// Four hues, at 0/12, 2/12, 6/12, and 8/12 around the circle.
            /// </summary>
            DoubleComplements,

            /// <summary>
            /// Black and white
            /// </summary>
            Neutral,

            /// <summary>
            /// One hue with black and white
            /// </summary>
            AccentedNeutral,

            /// <summary>
            /// Random colors
            /// </summary>
            Random
        }


#endregion

#region public interface

        /// <summary>
        /// Default power for hue scaling
        /// Makes the size of the various color variations more visually pleasing.
        /// </summary>
        public const double DefaultHueScalePower = 1.6;
        
        /// <summary>
        /// Get list of nice RGB colors
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public static List<NColor> GetNiceRgbs(SelectOptions options)
        {
            var colors = new List<NColor>();
            if (options.Count <= 0) 
                return colors;

            SelectOptions tempOption = null;
            if (options.UniformSpacing)
                tempOption = new SelectOptions(); // allocate once

            // save these
            var hMin = options.HueMinimum;
            var hMax = options.HueMaximum;
            var sMin = options.SaturationMinimum;
            var sMax = options.SaturationMaximum;
            var firstHue = Double.PositiveInfinity;

            var schemeOptions = GetScheme(options);
            for (var schemeIndex = 0; schemeIndex < schemeOptions.Count(); ++schemeIndex)
            {
                var item = schemeOptions[schemeIndex];

                // modify options as requested
                if (schemeIndex == 0)
                {
                    options.HueMinimum = hMin + item.hueShift;
                    options.HueMaximum = hMax + item.hueShift;
                }
                else
                { // smaller range based on first group

                    options.HueMinimum = firstHue - 1.0/12.0 + item.hueShift;
                    options.HueMaximum = firstHue + 1.0/12.0 + item.hueShift;

                }
                options.SaturationMinimum = sMin * item.satMult;
                options.SaturationMaximum = sMax * item.satMult;

                if (tempOption != null)
                {
                    // get first random color of scheme section
                    var tuple = GetNiceHslTuple(options, tempOption);
                    if (schemeIndex == 0)
                        firstHue = tuple.Item1;
                    colors.Add(HslTupleToRgb(tuple));

                    // make sure ranges non-decreasing for interpolation
                    if (tempOption.HueMaximum < tempOption.HueMinimum)
                        tempOption.HueMaximum += Math.Ceiling(tempOption.HueMinimum - tempOption.HueMaximum);

                    if (tempOption.SaturationMaximum < tempOption.SaturationMinimum)
                    {
                        var t = tempOption.SaturationMaximum;
                        tempOption.SaturationMaximum = tempOption.SaturationMinimum;
                        tempOption.SaturationMinimum = t;
                    }

                    if (tempOption.LightnessMaximum < tempOption.LightnessMinimum)
                    {
                        var t = tempOption.LightnessMaximum;
                        tempOption.LightnessMaximum = tempOption.LightnessMinimum;
                        tempOption.LightnessMinimum = t;
                    }


                    // picking one hue randomly, then others based on adding 1/phi then 
                    // mod 1 where phi is golden ratio makes nicely spread values in a range
                    // do for each component independently

                    // start at one since one already added
                    for (var i = 1; i < item.count; ++i)
                        colors.Add(HslTupleToRgb(PhiScaleColor(tuple, colors.Count, tempOption)));
                }
                else
                {
                    for (var i = 0; i < item.count; ++i)
                    {
                    var tuple = GetNiceHslTuple(options,null);
                    if (schemeIndex == 0 && i == 0)
                        firstHue = tuple.Item1;
                    colors.Add(HslTupleToRgb(tuple));
                    }
                }
            }

            // restore these
            options.HueMinimum = hMin;
            options.HueMaximum = hMax;
            options.SaturationMinimum = sMin;
            options.SaturationMaximum = sMax;
            return colors;
        }


        /// <summary>
        /// Get single nice RGB color
        /// This picks a random color based on a color profile.
        /// </summary>
        public static NColor GetNiceRgb(SelectOptions options)
        {
            return HslTupleToRgb(GetNiceHslTuple(options,null));
        }

        /// <summary>
        /// Get single nice HSL color
        /// This picks a random color based on a color profile.
        /// </summary>
        public static NColor GetNiceHsl(SelectOptions options)
        {
            return GetNiceHsl(
                options.HueMinimum, options.HueMaximum,
                options.SaturationMinimum, options.SaturationMaximum,
                options.LightnessMinimum, options.LightnessMaximum,
                options.HueScalePower,
                options.SelectFlags
                );

        }

        /// <summary>
        /// Get single nice HSL color
        /// This picks a random color based on a color profile.
        /// </summary>
        public static NColor GetNiceHsl(
            double hueMinimum = 0.0, double hueMaximum = 1.0,
            double saturationMinimum = 0.0, double saturationMaximum = 1.0,
            double lightnessMinimum = 0.0, double lightnessMaximum = 1.0,
            double hueScalePower = DefaultHueScalePower,
            SelectFlags selectFlags = SelectFlags.None
            )
        {

            var triple = GetNiceHslTuple(
                hueMinimum, hueMaximum, 
                saturationMinimum, saturationMaximum,
                lightnessMinimum, lightnessMaximum,
                hueScalePower,
                selectFlags, 
                null
                );
            return new NColor
            {
                R = (int) (255.0*triple.Item1),
                G = (int) (255.0*triple.Item2),
                B = (int) (255.0*triple.Item3)
            };
        }

        /// <summary>
        /// Set an external random source. Otherwise uses internal
        /// </summary>
        public static Random RandomSource { get { return randomBacking;  } set { randomBacking = value; } }
        private static Random randomBacking = new Random();

        /// <summary>
        /// Given a hue in 0-1, scales internal values such that
        /// colors are visually more evenly represented.
        /// Power 1 is default. 1.6 seems to be a good values
        /// </summary>
        /// <param name="hue"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static double ScaleHue(double hue, double power = DefaultHueScalePower)
        {
            // the idea is to keep the 6 base colors at 0,1/6,2/6,etc fixed, and make 
            // the odd value more represented, since they are not well represented now.
            // For example, in a hue cycle, yellow is much shorter than red

            // make sure h in [0,1). Also returns value back in same phase as it came in
            var floor = Math.Floor(hue);
            hue -= floor;
#if false

            var hInt = (int)Math.Floor(6 * hue);
            var hFrac = hue * 6 - hInt;
            var hOdd = (hInt & 1) != 0;
            var hNew = hInt + (hOdd ? Math.Pow(hFrac, power) : Math.Pow(hFrac, 1.0 / power));
            return hNew / 6 + floor;
#else
            // this function seems better - matches values AND first derivative at each end
            power = 1.0 / power;
            // scale to [0,6)
            var hInt = (int)Math.Floor(6 * hue);
            var hFrac = hue * 6 - hInt;

            var hNew = (hInt & 1) == 0
                ? Math.Pow(hFrac, power)
                : 1 - Math.Pow(2 - (hFrac + 1), power);
            return (hNew + hInt) / 6 + floor;
#endif
        }

        /// <summary>
        /// HSL in 0-1, RGB in 0-255
        /// </summary>
        /// <param name="h">Value in 0-1</param>
        /// <param name="s">Value in 0-1</param>
        /// <param name="l">Value in 0-1</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HslToRgb(double h, double s, double l, out int r, out int g, out int b)
        {
            double rd, gd, bd;
            HslToRgb(h, s, l, out rd, out gd, out bd);
            r = (int)(rd * 255);
            g = (int)(gd * 255);
            b = (int)(bd * 255);
        }

        /// <summary>
        /// Convert Hue,Saturation,Luminance (HSL) to Red,Green,Blue (RGB) in 0,1.
        /// If Hue were in [0,360): 
        ///    red    = 0
        ///    violet = 60
        ///    blue   = 120
        ///    cyan   = 180
        ///    green  = 240
        ///    yellow = 300
        /// </summary>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HslToRgb(double h, double s, double l, out double r, out double g, out double b)
        {
            // make sure h in [0,1)
            h -= Math.Floor(h);


            if (Math.Abs(s) < 0.00001)
            {
                r = g = b = l; // achromatic
            }
            else
            {

                var c = (1 - Math.Abs(2 * l - 1)) * s; // chroma
                var hp = 6 * h;
                var ab = (hp / 2 - Math.Floor(hp / 2)) * 2; // hp mod 2
                var x = c * (1 - Math.Abs(ab - 1));
                r = g = b = 0;
                if (hp < 1)
                {
                    r = c;
                    g = x;
                }
                else if (hp < 2)
                {
                    r = x;
                    g = c;
                }
                else if (hp < 3)
                {
                    g = c;
                    b = x;
                }
                else if (hp < 4)
                {
                    g = x;
                    b = c;
                }
                else if (hp < 5)
                {
                    b = c;
                    r = x;
                }
                else if (hp < 6)
                {
                    b = x;
                    r = c;
                }
                var m = l - c * 0.5;
                r += m;
                g += m;
                b += m;
            }
        }



#if COMPLETE
        /// <summary>
        /// General RGB to HSV/HSL converter, integer based
        /// Allows arbitrary bit sizes for each color type (RGB/HSLv),
        /// with total bit size (bit length red + bitlength hue) at most 27.
        /// Integer sizes made explicit for clarity.
        /// TODO - walk all ops and compute max bit restrictions
        /// TODO - figure out why these improve accuracy compared to http://code.google.com/p/streumix-frei0r-goodies/wiki/Integer_based_RGB_HSV_conversion
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="sHsl"></param>
        /// <param name="sHsv"></param>
        /// <param name="l"></param>
        /// <param name="v"></param>
        /// <param name="rgbBits">bits per RGB channel.</param>
        /// <param name="hslvBits">bits per HSLV channel.</param>
        public static void RgbToHslv(Int32 r, Int32 g, Int32 b, out Int32 h, out Int32 sHsl, out Int32 sHsv, out Int32 l, out Int32 v, int rgbBits, int hslvBits)
        {
            checked // throws exception on arithmetic error or overflow
            {
                Trace.Assert(0 <= rgbBits && 0 <= hslvBits && rgbBits + hslvBits <= 27);

                Int32 mr = (1 << rgbBits);
                Int32 mh = (1 << hslvBits);

                Trace.Assert(0 <= r && r < mr);
                Trace.Assert(0 <= g && g < mr);
                Trace.Assert(0 <= b && b < mr);

                Int32 M, m; // max, min of color components
                if (r > g)
                {
                    M = Math.Max(r, b);
                    m = Math.Min(g, b);
                }
                else
                {
                    M = Math.Max(g, b);
                    m = Math.Min(r, b);
                }
                Int32 c = M - m;        // chroma in [0,m1)

                Trace.Assert(0 <= c && c < mr);

                h = 0; // default case if c == 0

                if (c != 0)
                {
                    Int32 t1 = 0;
                    if (M == r)
                    {   // h = (G-B)/C mod 6 in [0,1] union [5,6)
                        // detect those cases that would round to a negative integer
                        if (3*c + mh*g >= mh*b)
                            t1 = g - b;
                        else
                            t1 = 6*c + g - b;
                    }
                    else if (M == g)
                    {   // h = (B-R)/C + 2 in [1,3]
                        t1 = 2*c + b - r;
                    }
                    else if (M == b)
                    { // h = (R-G)/C + 4 in [4,5]
                        t1 = 4*c + r - g;
                    }
                    h = RoundDiv32(mh * t1, 6 * c);
                }
                Trace.Assert(0 <= h && h < mh);

                v = RoundDiv32(M*(mh-1),mr-1); // in [0,mh)
                l = RoundDiv32((M + m)*(mh-1),2*(mr-1)); // in [0,m2)

                Trace.Assert(0 <= v && v < mh);
                Trace.Assert(0 <= l && l < mh);

                sHsv = 0; // default if v = 0
                if (v != 0)
                {
                    // sHsv = C/V with C in [0,m1] and V in [0,m2], answer in [0,m2]
                    // sHsv = m2 * (c/m1)/(v/m2) = m2*m2*c/(v*m1)
                    // more accurate to compute with C/M, both in [0,m1]
                    // thus sHsv = m2*C/M
                    sHsv = RoundDiv32(c*(mh-1), M);
                }

                sHsl = 0; // default if l = 0 or l = m2
                if (l != 0 && l != mh-1)
                {
                    Int32 t2;
                    if (M + m >= mr - 1)
                        t2 = 2*(mr - 1) - (M + m);
                    else
                        t2 = M + m;
                    sHsl = RoundDiv32((mh - 1)*c, t2);
                }
                
                Trace.Assert(0 <= sHsl && sHsl < mh);
                Trace.Assert(0 <= sHsv && sHsv < mh);
            }
        }

        /// <summary>
        /// General HSL to RGB converter, integer based
        /// Allows arbitrary bit sizes for each color type (RGB/HSLv),
        /// with total bit size (bit length red + bitlength hue) at most 27.
        /// Integer sizes made explicit for clarity.
        /// TODO - walk all ops and compute max bit restrictions
        /// TODO - test overflows?
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="v"></param>
        /// <param name="rgbBits">bits  1-16 per RGB channel</param>
        /// <param name="hslvBits">bits  1-16 per HSLV channel</param>
        /// <param name="useValue"></param>
        public static void HslvToRgb(Int32 h, Int32 s, Int32 l, Int32 v, out Int32 r, out Int32 g, out Int32 b, int rgbBits, int hslvBits, bool useValue)
        {
            checked // throws exception on arithmetic error or overflow
            {
                Trace.Assert(0 <= rgbBits && 0 <= hslvBits && rgbBits + hslvBits <= 27);

                Int32 mr = (1 << rgbBits);
                Int32 mh = (1 << hslvBits);

                Trace.Assert(0 <= h && h < mh);
                Trace.Assert(0 <= s && s < mh);
                Trace.Assert(0 <= l && l < mh);

                Int32 cp,mp;
                if (useValue)
                {
                    cp = v*s;
                    mp = (v*(mh-1) - cp)*2;
                    cp *= 2;
                }
                else
                {
                    Int32 t1;
                    if (2*l >= mh - 1)
                        t1 = 2*(mh - 1 - l); // mh-1 - (2l-mh-1)
                    else
                        t1 = 2*l; // mh-1 - (mh-1 - 2l)
                    cp = t1*s;
                    mp = 2*l*(mh-1) - cp;
                    cp *= 2;
                }
                Int32 hTerm = (6*h)%(2*mh); // 6h mod 2
                if (hTerm > mh)
                    hTerm = 2*mh-hTerm; // mh - [1,mh-1]

                Int64 num1,num2, den;
                //Int32 xm = RoundDiv32(
                //    (cp * f + mh * mp)*(mr-1),
                //    2*(mh-1)*mh*(mh-1)
                //    );

                num1 = cp;
                num1 *= hTerm;
                num2 = mh;
                num2 *= mp;
                num1 = (num1 + num2)*(mr - 1);
                den = 2*mh;
                den *= (mh - 1)*(mh - 1);
                Int32 xm = (Int32)((2*num1 + den)/(2*den));

                // todo - this approximation is not sufficient
                //Int32 a1 = RoundDiv32(cp, mh - 1);
                //Int32 a2 = RoundDiv32(mp, mh - 1);
                //xm = RoundDiv32(a1*hTerm, mh);
                //xm = RoundDiv32((xm+a2)*(mr-1), 2*(mh - 1));

                //Int32 cm = RoundDiv32(
                //    (cp + mp)*(mr - 1), 
                //    2*(mh - 1)*(mh - 1)
                //    );
                // compute as cp+mp modDiv mh-1 * mr-1 modDiv 2*(mh-1) round 2*(mh-1)*(mh-1)
                num1 = cp + mp;
                num1 *= mr - 1;
                den = 2*(mh - 1)*(mh - 1);
                Int32 cm = (Int32)((2*num1+den)/(2*den));

                // todo - this approximation works fine
                //Int32 b1 = RoundDiv32(cp+mp, mh - 1);
                //cm = RoundDiv32(b1*(mr-1), 2*(mh - 1));

                //Int32 m = RoundDiv32(
                //    mp*(mr - 1), 
                //    2*(mh - 1)*(mh - 1)
                //    );
                // compute as mp modDiv mh-1 * mr-1 modDiv 2*(mh-1) round 2*(mh-1)*(mh-1)
                num1 = mp;
                num1 *= mr - 1;
                den = 2*(mh - 1)*(mh - 1);
                Int32 m = (Int32)((2*num1+den)/(2*den));

                // todo - this approximation works fine
                //Int32 c1 = RoundDiv32(mp, mh - 1);
                //m = RoundDiv32(c1*(mr - 1), 2*(mh - 1));

                r = g = b = m;
                if (cp != 0)
                {
                    switch (6 * h / mh)
                    {
                        case 0: r = cm; g = xm; b = m; break;
                        case 1: r = xm; g = cm; b = m; break;
                        case 2: r = m; g = cm; b = xm; break;
                        case 3: r = m; g = xm; b = cm; break;
                        case 4: g = m; b = cm; r = xm; break;
                        default: g = m; b = xm; r = cm; break;
                    }
                }

                Trace.Assert(0 <= r && r < mr);
                Trace.Assert(0 <= g && g < mr);
                Trace.Assert(0 <= b && b < mr);

            }
        }

#else
        /// <summary>
        /// General RGB to HSV/HSL converter, integer based
        /// Allows arbitrary bit sizes for each color type (RGB/HSLv),
        /// with total bit size (bit length red + bitlength hue) at most 27.
        /// Integer sizes made explicit for clarity.
        /// TODO - walk all ops and compute max bit restrictions
        /// TODO - figure out why these improve accuracy compared to http://code.google.com/p/streumix-frei0r-goodies/wiki/Integer_based_RGB_HSV_conversion
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="sHsl"></param>
        /// <param name="sHsv"></param>
        /// <param name="l"></param>
        /// <param name="v"></param>
        /// <param name="rgbBits">bits per RGB channel.</param>
        /// <param name="hslvBits">bits per HSLV channel.</param>
        public static void RgbToHslv(Int32 r, Int32 g, Int32 b, out Int32 h, out Int32 sHsl, out Int32 sHsv, out Int32 l, out Int32 v, int rgbBits, int hslvBits)
        {
            checked // throws exception on arithmetic error or overflow
            {
                Trace.Assert(0 <= rgbBits && 0 <= hslvBits && rgbBits + hslvBits <= 27);

                Int32 m1 = (1 << rgbBits) - 1;
                Int32 m2 = (1 << hslvBits) - 1;
                Int32 scale = (1 << hslvBits); 

                Trace.Assert(0 <= r && r <= m1);
                Trace.Assert(0 <= g && g <= m1);
                Trace.Assert(0 <= b && b <= m1);

                Int32 M, m; // max, min of color components
                if (r > g)
                {
                    M = Math.Max(r, b);
                    m = Math.Min(g, b);
                }
                else
                {
                    M = Math.Max(g, b);
                    m = Math.Min(r, b);
                }
                Int32 c = M - m;        // chroma in [0,m1]

                Trace.Assert(0 <= c && c <= m1);

                h = 0; // default case if c == 0
#if SCALE
                Int32 cut = scale; // try also with m2
                if (c != 0)
                {
                    if (M == r)
                    {   // h = (G-B)/C mod 6 in [0,1] union [5,6)
#if false
                        if (g<b)//2*m2*b-6*c > 2*m2*g) // detect those cases that would round to a negative number
                        { // [5,6) case
                            // Note - if we only compared to g<b, and rounded, 
                            // in this case we need to round down to avoid the top value, but this makes other errors?
                            // which would be hit, for example, m2=255,r=c=85,g=0,b=1,scale=256, then div gives 255.5
                            // h= (g-b)/c + 6 in [5,6), want in [5m2/6,m2]
                            //  = m2(g-b)/(c) +  m2 or + (m2+1)?
                            // todo - analyze better - likely can subtract just 1, or compute
                            // h = (m2*(g - b)+6*c*scale)/(6*c);
                            h = RoundDiv32(m2 * (g - b) + 6 * c * m2, 6 * c); // todo - why works with m2 instead of scale?
                            // todo - using m2 instead of scale makes roundtrip work at 8/11 bits instead of 8/12!?
                            // compute in order to avoid negative numbers in case we want to use unsigned math everywhere
                            //h = RoundDiv32(m2 * g + 6 * c * scale - m2 * b, 6 * c);
                        }
#else
                        // detect those cases that would round to a negative integer
                        // more accurate, but both seem to work:
                        if (cut*b > cut*g + 3*c)// g<b works for some bit depth combinations, but fails for some.
                        { // [5,6) case
                            h = RoundDiv32(cut * (g - b) + 6 * c * cut, 6 * c);
                        }
#endif
                        else // [0,1] case
                            h = RoundDiv32(cut * (g - b), 6 * c);

                        //h = RoundDiv32(m2 * (g - b), 6 * c); // doing it this was needs 8/13 bits!
                        //if (h < 0) h += scale;

                    }
                    else if (M == g)
                    {   // h = (B-R)/C + 2 in [1,3]
                        h = RoundDiv32(cut * (b + c * 2 - r), 6 * c);
                    }
                    else if (M == b)
                    { // h = (R-G)/C + 4 in [4,5]
                        h = RoundDiv32(cut * (r + c * 4 - g), 6 * c);
                    }
                }
#else
                if (c != 0)
                {
                    if (M == r)
                    {   // h = (G-B)/C mod 6 in [0,1] union [5,6)
#if false
                        if (g<b)//2*m2*b-6*c > 2*m2*g) // detect those cases that would round to a negative number
                        { // [5,6) case
                            // Note - if we only compared to g<b, and rounded, 
                            // in this case we need to round down to avoid the top value, but this makes other errors?
                            // which would be hit, for example, m2=255,r=c=85,g=0,b=1,scale=256, then div gives 255.5
                            // h= (g-b)/c + 6 in [5,6), want in [5m2/6,m2]
                            //  = m2(g-b)/(c) +  m2 or + (m2+1)?
                            // todo - analyze better - likely can subtract just 1, or compute
                            // h = (m2*(g - b)+6*c*scale)/(6*c);
                            h = RoundDiv32(m2 * (g - b) + 6 * c * m2, 6 * c); // todo - why works with m2 instead of scale?
                            // todo - using m2 instead of scale makes roundtrip work at 8/11 bits instead of 8/12!?
                            // compute in order to avoid negative numbers in case we want to use unsigned math everywhere
                            //h = RoundDiv32(m2 * g + 6 * c * scale - m2 * b, 6 * c);
                        }
#else
                        // detect those cases that would round to a negative integer
                        // more accurate, but both seem to work:
                        //if (2*m2*b-6*c > 2*m2*g) 
                        if (g < b)
                        { // [5,6) case
                            h = RoundDiv32(m2 * (g - b) + 6 * c * m2, 6 * c);
                        }
#endif
                        else // [0,1] case
                            h = RoundDiv32(m2*(g - b),6*c);

                        //h = RoundDiv32(m2 * (g - b), 6 * c); // doing it this was needs 8/13 bits!
                        //if (h < 0) h += scale;

                    }
                    else if (M == g)
                    {   // h = (B-R)/C + 2 in [1,3]
                        h = RoundDiv32(m2 * (b + c*2 - r),6 * c);
                    }
                    else if (M == b)
                    { // h = (R-G)/C + 4 in [4,5]
                        h = RoundDiv32(m2 * (r + c*4 - g), 6 * c);
                    }
                }
#endif
                Trace.Assert(0 <= h && h <= m2);

                v = RoundDiv32(M*m2,m1); // in [0,m2]
                l = RoundDiv32((M + m)*m2,2*m1); // in [0,m2]

                Trace.Assert(0 <= v && v <= m2);
                Trace.Assert(0 <= l && l <= m2);

                sHsv = 0; // default if v = 0
                if (v != 0)
                {
                    // sHsv = C/V with C in [0,m1] and V in [0,m2], answer in [0,m2]
                    // sHsv = m2 * (c/m1)/(v/m2) = m2*m2*c/(v*m1)
                    // more accurate to compute with C/M, both in [0,m1]
                    // thus sHsv = m2*C/M
                    sHsv = RoundDiv32(c*m2, M);
                }

                sHsl = 0; // default if l = 0 or l = m2
                if (l != 0 && l != m2)
                {
                    Int32 den = M + m - m1;
                    if (den < 0) den = -den;
                    sHsl = RoundDiv32(c*m2 ,m1 - den);
                }
                
                Trace.Assert(0 <= sHsl && sHsl <= m2);
                Trace.Assert(0 <= sHsv && sHsv <= m2);
            }
        }

        /// <summary>
        /// General HSL to RGB converter, integer based
        /// Allows arbitrary bit sizes for each color type (RGB/HSLv),
        /// with total bit size (bit length red + bitlength hue) at most 27.
        /// Integer sizes made explicit for clarity.
        /// TODO - walk all ops and compute max bit restrictions
        /// TODO - test overflows?
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="v"></param>
        /// <param name="rgbBits">bits  1-16 per RGB channel</param>
        /// <param name="hslvBits">bits  1-16 per HSLV channel</param>
        /// <param name="useValue"></param>
        public static void HslvToRgb(Int32 h, Int32 s, Int32 l, Int32 v, out Int32 r, out Int32 g, out Int32 b, int rgbBits, int hslvBits, bool useValue)
        {
            checked // throws exception on arithmetic error or overflow
            {
                Trace.Assert(0 <= rgbBits && 0 <= hslvBits && rgbBits + hslvBits <= 27);

                Int32 m1 = (1 << rgbBits) - 1;
                Int32 m2 = (1 << hslvBits) - 1;

                Trace.Assert(0 <= h && h <= m2);
                Trace.Assert(0 <= s && s <= m2);
                Trace.Assert(0 <= l && l <= m2);

                Int32 c = 0;
                Int32 num = 0;
                if (useValue)
                {
                    // c = s*v, want result in [0,m1]
                    // thus c=v*s*m1/(m2*m2) rounded
                    num = v;
                }
                else
                {
                    // c = s(1-|2L-1|), want result in [0,m1]
                    // thus c=(m2-|2L-m2|) *s*m1 / (m2*m2) rounded
                    num = 2 * l - m2;
                    if (num < 0) num = -num;
                    num = m2 - num;
                    Trace.Assert(0 <= num & num <= m2);
                }
                //c = Chroma64(num, s, m1, m2);
                 c = Chroma32(num, s, m1, m2); // 32-bit version, slower? Todo - timing

                Trace.Assert(0 <= c && c <= m1);

                // six hue cutoffs, each rounded properly
                // ai = (m2*i + 5)/6 for i = 0,1,2,3,4,5,6
                // then comparisons with ai <= h < a(i+1) get replaced with inline math to avoid rounding error

#if SCALE
                Int32 hp = 6*h;
                Int32 cut = m2 + 1; // was m2
                if (hp < cut)
                    hp = cut - 1 - hp; // (cut,0]
                else if (hp < 2*cut)
                    hp = hp - cut;     // [1,cut)
                else if (hp < 3*cut)
                    hp = 3*cut -1 - hp;
                else if (hp < 4*cut)
                    hp = hp - 3*cut;
                else if (hp < 5*cut)
                    hp = 5*cut -1 - hp;
                else
                    hp = hp - 5*cut;
                // hp in [0,m2]
#else
                Int32 hp = 6*h;
                if (hp < m2)
                    hp = m2 - hp;
                else if (hp < 2*m2)
                    hp = hp - m2;
                else if (hp < 3*m2)
                    hp = 3*m2 - hp;
                else if (hp < 4*m2)
                    hp = hp - 3*m2;
                else if (hp < 5*m2)
                    hp = 5*m2 - hp;
                else
                    hp = hp - 5*m2;
                // hp in [0,m2]
#endif

                Trace.Assert(0 <= hp && hp <= m2);

                // x = c(1-|hp mod 2 - 1|) in [0,m1]
                Int32 x = RoundDiv32(c*(m2 - hp),m2); // in [0,m1]
                Trace.Assert(0 <= x && x <= m1);

                // min m in [0,m1] computed via lightness or value
                Int32 m = 0;
                if (useValue)
                { // use value V
                    // m = V-C, V in [0,m2], C in [0,m1], want m in [0,m1], so
                    // m = m1*V/m2 - C = (m1*v - m2*c)/m2 rounded
                    m = RoundDiv32(m1 * v - m2 * c, m2);

                }
                else
                { // use lightness l
                    // m = l-c/2
                    m = RoundDiv32(l * m1 * 2 - c * m2, 2 * m2);
                }

                // shift RGB back from minimum by shifting x and c
                c += m;
                x += m;

                // condition like h < ai is same as 6*h < i*m2+5 is same as 6*h < i*m2
                // then equivalent to switch
                switch (6*h/m2)
                {
                    case 0 : r = c; g = x; b = m; break;
                    case 1 : r = x; g = c; b = m; break;
                    case 2 : r = m; g = c; b = x; break;
                    case 3 : r = m; g = x; b = c; break;
                    case 4 : g = m; b = c; r = x; break;
                    default: g = m; b = x; r = c; break;
                }

                Trace.Assert(0 <= r && r <= m1);
                Trace.Assert(0 <= g && g <= m1);
                Trace.Assert(0 <= b && b <= m1);

            }
        }
#endif
        /// <summary>
        /// Compute a/b, rounded to the nearest integer, as 32 bit integers
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        static Int32 RoundDiv32(Int32 a, Int32 b)
        {
            checked
            {
                // Round(a/b) = Floor( (a div b) + 1/2) = Floor( (2a+b) div 2b) = (2a+b) div (2b)
                return (2 * a + b) / (2 * b);
            }
        }

        /// <summary>
        /// Compute (a*b*c)/(d*d) as a 32-bit answer, allowing 64-bit integers internally, 
        /// where a,b,c,d are at most 15-bit integers, rounded to the nearest integer answer.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static Int32 Chroma64(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            checked
            {
                Int64 t = a*b;
                t *= c;
                // Round(a/b) = Floor( (a div b) + 1/2) = Floor( (2a+b) div 2b) = (2a+b) div (2b)
                return (Int32)((2 * t + d*d) / (2 * d*d));
            }
        }


        /// <summary>
        /// Compute (a*b*c)/(d*d) as a 32-bit answer, allowing 64-bit integers internally, 
        /// where a,b,c,d are at most 15-bit integers, rounded to the nearest integer answer.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private static Int32 Chroma32Kenyan(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            checked
            {
                Int32 t1 = a*b;
                Int32 t2 = (t1 & 65535)*c; // lower part
                Int32 t3 = (t1 >> 16)*c;   // higher part
                t3 += (t2 >> 16); // add higher into t3, allowing carry to propogate up
                t2 = (t2 & 65535) | ((t3 & 65535) << 16); // mask back into lower part
                t3 >>= 16; // low 16 bits of t3, followed by t2, contain a*b*c
                
                Int64 check = a*b;
                check *= c;
                check -= (long) (t3) << 48;
                check -= t2;
                Trace.Assert(check==0); // sanity check

                Int32 div = d*d;

                // kenyan divide: http://www.piclist.com/techref/method/math/muldiv.htm
                // 1. double divisor until just less than dividend. Keep count of how many
                // 2. subtract until will go negative, each subtraction store a 1, when about to go too small, store a 0
                // 3. halve this divisior, repeat until divisor back to start
                // 4. sequence of 0 and 1 is divisor
                
                return 0; // todo - probabaly much slower....
            }
        }


        /// <summary>
        /// Compute (a*b*c)/(d*d) as a 32-bit answer, allowing 32-bit integers internally, 
        /// where a,b,c,d are at most 15-bit integers, rounded to the nearest integer answer.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        static Int32 Chroma32(Int32 a, Int32 b, Int32 c, Int32 d)
        {
            // perform in the order (((a*b)/d)*c)/d) to avoid overflows
            checked
            {
                // todo - is "kenyan" multiplication and division faster?
                // see http://forums.parallax.com/showthread.php/139711-Fast-Faster-Fastest-Code-Ultrafast-32-bit-integer-division

                Int32 q1, r1, q2, r2, q3, r3, q4, r4; // quotients and remainders
                DivRem(a*b, d, out q1, out r1);
                DivRem(q1*c, d, out q2, out r2);
                DivRem(r1*c, d*d, out q3, out r3);
                DivRem(d*r2 + r3, d*d, out q4, out r4);

                if (2*r4 >= d*d)
                    return q2 + q3 + q4 + 1;
                return q2 + q3 + q4;
            }
        }

        /// <summary>
        /// Compute the quotient and remainder of num/den
        /// </summary>
        /// <param name="num"></param>
        /// <param name="den"></param>
        /// <param name="quotient"></param>
        /// <param name="remainder"></param>
        static void DivRem(Int32 num, Int32 den, out Int32 quotient, out Int32 remainder)
        {
            checked
            {
                quotient = num/den;
                remainder = num - quotient*den;
            }
        }



#endregion

#region private implementation

        class Triple : Tuple<double, double, double>
        {
            internal Triple(double a, double b, double c) : base(a,b,c)
            {
                
            }
            
        }

        private static Triple GetNiceHslTuple(SelectOptions options, SelectOptions computedRange)
        {
            return GetNiceHslTuple(
                options.HueMinimum, options.HueMaximum,
                options.SaturationMinimum, options.SaturationMaximum,
                options.LightnessMinimum, options.LightnessMaximum,
                options.HueScalePower,
                options.SelectFlags,
                computedRange
                );
        }


        // this is the workhorse for color selection
        // returns a h,s,l tuple.
        private static Triple GetNiceHslTuple(
            double hueMinimum, double hueMaximum,
            double saturationMinimum, double saturationMaximum,
            double lightnessMinimum, double lightnessMaximum,
            double hueScalePower,
            SelectFlags selectFlags,
            SelectOptions computedRanges
            )
        {
            // Get random number in range, inclusive
            Func<double, double, double> randomRange = (min, max) => min + RandomSource.NextDouble()*(max - min);

            // pick a hue, decided by hue and +- range.
            var minFix = Math.Ceiling(Math.Max(hueMinimum-hueMaximum,0)); // if hueMin > hueMax
            var hue = randomRange(hueMinimum-minFix, hueMaximum);

            // and scale hue and range
            hue = ScaleHue(hue, hueScalePower);
            hueMinimum = ScaleHue(hueMinimum, hueScalePower);
            hueMaximum = ScaleHue(hueMaximum, hueScalePower);
           

            if (computedRanges != null)
            {
                computedRanges.SelectFlags = selectFlags;
                computedRanges.HueMinimum = hueMinimum;
                computedRanges.HueMaximum = hueMaximum;
            }


            // put back in range
            hue += Math.Floor(Math.Max(hueMinimum-hue,0)); // if hue < min
            hue -= Math.Floor(Math.Max(hue - hueMaximum, 0)); // if max < hue

            // pick a saturation
            var satMin = Math.Max(0, saturationMinimum);
            var satMax = Math.Min(saturationMaximum, 1.0);
            if (selectFlags.HasFlag(SelectFlags.Bright))
            {
                satMin = (satMin + satMax) / 2;
                satMax = Math.Min(1.0, satMax + 0.2);
            }
            else if (selectFlags.HasFlag(SelectFlags.Flat))
            {
                satMin = Math.Max(0, satMin - 0.2);
                satMax = (satMin + satMax) / 2;
            }

            var saturation = randomRange(satMin, satMax);
            if (computedRanges != null)
            {
                computedRanges.SaturationMinimum = satMin;
                computedRanges.SaturationMaximum = satMax;
            }



            // pick a lightness
            var lightMin = Math.Max(0, lightnessMinimum);
            var lightMax = Math.Min(lightnessMaximum,1.0);

            if (selectFlags.HasFlag(SelectFlags.Light))
            {
                lightMin = (lightMin + lightMax) / 2;
                lightMax = Math.Min(1.0, lightMax + 0.2);
            }
            else if (selectFlags.HasFlag(SelectFlags.Dark))
            {
                lightMin = Math.Max(0, lightMin - 0.2);
                lightMax = (lightMin + lightMax) / 2;
            }
            var lightness = randomRange(lightMin, lightMax);
            if (computedRanges != null)
            {
                computedRanges.LightnessMinimum = lightMin;
                computedRanges.LightnessMaximum = lightMax;
            }

            return new Triple(hue, saturation, lightness); 
        }

        private static readonly double phiReciprocal = 2.0 / (1 + Math.Sqrt(5));

            
        // given min, max, start value, and multiplier, compute next value as...
        private static double PhiScaleScalar(double min, double max, double value, int m)
        {   
            var delta = max - min;
            double t;
            // scaling is bad if bad if delta is too small
            if (delta > 0.0001)
            {
                t = value + m*phiReciprocal*delta - min;
                // need to reduce t mod delta - 
                t -= delta*Math.Floor(t/delta);
            }
            else
            {
                t = value - min;
            }
            Debug.Assert(0 <= t && t <= delta);
            return t + min;
        }

        private static Triple PhiScaleColor(Triple color, int multiplier, SelectOptions computedRange)
        {

            //aliases to make code smaller to read
            var rr = computedRange;
            var cc = color;
            return new Triple
            (
                // hue
                PhiScaleScalar(rr.HueMinimum, rr.HueMaximum, cc.Item1, multiplier), 
                // saturation
                PhiScaleScalar(rr.SaturationMinimum, rr.SaturationMaximum, cc.Item2, multiplier),
                // lightness
                PhiScaleScalar(rr.LightnessMinimum, rr.LightnessMaximum, cc.Item3, multiplier)  
            );
        }

        private static NColor HslTupleToRgb(Triple triple)
        {
            int r, g, b;
            HslToRgb(triple.Item1, triple.Item2, triple.Item3, out r, out g, out b);
            return new NColor { R = r, G = g, B = b };
        }

        class SchemeEntry
        {
            public SchemeEntry(double hueShift, double satMult = 1, double hueMult = 1)
            {
                this.hueShift = hueShift;
                this.satMult = satMult;
                this.hueMult = hueMult;
            }
            // shift hue range by this
            public double hueShift;

            // scale hue range by this
            public double hueMult;

            // mult each sat by this
            public double satMult;

            // get this many of this style of color
            public int count;
        }

        private static List<SchemeEntry> GetScheme(SelectOptions options)
        {


            var list = new List<SchemeEntry>();
            switch (options.Scheme)
            {
                case ColorSchemes.Random:
                    // Random colors
                    list.Add(new SchemeEntry(0));
                    break;
                case ColorSchemes.Complementary:
                    // Hues opposite, 6/12th away around circle
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(0.5));
                    break;
                case ColorSchemes.Triad:
                    // Three hues equally spaced around circle
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(1.0 / 3.0));
                    list.Add(new SchemeEntry(-1.0 / 3.0));
                    break;
                case ColorSchemes.Tetrad:
                    // Four hues equally spaced around circle
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(0.25));
                    list.Add(new SchemeEntry(0.5));
                    list.Add(new SchemeEntry(0.75));
                    break;
                case ColorSchemes.AccentedNeutral:
                    // One hue with black and white
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(0, 0));
                    break;
                case ColorSchemes.Analogous:
                    // Hue varies 1/12, 2/2, or 3/12 of the way around, in 1/12 steps
                    var steps = RandomSource.Next(4) + 1;
                    for (var i = 0; i < steps; ++i)
                        list.Add(new SchemeEntry(i / 12.0));
                    break;
                case ColorSchemes.Neutral:
                    // Black and white
                    list.Add(new SchemeEntry(0, 0));
                    break;
                case ColorSchemes.Monochrome:
                    // Same hue, vary saturation, lightness
                    list.Add(new SchemeEntry(0, 1, 0));
                    break;
                case ColorSchemes.SplitAnalogous:
                    // Hue varies -2,0,2  12ths of the way around
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(1.0 / 6.0));
                    list.Add(new SchemeEntry(-1.0 / 6.0));
                    break;
                case ColorSchemes.SplitComplementary:
                    // Hue varies +-5/12 of the way around (i.e., complementary, then off one)
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(5.0 / 12.0));
                    list.Add(new SchemeEntry(-5.0 / 12.0));
                    break;
                case ColorSchemes.DoubleComplements:
                    // Four hues, at 0/12, 2/12, 6/12, and 8/12 around the circle.
                    list.Add(new SchemeEntry(0));
                    list.Add(new SchemeEntry(1.0 / 6.0));
                    list.Add(new SchemeEntry(0.5));
                    list.Add(new SchemeEntry(2.0 / 3.0));
                    break;
                default:
                    throw new NotImplementedException("Unsupported color scheme " + options.Scheme);
            }


            // set bucket sizes
            var buckets = list.Count;
            var count = options.Count;
            var min = count / buckets;
            var rem = count % buckets;
            for (var i = 0; i < buckets; ++i)
                list[i].count = min + (i < rem ? 1 : 0);

            Debug.Assert(list.Sum(p => p.count) == options.Count);

            return list;
        }


#endregion
    }
#endif
}
