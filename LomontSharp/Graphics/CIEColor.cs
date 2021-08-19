using System;
using static Lomont.Numerical.Utility;

namespace Lomont.Graphics
{
    /// <summary>
    /// CIE color spaces CIEXYZ and CIELAB (also known as L*a*b) 
    /// </summary>
    public static class CIEColor
    {

        /// <summary>
        /// Convert RGB each in 0-1 to CIE XYZ (CIE 1931 color space)
        /// </summary>
        /// <param name="rgbColor"></param>
        /// <returns></returns>
        public static Color RGBToXYZ(Color rgbColor)
        {
            // components in 0-1
            var (r, g, b) = ColorUtils.LinearRGBToSRGB(rgbColor);


            // applying the matrix
            var x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            var y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            var z = r * 0.0193 + g * 0.1192 + b * 0.9505;

            return new Color(x, y, z, rgbColor.A);
        }

        /// <summary>
        /// Converts CIEXYZ to RGB structure.
        /// </summary>
        public static Color XYZToRGB(Color color)
        {
            var (x, y, z, a) = color;

            var red = Clamp01(Func(x * 3.2406 - y * 1.5372 - z * 0.4986));
            var green = Clamp01(Func(-x * 0.9689 + y * 1.8758 - z * 0.0415));
            var blue = Clamp01(Func(x * 0.0557 - y * 0.2040 + z * 1.0570));

            return new(red, green, blue, a);

            // invert sRGB?
            static double Func(double val) => ColorUtils.SRGBToLinearRGB(val);

        }

        /// <summary>
        /// Converts RGB to CIELab.
        /// </summary>
        public static Color RGBToLab(Color color) => XYZToLab(RGBToXYZ(color));

        /// <summary>
        /// Converts CIELab to RGB.
        /// </summary>
        public static Color LabToRGB(Color color) => XYZToRGB(LabToXYZ(color));

        /// <summary>
        /// Convert XYZ color to La*b* color
        /// L coords are in 0-100, a,b unbounded, often clipped to -128 to 127
        /// XYZ are in 
        /// </summary>
        /// <returns></returns>
        public static Color XYZToLab(Color color)
        {

            var (x, y, z) = color;

            // apply white point (todo- make option?), map function
            var L = 116.0 * LabXyzFunction(y / cie65y) - 16;
            var a = 500.0 * (LabXyzFunction(x / cie65x) - LabXyzFunction(y / cie65y));
            var b = 200.0 * (LabXyzFunction(y / cie65y) - LabXyzFunction(z / cie65z));

            return new Color(L, a, b, color.A);

            // XYZ to L*a*b* transformation function.
            static double LabXyzFunction(double t)
            {
                return ((t > delta3) ? Math.Pow(t, (1.0 / 3.0)) : (t * div + 4.0 / 29.0));
            }

        }


        /// <summary>
        /// Converts CIELab to CIEXYZ.
        /// </summary>
        public static Color LabToXYZ(Color color)
        {
            var (l, a, b, alpha) = color;

            var l2 = (l + 16.0) / 116.0;

            var x = cie65x * InverseLab(l2 + (a / 500));
            var y = cie65y * InverseLab(l2);
            var z = cie65z * InverseLab(l2 - (b / 200));
            return new Color(x, y, z, alpha);

            // helper function
            static double InverseLab(double t)
            {
                return t > delta
                    ? (t * t * t)
                    : (3 * delta2 * (t - 4.0 / 29.0));
            }

        }

        #region Misc

        /// <summary>
        /// Compute the difference between RGB colors in Delta E 1994 method
        /// </summary>
        /// <param name="rgbColor1"></param>
        /// <param name="rgbColor2"></param>
        /// <returns></returns>
        public static double DeltaE94_From_sRGB_OLD(Color rgbColor1, Color rgbColor2)
        {
            var xyz1 = RGBToXYZ(rgbColor1);
            var xyz2 = RGBToXYZ(rgbColor2);
            var (l1, a1, b1) = XYZToLab(xyz1);
            var (l2, a2, b2) = XYZToLab(xyz2);

            var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var dc = c1 - c2;
            var dl = l1 - l2;
            var da = a1 - a2;
            var db = b1 - b2;
            var dh = Math.Sqrt(da * da + db * db - dc * dc);
            var first = dl;
            var c = Math.Sqrt(c1 *
                              c2); // this one is symmetric, see // see http://www.gamutvision.com/docs/gamutvision_equations.html
            //var c = c1; // this one is asymmetric, used by Bruce Lindbloom
            var second = dc / (1 + 0.045 * c);
            var third = dh / (1 + 0.015 * c);
            var dE94 = Math.Sqrt(first * first + second * second + third * third);
            return dE94;
        }


