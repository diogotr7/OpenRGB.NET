using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct MatrixMapReader : ISpanReader<MatrixMap>
{
    public static MatrixMap ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default,
        int? outerCount = default)
    {
        var height = reader.Read<uint>();
        var width = reader.Read<uint>();
        var matrix = new uint[height, width];

        for (var i = 0; i < height; i++)
        {
            for (var j = 0; j < width; j++)
            {
                matrix[i, j] = reader.Read<uint>();
            }
        }

        return new MatrixMap(height, width, matrix);
    }
}