using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Matrix Map class for the matrix Zone type
/// </summary>
public class MatrixMap
{
    private MatrixMap(uint height, uint width, uint[,] matrix)
    {
        Height = height;
        Width = width;
        Matrix = matrix;
    }

    /// <summary>
    ///     The height of the matrix.
    /// </summary>
    public uint Height { get; }

    /// <summary>
    ///     The width of the matrix.
    /// </summary>
    public uint Width { get; }

    /// <summary>
    ///     The matrix.
    /// </summary>
    public uint[,] Matrix { get; }

    internal uint Length => Height * Width * 4 + 8;

    /// <summary>
    ///     Decodes a byte array into a matrix map
    /// </summary>
    /// <param name="reader"></param>
    internal static MatrixMap ReadFrom(ref SpanReader reader)
    {
        var height = reader.ReadUInt32();

        var width = reader.ReadUInt32();

        var matrix = new uint[height, width];

        for (var i = 0; i < height; i++)
            for (var j = 0; j < width; j++)
                matrix[i, j] = reader.ReadUInt32();

        return new MatrixMap(height, width, matrix);
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteUInt32(Height);
        writer.WriteUInt32(Width);

        for (var i = 0; i < Height; i++)
            for (var j = 0; j < Width; j++)
                writer.WriteUInt32(Matrix[i, j]);
    }
}