        // Lab <-> XYZ functions
        private const double delta = 6.0 / 29.0;
        private const double delta2 = delta * delta;
        private const double delta3 = delta * delta2;
        private const double div = 1 / (3.0 * delta2);



        // CIE D65 white point
        const double cie65x = 95.0489;
        const double cie65y = 100.0;
        const double cie65z = 108.840;

        // =lluminant D50 white point, often used for print
        const double cie50x = 96.4212;
        const double cie50y = 100.0;
        const double cie50z = 82.5188;

        /// <summary>
        /// Compute the difference between sRGB colors in Delta E 2000 method
        /// </summary>
        /// <param name="rgbColor1"></param>
        /// <param name="rgbColor2"></param>
        /// <returns></returns>
        public static double DeltaE00_From_sRGB(Color rgbColor1, Color rgbColor2)
        {
            // NOTE: read The CIEDE2000 Color-Difference Formula: Implementation Notes, Supplementary Test Data, and Mathematical Observations
            // by Sharma, Wu, Dalal
            // http://www.ece.rochester.edu/~gsharma/papers/CIEDE2000CRNAFeb05.pdf
            var xyz1 = RGBToXYZ(rgbColor1);
            var xyz2 = RGBToXYZ(rgbColor2);
            var (l1, a1, b1) = XYZToLab(xyz1);
            var (l2, a2, b2) = XYZToLab(xyz2);
            return DeltaE00_From_Lab(l1, a1, b1, l2, a2, b2);
        }



        // helper to compute DeltaE. If testIndex >= 0, runs a test case, throwing on failure
        static double DeltaE00_From_Lab(
            double l1, double a1, double b1,
            double l2, double a2, double b2,
            int testIndex = -1
        )
        {
            if (testIndex >= 0)
            {
                // get test data
                testIndex *= dataCols;
                l1 = Get();
                a1 = Get();
                b1 = Get();
                l2 = Get();
                a2 = Get();
                b2 = Get();
            }


            // return arctan in range [0,2pi)
            double ArcTan(double y, double x)
            {
                var v = Math.Atan2(y, x); // C# version returns in -pi to pi
                if (v < 0)
                    v += 2 * Math.PI;
                return v;
            }

            // Step 1 : calculate cip,hip
            var c1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var c2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var cAvg = (c1 + c2) / 2.0;
            var cAvgPow7 = Math.Pow(cAvg, 7);
            var g = 0.5 * (1 - Math.Sqrt(cAvgPow7 / (cAvgPow7 + Math.Pow(25, 7))));
            var a1p = (1 + g) * a1;
            var a2p = (1 + g) * a2;
            var c1p = Math.Sqrt(a1p * a1p + b1 * b1);
            var c2p = Math.Sqrt(a2p * a2p + b2 * b2);


            var h1p = 0.0;
            if (b1 != 0 || a1p != 0)
                h1p = ArcTan(b1, a1p);
            var h2p = 0.0;
            if (b2 != 0 || a2p != 0)
                h2p = ArcTan(b2, a2p);

            // Step 2 : compute dl,dc,dh
            var dl = l2 - l1;
            var dc = c2p - c1p;

            var deg180 = Math.PI;
            var deg360 = 2 * Math.PI;
            var deg30 = 30 * Math.PI / 180;
            var deg6 = 6 * Math.PI / 180;
            var deg63 = 63 * Math.PI / 180;
            var deg275 = 275 * Math.PI / 180;

            var dh = 0.0;
            if (c1p * c2p == 0)
                dh = 0;
            else if (c1p * c2p != 0 && Math.Abs(h2p - h1p) <= deg180)
                dh = h2p - h1p;
            else if (c1p * c2p != 0 && h2p - h1p > deg180)
                dh = h2p - h1p - deg360;
            else if (c1p * c2p != 0 && h2p - h1p < -deg180)
                dh = h2p - h1p + deg360;
            var dH = 2 * Math.Sqrt(c1p * c2p) * Math.Sin(dh / 2);

            // Step 3: calculate CIEDE2000 color difference dE00
            var lAvg = (l1 + l2) / 2.0;
            var cpAvg = (c1p + c2p) / 2.0;

            var hAvg = 0.0;
            if (Math.Abs(h1p - h2p) <= deg180 && c1p * c2p != 0)
                hAvg = (h1p + h2p) / 2;
            else if (Math.Abs(h1p - h2p) > deg180 && (h1p + h2p) < deg360 && c1p * c2p != 0)
                hAvg = (h1p + h2p + deg360) / 2;
            else if (Math.Abs(h1p - h2p) > deg180 && (h1p + h2p) >= deg360 && c1p * c2p != 0)
                hAvg = (h1p + h2p - deg360) / 2;
            else if (c1p * c2p == 0)
                hAvg = (h1p + h2p);

            var t = 1 - 0.17 * Math.Cos(hAvg - deg30) + 0.24 * Math.Cos(2 * hAvg) + 0.32 * Math.Cos(3 * hAvg + deg6) -
                    0.20 * Math.Cos(4 * hAvg - deg63);
            var q = ToDegrees(hAvg - deg275, false) / 25;
            var dt = 30 * Math.Exp(-q * q);

            var cBar7 = Math.Pow(cpAvg, 7);
            var rc = 2 * Math.Sqrt(cBar7 / (cBar7 + Math.Pow(25, 7)));

            var l50 = (lAvg - 50) * (lAvg - 50);
            var sl = 1 + 0.015 * l50 / Math.Sqrt(20 + l50);
            var sc = 1 + 0.045 * cpAvg;
            var sh = 1 + 0.015 * cpAvg * t;
            var rt = -Math.Sin(2 * ToRadians(dt)) * rc;

            // these three factors usually 1.0
            const double kl = 1.0;
            const double kc = 1.0;
            const double kh = 1.0;

            // dE terms
            var first = dl / (kl * sl);
            var second = dc / (kc * sc);
            var third = dH / (kh * sh);
            var fourth = rt * second * third;

            var dE00 = Math.Sqrt(first * first + second * second + third * third + fourth);

            if (testIndex >= 0)
            {
                // checks
                Check(c1);
                Check(c2);
                Check(cAvg);
                Check(g);
                Check(l1);
                Check(a1p);
                Check(b1);
                Check(l2);
                Check(a2p);
                Check(b2);
                Check(c1p);
                Check(c2p);
                Check(ToDegrees(h1p, false));
                Check(ToDegrees(h2p, false));

                // this test is different in paper and data, so we skip
                if (c1p * c2p == 0)
                    Get();
                else
                    Check(ToDegrees(dh, false));

                Check(dl);
                Check(dc);

                // this test is different in paper and data, so we skip
                Check(dH);
                //Get();

                Check(lAvg);
                Check(cpAvg);
                Check(ToDegrees(hAvg, false));
                Check(l50);
                Check(sl);
                Check(sc);
                Check(t);
                Check(sh);
                Check(dt);
                Check(rc);
                Check(rt);
                Check(first);
                Check(second);
                Check(third);
                Check(dE00);
            }

            return dE00;

            // helper functions
            double Get()
            {
                return testData[testIndex++];
            }

            void Check(double val)
            {
                if (!CloseEnough(val, Get()))
                {
                    throw new Exception();
                }
            }

        }

