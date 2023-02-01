using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///  Represents a color.
/// </summary>
/// <param name="R">The Red component</param>
/// <param name="G">The Green component</param>
/// <param name="B">The Blue component</param>
public readonly record struct Color(byte R, byte G, byte B)
{
    /// <summary>
    /// Red value of the color.
    /// </summary>
    public byte R { get; } = R;

    /// <summary>
    /// Green value of the color.
    /// </summary>
    public byte G { get; } = G;

    /// <summary>
    /// Blue value of the color.
    /// </summary>
    public byte B { get; } = B;

    /// <summary>
    /// Constructs a default Color struct, set to (0,0,0).
    /// </summary>
    public Color() : this(0, 0, 0) { }
    
    internal static Color ReadFrom(ref SpanReader reader)
    {
        var r = reader.ReadByte();
        var g = reader.ReadByte();
        var b = reader.ReadByte();
        var a = reader.ReadByte();
        
        return new Color(r, g, b);
    }
    
    internal static Color[] ReadManyFrom(ref SpanReader reader, int count)
    { 
        var colors = new Color[count];
        
        for (int i = 0; i < count; i++)
            colors[i] = ReadFrom(ref reader);
        
        return colors;
    }
    
    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteByte(R);
        writer.WriteByte(G);
        writer.WriteByte(B);
        writer.WriteByte(0);
    }
}
