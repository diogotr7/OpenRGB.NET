namespace OpenRGB.NET;

/// <summary>
///     Matrix Map class for the matrix Zone type
/// </summary>
public class MatrixMap
{
    internal MatrixMap(uint height, uint width, uint[,] matrix)
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
}