        // is |a-b| < epsilon?
        static bool CloseEnough(double a, double b)
        {
            return Math.Abs(a - b) < 0.05;
        }

        // test data from http://www.ece.rochester.edu/~gsharma/ciede2000/

        static CIEColor()
        {
            // Test(); // run the test, for debugging
        }

        /// <summary>
        /// run test, throw on failure
        /// </summary>
        /// <returns></returns>
        public static bool TestDeltaE00()
        {
            var success = true;
            for (var i = 0; i < testData.Length / dataCols; ++i)
            {
                // computing the value checks intermediate values
                var dE00 = DeltaE00_From_Lab(0, 0, 0, 0, 0, 0, i);

                // symmetry check
                var l1 = testData[i * dataCols + 0];
                var a1 = testData[i * dataCols + 1];
                var b1 = testData[i * dataCols + 2];
                var l2 = testData[i * dataCols + 3];
                var a2 = testData[i * dataCols + 4];
                var b2 = testData[i * dataCols + 5];

                var dE00_symm = DeltaE00_From_Lab(l2, a2, b2, l1, a1, b1);
                if (!CloseEnough(dE00, dE00_symm))
                {
                    success = false;
                    throw new Exception("Color test failed");
                }
            }

            return success;
        }

        // CIE2000 deltaE test data, 39 columns
        const int dataCols = 39;

