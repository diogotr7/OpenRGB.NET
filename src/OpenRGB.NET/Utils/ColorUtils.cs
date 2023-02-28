using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenRGB.NET.Utils;

/// <summary>
///     Utility class for colors.
/// </summary>
public static class ColorUtils
{
    /// <summary>
    ///     Method used to create a color from HSV values.
    /// </summary>
    /// <param name="hue">Hue ranges from 0 to 360, input range wraps automatically.</param>
    /// <param name="saturation">Ranges from 0.0 to 1.0.</param>
    /// <param name="value">Ranges from 0.0 to 1.0.</param>
    /// <returns>The color converted to RGB.</returns>
    public static Color FromHsv(double hue, double saturation, double value)
    {
        if (saturation is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(saturation));
        if (value is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(value));
        
        while (hue < 0) { hue += 360; }
        while (hue >= 360) { hue -= 360; }

        var hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
        var f = hue / 60 - Math.Floor(hue / 60);

        value *= 255;
        var v = Convert.ToByte(value);
        var p = Convert.ToByte(value * (1 - saturation));
        var q = Convert.ToByte(value * (1 - f * saturation));
        var t = Convert.ToByte(value * (1 - (1 - f) * saturation));

        switch (hi)
        {
            case 0:
                return new Color(v, t, p);
            case 1:
                return new Color(q, v, p);
            case 2:
                return new Color(p, v, t);
            case 3:
                return new Color(p, q, v);
            case 4:
                return new Color(t, p, v);
            default:
                return new Color(v, p, q);
        }
    }

    /// <summary>
    ///     Converts a color to HSV.
    /// </summary>
    /// <returns>Tuple with the HSV values.</returns>
    public static (double h, double s, double v) ToHsv(this Color clr)
    {
        var max = Math.Max(clr.R, Math.Max(clr.G, clr.B));
        var min = Math.Min(clr.R, Math.Min(clr.G, clr.B));

        var delta = max - min;

        var hue = 0d;
        if (delta != 0)
        {
            if (clr.R == max) hue = (clr.G - clr.B) / (double)delta;
            else if (clr.G == max) hue = 2d + (clr.B - clr.R) / (double)delta;
            else if (clr.B == max) hue = 4d + (clr.R - clr.G) / (double)delta;
        }

        hue *= 60;
        if (hue < 0.0) hue += 360;

        var saturation = max == 0 ? 0 : 1d - 1d * min / max;
        var value = max / 255d;

        return (hue, saturation, value);
    }

    /// <summary>
    ///     Generates a smooth rainbow with the given amount of colors.
    ///     Uses HSV conversion to get a hue-based rainbow.
    /// </summary>
    /// <param name="amount">How many colors to generate.</param>
    /// <param name="hueStart">The hue of the first color, 0 to 360.</param>
    /// <param name="huePercent">How much of the hue scale to use. 1.0 represents the full range.</param>
    /// <param name="saturation">The HSV saturation of the colors, 0.0 to 1.0.</param>
    /// <param name="value">The HSV value of the colors, 0.0 to 1.0.</param>
    /// <returns>An collection of Colors in a rainbow pattern.</returns>
    public static IEnumerable<Color> GetHueRainbow(int amount, double hueStart = 0, double huePercent = 1.0,
        double saturation = 1.0, double value = 1.0)
    {
        return Enumerable.Range(0, amount)
            .Select(i => FromHsv(hueStart + 360.0d * huePercent / amount * i, saturation, value));
    }

    /// <summary>
    ///     Generates a smooth rainbow with the given amount of colors.
    ///     Uses sine waves to generate the pattern.
    /// </summary>
    /// <param name="amount">How many colors to generate.</param>
    /// <param name="floor">The least bright any given RGB value can be.</param>
    /// <param name="width">The brightness variation of any given RGB value.</param>
    /// <param name="range">
    ///     How much of the sine wave is used to generate the colors. Decrease this value to get a fraction of
    ///     the spectrum. In percent.
    /// </param>
    /// <param name="offset">The value the first color of the sequence will be generated with.</param>
    /// <returns>A collection of Colors in a rainbow pattern.</returns>
    public static IEnumerable<Color> GetSinRainbow(int amount, int floor = 127, int width = 128, double range = 1.0,
        double offset = Math.PI / 2)
    {
        return Enumerable.Range(0, amount)
            .Select(i => new Color(
                (byte)(floor + width * Math.Sin(offset + 2 * Math.PI * range / amount * i + 0)),
                (byte)(floor + width * Math.Sin(offset + 2 * Math.PI * range / amount * i + 2 * Math.PI / 3)),
                (byte)(floor + width * Math.Sin(offset + 2 * Math.PI * range / amount * i + 4 * Math.PI / 3))
            ));
    }
}