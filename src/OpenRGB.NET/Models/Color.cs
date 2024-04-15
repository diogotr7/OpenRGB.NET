using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Represents a color.
/// </summary>
/// <param name="R">The Red component</param>
/// <param name="G">The Green component</param>
/// <param name="B">The Blue component</param>
public readonly record struct Color(byte R = 0, byte G = 0, byte B = 0)
{
    /// <summary>
    ///     Red value of the color.
    /// </summary>
    public byte R { get; } = R;

    /// <summary>
    ///     Green value of the color.
    /// </summary>
    public byte G { get; } = G;

    /// <summary>
    ///     Blue value of the color.
    /// </summary>
    public byte B { get; } = B;

    //Added for padding, so Color fits within 4 bytes. TODO verify byte order
    private readonly byte UnusedAlpha = 0;

    private static Color ReadFrom(ref SpanReader reader)
    {
        var r = reader.Read<byte>();
        var g = reader.Read<byte>();
        var b = reader.Read<byte>();
        var a = reader.Read<byte>();

        return new Color(r, g, b);
    }

    internal static Color[] ReadManyFrom(ref SpanReader reader, int count)
    {
        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be greater than or equal to 0.");

        if (count == 0)
            return [];
        
        var colors = new Color[count];

        for (var i = 0; i < count; i++)
            colors[i] = ReadFrom(ref reader);

        return colors;
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.Write(R);
        writer.Write(G);
        writer.Write(B);
        writer.Write(UnusedAlpha);
    }
}