        private static double[] testData =
        {

            //L1 a1  b1  L2  a2  b2  C1  C2  C_ave   G   L1'	a1' b1'	L2' a2'	b2' C1'	C2' h1'	h2' dh'	dL' dC'	dH' L'_ave	C'_ave  h'_ave	(L'_ave-50)^2   S_L S_C T   S_H dTheta  R_C R_T dL'/k_L/S_L	dC'/k_C/S_C dH'/k_H/S_H	dE2000

            50.0000, 2.6772, -79.7751, 50.0000, 0.0000, -82.7485, 79.8200, 82.7485, 81.2843, 0.0001, 50.0000, 2.6774,
            -79.7751, 50.0000, 0.0000, -82.7485, 79.8200, 82.7485, 271.9222, 270.0000, -1.9222, 0.0000, 2.9285, -2.7264,
            50.0000, 81.2843, 270.9611, 0.0000, 1.0000, 4.6578, 0.6907, 1.8421, 29.2271, 1.9997, -1.7042, 0.0000,
            0.6287, -1.4800, 2.0425,
            50.0000, 3.1571, -77.2803, 50.0000, 0.0000, -82.7485, 77.3448, 82.7485, 80.0466, 0.0001, 50.0000, 3.1573,
            -77.2803, 50.0000, 0.0000, -82.7485, 77.3448, 82.7485, 272.3395, 270.0000, -2.3395, 0.0000, 5.4037, -3.2664,
            50.0000, 80.0466, 271.1698, 0.0000, 1.0000, 4.6021, 0.6843, 1.8216, 29.3040, 1.9997, -1.7070, 0.0000,
            1.1742, -1.7932, 2.8615,
            50.0000, 2.8361, -74.0200, 50.0000, 0.0000, -82.7485, 74.0743, 82.7485, 78.4114, 0.0001, 50.0000, 2.8363,
            -74.0200, 50.0000, 0.0000, -82.7485, 74.0743, 82.7485, 272.1944, 270.0000, -2.1944, 0.0000, 8.6742, -2.9984,
            50.0000, 78.4114, 271.0972, 0.0000, 1.0000, 4.5285, 0.6865, 1.8074, 29.2777, 1.9997, -1.7060, 0.0000,
            1.9155, -1.6589, 3.4412,
            50.0000, -1.3802, -84.2814, 50.0000, 0.0000, -82.7485, 84.2927, 82.7485, 83.5206, 0.0001, 50.0000, -1.3803,
            -84.2814, 50.0000, 0.0000, -82.7485, 84.2927, 82.7485, 269.0618, 270.0000, 0.9382, 0.0000, -1.5442, 1.3676,
            50.0000, 83.5206, 269.5309, 0.0000, 1.0000, 4.7584, 0.7357, 1.9217, 28.5981, 1.9998, -1.6809, 0.0000,
            -0.3245, 0.7117, 1.0000,
            50.0000, -1.1848, -84.8006, 50.0000, 0.0000, -82.7485, 84.8089, 82.7485, 83.7787, 0.0001, 50.0000, -1.1849,
            -84.8006, 50.0000, 0.0000, -82.7485, 84.8089, 82.7485, 269.1995, 270.0000, 0.8005, 0.0000, -2.0604, 1.1704,
            50.0000, 83.7787, 269.5997, 0.0000, 1.0000, 4.7700, 0.7335, 1.9218, 28.6323, 1.9998, -1.6822, 0.0000,
            -0.4319, 0.6090, 1.0000,
            50.0000, -0.9009, -85.5211, 50.0000, 0.0000, -82.7485, 85.5258, 82.7485, 84.1372, 0.0001, 50.0000, -0.9009,
            -85.5211, 50.0000, 0.0000, -82.7485, 85.5258, 82.7485, 269.3964, 270.0000, 0.6036, 0.0000, -2.7773, 0.8862,
            50.0000, 84.1372, 269.6982, 0.0000, 1.0000, 4.7862, 0.7303, 1.9217, 28.6807, 1.9998, -1.6840, 0.0000,
            -0.5803, 0.4611, 1.0000,
            50.0000, 0.0000, 0.0000, 50.0000, -1.0000, 2.0000, 0.0000, 2.2361, 1.1180, 0.5000, 50.0000, 0.0000, 0.0000,
            50.0000, -1.5000, 2.0000, 0.0000, 2.5000, 0.0000, 126.8697, 126.8697, 0.0000, 2.5000, 0.0000, 50.0000,
            1.2500, 126.8697, 0.0000, 1.0000, 1.0562, 1.2200, 1.0229, 0.0000, 0.0001, 0.0000, 0.0000, 2.3669, 0.0000,
            2.3669,
            50.0000, -1.0000, 2.0000, 50.0000, 0.0000, 0.0000, 2.2361, 0.0000, 1.1180, 0.5000, 50.0000, -1.5000, 2.0000,
            50.0000, 0.0000, 0.0000, 2.5000, 0.0000, 126.8697, 0.0000, -126.8697, 0.0000, -2.5000, 0.0000, 50.0000,
            1.2500, 126.8697, 0.0000, 1.0000, 1.0562, 1.2200, 1.0229, 0.0000, 0.0001, 0.0000, 0.0000, -2.3669, 0.0000,
            2.3669,
            50.0000, 2.4900, -0.0010, 50.0000, -2.4900, 0.0009, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, 3.7346,
            -0.0010, 50.0000, -3.7346, 0.0009, 3.7346, 3.7346, 359.9847, 179.9862, -179.9985, 0.0000, 0.0000, -7.4692,
            50.0000, 3.7346, 269.9854, 0.0000, 1.0000, 1.1681, 0.7212, 1.0404, 28.8170, 0.0026, -0.0022, 0.0000, 0.0000,
            -7.1792, 7.1792,
            50.0000, 2.4900, -0.0010, 50.0000, -2.4900, 0.0010, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, 3.7346,
            -0.0010, 50.0000, -3.7346, 0.0010, 3.7346, 3.7346, 359.9847, 179.9847, -180.0000, 0.0000, 0.0000, -7.4692,
            50.0000, 3.7346, 269.9847, 0.0000, 1.0000, 1.1681, 0.7212, 1.0404, 28.8166, 0.0026, -0.0022, 0.0000, 0.0000,
            -7.1792, 7.1792,
            50.0000, 2.4900, -0.0010, 50.0000, -2.4900, 0.0011, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, 3.7346,
            -0.0010, 50.0000, -3.7346, 0.0011, 3.7346, 3.7346, 359.9847, 179.9831, 179.9985, 0.0000, 0.0000, 7.4692,
            50.0000, 3.7346, 89.9839, 0.0000, 1.0000, 1.1681, 0.6175, 1.0346, 0.0000, 0.0026, 0.0000, 0.0000, 0.0000,
            7.2195, 7.2195,
            50.0000, 2.4900, -0.0010, 50.0000, -2.4900, 0.0012, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, 3.7346,
            -0.0010, 50.0000, -3.7346, 0.0012, 3.7346, 3.7346, 359.9847, 179.9816, 179.9969, 0.0000, 0.0000, 7.4692,
            50.0000, 3.7346, 89.9831, 0.0000, 1.0000, 1.1681, 0.6175, 1.0346, 0.0000, 0.0026, 0.0000, 0.0000, 0.0000,
            7.2195, 7.2195,
            50.0000, -0.0010, 2.4900, 50.0000, 0.0009, -2.4900, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, -0.0015,
            2.4900, 50.0000, 0.0013, -2.4900, 2.4900, 2.4900, 90.0345, 270.0311, 179.9965, 0.0000, 0.0000, 4.9800,
            50.0000, 2.4900, 180.0328, 0.0000, 1.0000, 1.1121, 0.9779, 1.0365, 0.0000, 0.0006, 0.0000, 0.0000, 0.0000,
            4.8045, 4.8045,
            50.0000, -0.0010, 2.4900, 50.0000, 0.0010, -2.4900, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, -0.0015,
            2.4900, 50.0000, 0.0015, -2.4900, 2.4900, 2.4900, 90.0345, 270.0345, 180.0000, 0.0000, 0.0000, 4.9800,
            50.0000, 2.4900, 180.0345, 0.0000, 1.0000, 1.1121, 0.9779, 1.0365, 0.0000, 0.0006, 0.0000, 0.0000, 0.0000,
            4.8045, 4.8045,
            50.0000, -0.0010, 2.4900, 50.0000, 0.0011, -2.4900, 2.4900, 2.4900, 2.4900, 0.4998, 50.0000, -0.0015,
            2.4900, 50.0000, 0.0016, -2.4900, 2.4900, 2.4900, 90.0345, 270.0380, -179.9965, 0.0000, 0.0000, -4.9800,
            50.0000, 2.4900, 0.0362, 0.0000, 1.0000, 1.1121, 1.3197, 1.0493, 0.0000, 0.0006, 0.0000, 0.0000, 0.0000,
            -4.7461, 4.7461,
            50.0000, 2.5000, 0.0000, 50.0000, 0.0000, -2.5000, 2.5000, 2.5000, 2.5000, 0.4998, 50.0000, 3.7496, 0.0000,
            50.0000, 0.0000, -2.5000, 3.7496, 2.5000, 0.0000, 270.0000, -90.0000, 0.0000, -1.2496, -4.3299, 50.0000,
            3.1248, 315.0000, 0.0000, 1.0000, 1.1406, 0.8454, 1.0396, 2.3191, 0.0014, -0.0001, 0.0000, -1.0956, -4.1649,
            4.3065,
            50.0000, 2.5000, 0.0000, 73.0000, 25.0000, -18.0000, 2.5000, 30.8058, 16.6529, 0.3827, 50.0000, 3.4569,
            0.0000, 73.0000, 34.5687, -18.0000, 3.4569, 38.9743, 0.0000, 332.4939, -27.5061, 23.0000, 35.5174, -5.5190,
            61.5000, 21.2156, 346.2470, 132.2500, 1.1608, 1.9547, 1.4453, 1.4599, 0.0089, 0.9812, -0.0003, 19.8144,
            18.1703, -3.7803, 27.1492,
            50.0000, 2.5000, 0.0000, 61.0000, -5.0000, 29.0000, 2.5000, 29.4279, 15.9639, 0.3981, 50.0000, 3.4954,
            0.0000, 61.0000, -6.9907, 29.0000, 3.4954, 29.8307, 0.0000, 103.5532, 103.5532, 11.0000, 26.3353, 16.0440,
            55.5000, 16.6630, 51.7766, 30.2500, 1.0640, 1.7498, 0.6447, 1.1612, 0.0000, 0.4699, 0.0000, 10.3382,
            15.0502, 13.8173, 22.8977,
            50.0000, 2.5000, 0.0000, 56.0000, -27.0000, -3.0000, 2.5000, 27.1662, 14.8331, 0.4206, 50.0000, 3.5514,
            0.0000, 56.0000, -38.3556, -3.0000, 3.5514, 38.4728, 0.0000, 184.4723, -175.5277, 6.0000, 34.9213, -23.3603,
            53.0000, 21.0121, 272.2362, 9.0000, 1.0251, 1.9455, 0.6521, 1.2055, 29.6356, 0.9562, -0.8219, 5.8533,
            17.9494, -19.3775, 31.9030,
            50.0000, 2.5000, 0.0000, 58.0000, 24.0000, 15.0000, 2.5000, 28.3019, 15.4010, 0.4098, 50.0000, 3.5244,
            0.0000, 58.0000, 33.8342, 15.0000, 3.5244, 37.0102, 0.0000, 23.9095, 23.9095, 8.0000, 33.4858, 4.7315,
            54.0000, 20.2673, 11.9548, 16.0000, 1.0400, 1.9120, 1.1031, 1.3353, 0.0000, 0.8651, 0.0000, 7.6923, 17.5132,
            3.5433, 19.4535,
            50.0000, 2.5000, 0.0000, 50.0000, 3.1736, 0.5854, 2.5000, 3.2271, 2.8636, 0.4997, 50.0000, 3.7494, 0.0000,
            50.0000, 4.7596, 0.5854, 3.7494, 4.7954, 0.0000, 7.0113, 7.0113, 0.0000, 1.0461, 0.5186, 50.0000, 4.2724,
            3.5056, 0.0000, 1.0000, 1.1923, 1.2616, 1.0808, 0.0000, 0.0041, 0.0000, 0.0000, 0.8774, 0.4798, 1.0000,
            50.0000, 2.5000, 0.0000, 50.0000, 3.2972, 0.0000, 2.5000, 3.2972, 2.8986, 0.4997, 50.0000, 3.7493, 0.0000,
            50.0000, 4.9450, 0.0000, 3.7493, 4.9450, 0.0000, 0.0000, 0.0000, 0.0000, 1.1956, 0.0000, 50.0000, 4.3471,
            0.0000, 0.0000, 1.0000, 1.1956, 1.3202, 1.0861, 0.0000, 0.0044, 0.0000, 0.0000, 1.0000, 0.0000, 1.0000,
            50.0000, 2.5000, 0.0000, 50.0000, 1.8634, 0.5757, 2.5000, 1.9503, 2.2252, 0.4999, 50.0000, 3.7497, 0.0000,
            50.0000, 2.7949, 0.5757, 3.7497, 2.8536, 0.0000, 11.6380, 11.6380, 0.0000, -0.8961, 0.6633, 50.0000, 3.3017,
            5.8190, 0.0000, 1.0000, 1.1486, 1.2197, 1.0604, 0.0000, 0.0017, 0.0000, 0.0000, -0.7802, 0.6255, 1.0000,
            50.0000, 2.5000, 0.0000, 50.0000, 3.2592, 0.3350, 2.5000, 3.2763, 2.8882, 0.4997, 50.0000, 3.7493, 0.0000,
            50.0000, 4.8879, 0.3350, 3.7493, 4.8994, 0.0000, 3.9206, 3.9206, 0.0000, 1.1500, 0.2932, 50.0000, 4.3244,
            1.9603, 0.0000, 1.0000, 1.1946, 1.2883, 1.0836, 0.0000, 0.0043, 0.0000, 0.0000, 0.9627, 0.2706, 1.0000,
            60.2574, -34.0099, 36.2677, 60.4626, -34.1751, 39.4387, 49.7194, 52.1857, 50.9526, 0.0017, 60.2574,
            -34.0678, 36.2677, 60.4626, -34.2333, 39.4387, 49.7590, 52.2238, 133.2085, 130.9584, -2.2501, 0.2052,
            2.4648, -2.0018, 60.3600, 50.9914, 132.0835, 107.3296, 1.1427, 3.2946, 1.3010, 1.9951, 0.0000, 1.9932,
            0.0000, 0.1796, 0.7481, -1.0034, 1.2644,
            63.0109, -31.0961, -5.8663, 62.8187, -29.7946, -4.0864, 31.6446, 30.0735, 30.8591, 0.0490, 63.0109,
            -32.6194, -5.8663, 62.8187, -31.2542, -4.0864, 33.1427, 31.5202, 190.1951, 187.4490, -2.7461, -0.1922,
            -1.6225, -1.5490, 62.9148, 32.3315, 188.8221, 166.7921, 1.1831, 2.4549, 0.9402, 1.4560, 0.0002, 1.8527,
            0.0000, -0.1625, -0.6609, -1.0639, 1.2630,
            61.2901, 3.7196, -5.3901, 61.4292, 2.2480, -4.9620, 6.5489, 5.4475, 5.9982, 0.4966, 61.2901, 5.5668,
            -5.3901, 61.4292, 3.3644, -4.9620, 7.7487, 5.9950, 315.9240, 304.1385, -11.7855, 0.1391, -1.7537, -1.3995,
            61.3597, 6.8719, 310.0313, 129.0416, 1.1586, 1.3092, 0.6952, 1.0717, 4.2110, 0.0218, -0.0032, 0.1201,
            -1.3395, -1.3059, 1.8731,
            35.0831, -44.1164, 3.7933, 35.0232, -40.0716, 1.5901, 44.2792, 40.1031, 42.1912, 0.0063, 35.0831, -44.3939,
            3.7933, 35.0232, -40.3237, 1.5901, 44.5557, 40.3550, 175.1161, 177.7418, 2.6257, -0.0599, -4.2007, 1.9430,
            35.0532, 42.4554, 176.4290, 223.4083, 1.2148, 2.9105, 1.0168, 1.6476, 0.0000, 1.9759, 0.0000, -0.0493,
            -1.4433, 1.1793, 1.8645,
            22.7233, 20.0904, -46.6940, 23.0331, 14.9730, -42.5619, 50.8326, 45.1188, 47.9757, 0.0026, 22.7233, 20.1424,
            -46.6940, 23.0331, 15.0118, -42.5619, 50.8532, 45.1317, 293.3339, 289.4279, -3.9060, 0.3098, -5.7215,
            -3.2653, 22.8782, 47.9924, 291.3809, 735.5920, 1.4014, 3.1597, 0.3636, 1.2617, 19.5282, 1.9897, -1.2537,
            0.2211, -1.8108, -2.5880, 2.0373,
            36.4612, 47.8580, 18.3852, 36.2715, 50.5065, 21.2231, 51.2680, 54.7844, 53.0262, 0.0013, 36.4612, 47.9197,
            18.3852, 36.2715, 50.5716, 21.2231, 51.3256, 54.8444, 20.9901, 22.7660, 1.7759, -0.1897, 3.5188, 1.6444,
            36.3664, 53.0850, 21.8781, 185.8764, 1.1943, 3.3888, 0.9239, 1.7357, 0.0000, 1.9949, 0.0000, -0.1588,
            1.0384, 0.9474, 1.4146,
            90.8027, -2.0831, 1.4410, 91.1528, -1.6435, 0.0447, 2.5329, 1.6441, 2.0885, 0.4999, 90.8027, -3.1245,
            1.4410, 91.1528, -2.4651, 0.0447, 3.4408, 2.4655, 155.2410, 178.9612, 23.7202, 0.3501, -0.9752, 1.1972,
            90.9778, 2.9531, 167.1011, 1679.1760, 1.6110, 1.1329, 1.1546, 1.0511, 0.0000, 0.0011, 0.0000, 0.2173,
            -0.8608, 1.1390, 1.4441,
            90.9257, -0.5406, -0.9208, 88.6381, -0.8985, -0.7239, 1.0678, 1.1538, 1.1108, 0.5000, 90.9257, -0.8109,
            -0.9208, 88.6381, -1.3477, -0.7239, 1.2270, 1.5298, 228.6315, 208.2412, -20.3903, -2.2876, 0.3029, -0.4850,
            89.7819, 1.3784, 218.4363, 1582.5996, 1.5930, 1.0620, 1.3916, 1.0288, 0.1794, 0.0001, 0.0000, -1.4360,
            0.2852, -0.4714, 1.5381,
            6.7747, -0.2908, -2.4247, 5.8714, -0.0985, -2.2286, 2.4421, 2.2308, 2.3364, 0.4999, 6.7747, -0.4362,
            -2.4247, 5.8714, -0.1477, -2.2286, 2.4636, 2.2335, 259.8025, 266.2073, 6.4048, -0.9033, -0.2301, 0.2621,
            6.3231, 2.3486, 263.0049, 1907.6760, 1.6517, 1.1057, 0.9556, 1.0337, 23.8310, 0.0005, -0.0004, -0.5469,
            -0.2081, 0.2535, 0.6377,
            2.0776, 0.0795, -1.1350, 0.9033, -0.0636, -0.5514, 1.1378, 0.5551, 0.8464, 0.5000, 2.0776, 0.1192, -1.1350,
            0.9033, -0.0954, -0.5514, 1.1412, 0.5596, 275.9978, 260.1842, -15.8136, -1.1743, -0.5817, -0.2199, 1.4905,
            0.8504, 268.0910, 2353.1764, 1.7246, 1.0383, 0.7826, 1.0100, 27.7941, 0.0000, 0.0000, -0.6809, -0.5602,
            -0.2177, 0.9082
        };

