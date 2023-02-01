using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
/// Matrix Map class for the matrix Zone type
/// </summary>
public class MatrixMap
{
    /// <summary>
    /// The height of the matrix.
    /// </summary>
    public uint Height { get; private set; }

    /// <summary>
    /// The width of the matrix.
    /// </summary>
    public uint Width { get; private set; }

    /// <summary>
    /// The matrix.
    /// </summary>
    public uint[,] Matrix { get; private set; }

    internal uint Length => Height * Width * 4 + 8;

    /// <summary>
    /// Decodes a byte array into a matrix map
    /// </summary>
    /// <param name="reader"></param>
    internal static MatrixMap ReadFrom(ref SpanReader reader)
    {
        var matx = new MatrixMap();

        matx.Height = reader.ReadUInt32();

        matx.Width = reader.ReadUInt32();

        matx.Matrix = new uint[matx.Height, matx.Width];

        for (int i = 0; i < matx.Height; i++)
        {
            for (int j = 0; j < matx.Width; j++)
            {
                matx.Matrix[i, j] = reader.ReadUInt32();
            }
        }

        return matx;
    }
        
    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteUInt32(Height);
        writer.WriteUInt32(Width);

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                writer.WriteUInt32(Matrix[i, j]);
            }
        }
    }
}