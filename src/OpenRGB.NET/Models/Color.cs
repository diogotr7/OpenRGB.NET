using System.Runtime.InteropServices;

namespace OpenRGB.NET;

/// <summary>
///     Represents a color.
/// </summary>
/// <param name="R">The Red component</param>
/// <param name="G">The Green component</param>
/// <param name="B">The Blue component</param>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
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

    //Added for padding, so Color fits within 4 bytes.
    private readonly byte UnusedAlpha = 0;
}