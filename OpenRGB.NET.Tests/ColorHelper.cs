using System;

namespace OpenRGB.NET.Test
{
    public static class ColorHelper
    {
        public static void ToHsv(OpenRGBColor clr, out double hue, out double saturation, out double value)
        {
            var max = Math.Max(clr.Red, Math.Max(clr.Green, clr.Blue));
            var min = Math.Min(clr.Red, Math.Min(clr.Green, clr.Blue));

            var delta = max - min;

            hue = 0d;
            if (delta != 0)
            {
                if (clr.Red == max) hue = (clr.Green - clr.Blue) / (double)delta;
                else if (clr.Green == max) hue = 2d + (clr.Blue - clr.Red) / (double)delta;
                else if (clr.Blue == max) hue = 4d + (clr.Red - clr.Green) / (double)delta;
            }

            hue *= 60;
            if (hue < 0.0) hue += 360;

            saturation = (max == 0) ? 0 : 1d - (1d * min / max);
            value = max / 255d;
        }

        public static OpenRGBColor FromHsv(double hue, double saturation, double value)
        {
            saturation = Math.Max(Math.Min(saturation, 1), 0);
            value = Math.Max(Math.Min(value, 1), 0);

            var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            var f = hue / 60 - Math.Floor(hue / 60);

            value *= 255;
            var v = (byte)(value);
            var p = (byte)(value * (1 - saturation));
            var q = (byte)(value * (1 - f * saturation));
            var t = (byte)(value * (1 - (1 - f) * saturation));

            return hi switch
            {
                0 => new OpenRGBColor(v, t, p),
                1 => new OpenRGBColor(q, v, p),
                2 => new OpenRGBColor(p, v, t),
                3 => new OpenRGBColor(p, q, v),
                4 => new OpenRGBColor(t, p, v),
                _ => new OpenRGBColor(v, p, q),
            };
        }

        public static OpenRGBColor ChangeHue(this OpenRGBColor OpenRGBColor, double offset)
        {
            if (offset == 0)
                return OpenRGBColor;

            ToHsv(OpenRGBColor, out var hue, out var saturation, out var value);

            hue += offset;

            while (hue > 360) hue -= 360;
            while (hue < 0) hue += 360;

            return FromHsv(hue, saturation, value);
        }

        public static OpenRGBColor[] GetRainbow(OpenRGBColor start, int amount)
        {
            var array = new OpenRGBColor[amount];
            var hueIncrement = 360.0 / amount;
            for (int i = 0; i < amount; i++)
            {
                array[i] = new OpenRGBColor(start);
                start = start.ChangeHue(hueIncrement);
            }
            return array;
        }

        public static OpenRGBColor[] GetRainbow(OpenRGBColor start, uint amount) => GetRainbow(start, (int)amount);
    }
}
