namespace Lomont.Graphics
{
    /// <summary>
    /// Basic RGB color as bytes
    /// </summary>
    public class ColorB
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public byte Alpha  { get; set; }

        public ColorB(byte red, byte green, byte blue, byte alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;

        }
        public ColorB(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 255;
        }

        // todo - add parse, more things from Color
        public override string ToString()
        {
            return $"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
        }
    }
}
