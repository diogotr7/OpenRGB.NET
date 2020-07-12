using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public static OpenRGBColor FromHsv(double hue, double saturation, double value)
        {
            if (saturation < 0 || saturation > 1)
                throw new ArgumentOutOfRangeException(nameof(saturation));
            if (value < 0 || value > 1)
                throw new ArgumentOutOfRangeException(nameof(value));

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
                    return new OpenRGBColor(v, t, p);
                case 1:
                    return new OpenRGBColor(q, v, p);
                case 2:
                    return new OpenRGBColor(p, v, t);
                case 3:
                    return new OpenRGBColor(p, q, v);
                case 4:
                    return new OpenRGBColor(t, p, v);
                default:
                    return new OpenRGBColor(v, p, q);
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

        public static IEnumerable<OpenRGBColor> GetRainbow(int amount, double offset = 0, double range = 360.0d) => Enumerable.Range(0, amount)
            .Select(i => FromHsv(offset + (range / amount * i), 1, 1));

        public static IEnumerable<OpenRGBColor> GetRainbow(uint amount, double offset = 0, double range = 360.0d) => GetRainbow((int)amount, offset, range);

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B} ";
        }

        public bool Equals(OpenRGBColor other) =>
            this.R == other.R &&
            this.G == other.G &&
            this.B == other.B;

        public OpenRGBColor Clone() => new OpenRGBColor(R, G, B);
    }
}
