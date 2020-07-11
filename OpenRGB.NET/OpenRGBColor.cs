using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRGB.NET
{
    public class OpenRGBColor : IEquatable<OpenRGBColor>
    {
        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }

        public OpenRGBColor(byte red = 0, byte green = 0, byte blue = 0)
        {
            R = red;
            G = green;
            B = blue;
        }

        public OpenRGBColor(OpenRGBColor other)
        {
            if (other is null)
                throw new ArgumentNullException(nameof(other));

            R = other.R;
            G = other.G;
            B = other.B;
        }

        public static OpenRGBColor FromRgb(byte red, byte green, byte blue)
        {
            return new OpenRGBColor { R = red, G = green, B = blue };
        }

        public static OpenRGBColor FromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = (hue / 60) - Math.Floor(hue / 60);

            value *= 255;
            var v = Convert.ToByte(value);
            var p = Convert.ToByte(value * (1 - saturation));
            var q = Convert.ToByte(value * (1 - (f * saturation)));
            var t = Convert.ToByte(value * (1 - ((1 - f) * saturation)));

            switch (hi)
            {
                case 0:
                    return FromRgb(v, t, p);
                case 1:
                    return FromRgb(q, v, p);
                case 2:
                    return FromRgb(p, v, t);
                case 3:
                    return FromRgb(p, q, v);
                case 4:
                    return FromRgb(t, p, v);
                default:
                    return FromRgb(v, p, q);
            }
        }

        public (double h, double s, double v) ToHsv()
        {
            var max = Math.Max(R, Math.Max(G, B));
            var min = Math.Min(R, Math.Min(G, B));

            var delta = max - min;

            var hue = 0d;
            if (delta != 0)
            {
                if (R == max) hue = (G - B) / (double)delta;
                else if (G == max) hue = 2d + ((B - R) / (double)delta);
                else if (B == max) hue = 4d + ((R - G) / (double)delta);
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            var saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            var value = max / 255d;

            return (hue, saturation, value);
        }

        internal static OpenRGBColor[] Decode(byte[] buffer, ref int offset, ushort colorCount)
        {
            var colors = new List<OpenRGBColor>(colorCount);

            for (int i = 0; i < colorCount; i++)
            {
                colors.Add(new OpenRGBColor
                {
                    R = buffer[offset],
                    G = buffer[offset + 1],
                    B = buffer[offset + 2]
                    //Alpha = buffer[offset + 3]
                });
                offset += 4 * sizeof(byte);
            }
            return colors.ToArray();
        }

        internal byte[] Encode()
        {
            return new byte[]
            {
                R,
                G,
                B,
                0
            };
        }

        public OpenRGBColor GetHueShiftedColor(double offset)
        {
            if (offset == 0)
                return this;

            var (h, s, v) = ToHsv();

            h += offset;

            while (h > 360) h -= 360;
            while (h < 0) h += 360;

            return FromHsv(h, s, v);
        }

        public static IEnumerable<OpenRGBColor> GetRainbow(OpenRGBColor start, int amount) => Enumerable.Range(0, amount)
            .Select(i => new OpenRGBColor(start.GetHueShiftedColor(360.0d / amount * i)));

        public static IEnumerable<OpenRGBColor> GetRainbow(OpenRGBColor start, uint amount) => GetRainbow(start, (int)amount);

        public static IEnumerable<OpenRGBColor> GetRainbow(int amount) => GetRainbow(new OpenRGBColor(255, 0, 0), amount);

        public static IEnumerable<OpenRGBColor> GetRainbow(uint amount) => GetRainbow(new OpenRGBColor(255, 0, 0), (int)amount);

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B} ";
        }

        public bool Equals(OpenRGBColor other) =>
            this.R == other.R &&
            this.G == other.G &&
            this.B == other.B;
    }
}