        /// <summary>
        /// Returns the color difference (distance) between 
        /// two Lab colors using the CIE 2000 algorithm
        /// </summary>
        /// <returns>Color difference.</returns>
        public static double LabColorDifference2000(
            double L1, double a1, double b1,
            double L2, double a2, double b2
        )
        {
            var p25 = Math.Pow(25, 7);

            var C1 = Math.Sqrt(a1 * a1 + b1 * b1);
            var C2 = Math.Sqrt(a2 * a2 + b2 * b2);
            var avgCp = (C1 + C2) / 2F;

            var powAvgC = Math.Pow(avgCp, 7);
            var G = (1 - Math.Sqrt(powAvgC / (powAvgC + p25))) / 2D;

            var a_1 = a1 * (1 + G);
            var a_2 = a2 * (1 + G);

            var c1 = Math.Sqrt(a_1 * a_1 + b1 * b1);
            var c2 = Math.Sqrt(a_2 * a_2 + b2 * b2);
            var avgCq = (c1 + c2) / 2D;

            var h1 = (Atan(b1, a_1) >= 0 ? Atan(b1, a_1) : Atan(b1, a_1) + 360F);
            var h2 = (Atan(b2, a_2) >= 0 ? Atan(b2, a_2) : Atan(b2, a_2) + 360F);

            var H = (h1 - h2 > 180D ? (h1 + h2 + 360F) / 2D : (h1 + h2) / 2D);

            var T = 1.0;
            T -= 0.17 * Cos(H - 30);
            T += 0.24 * Cos(2 * H);
            T += 0.32 * Cos(3 * H + 6);
            T -= 0.20 * Cos(4 * H - 63);

            var deltah = 0.0;
            if (h2 - h1 <= 180)
                deltah = h2 - h1;
            else if (h2 <= h1)
                deltah = h2 - h1 + 360;
            else
                deltah = h2 - h1 - 360;

            var avgL = (L1 + L2) / 2F;
            var deltaL = L2 - L1;
            var deltaC = c2 - c1;
            var deltaH = 2 * Math.Sqrt(c1 * c2) * Sin(deltah / 2);

            var sl = 1 + (0.015 * Math.Pow(avgL - 50, 2)) / Math.Sqrt(20 + Math.Pow(avgL - 50, 2));
            var sc = 1 + 0.045 * avgCq;
            var sh = 1 + 0.015 * avgCq * T;

            var exp = Math.Pow((H - 275) / 25, 2);
            var teta = Math.Pow(30, -exp);

            var rc = 2D * Math.Sqrt(Math.Pow(avgCq, 7) / (Math.Pow(avgCq, 7) + p25));
            var rt = -rc * Sin(2 * teta);

            var deltaE = 0.0;
            deltaE = Math.Pow(deltaL / sl, 2);
            deltaE += Math.Pow(deltaC / sc, 2);
            deltaE += Math.Pow(deltaH / sh, 2);
            deltaE += rt * (deltaC / sc) * (deltaH / sh);
            deltaE = Math.Sqrt(deltaE);

            return deltaE;
        }

        /// <summary>
        /// Returns the angle in degree whose tangent is the quotient of the two specified numbers.
        /// </summary>
        /// <param name="y">The y coordinate of a point.</param>
        /// <param name="x">The x coordinate of a point.</param>
        /// <returns>Angle in degree.</returns>
        private static double Atan(double y, double x)
        {
            return Math.Atan2(y, x) * 180D / Math.PI;
        }

        /// <summary>
        /// Returns the cosine of the specified angle in degree.
        /// </summary>
        /// <param name="d">Angle in degree</param>
        /// <returns>Cosine of the specified angle.</returns>
        private static double Cos(double d)
        {
            return Math.Cos(d * Math.PI / 180);
        }

        /// <summary>
        /// Returns the sine of the specified angle in degree.
        /// </summary>
        /// <param name="d">Angle in degree</param>
        /// <returns>Sine of the specified angle.</returns>
        private static double Sin(double d)
        {
            return Math.Sin(d * Math.PI / 180);
        }

        #endregion
    }
}
