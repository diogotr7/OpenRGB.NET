using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly record struct ColorsReader : ISpanReader<Color[]>
{
    public Color[] ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default, int? outerCount = default)
    {
        var count = reader.Read<ushort>();
        var colors = new Color[count];

        for (var i = 0; i < count; i++)
        {
            var r = reader.Read<byte>();
            var g = reader.Read<byte>();
            var b = reader.Read<byte>();
            _ = reader.Read<byte>();
            
            colors[i] = new Color(r, g, b);
        }

        return colors;
    }
}