using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct SegmentsReader : ISpanReader<Segment[]>
{
    public static Segment[] ReadFrom(ref SpanReader reader, ProtocolVersion? protocolVersion = default, int? index = default,
        int? outerCount = default)
    {
        var segmentCount = reader.Read<ushort>();
        var segments = new Segment[segmentCount];

        for (var i = 0; i < segmentCount; i++)
        {
            var name = reader.ReadLengthAndString();
            var type = (ZoneType)reader.Read<uint>();
            var start = reader.Read<uint>();
            var ledCount = reader.Read<uint>();

            segments[i] = new Segment(i, name, type, start, ledCount);
        }

        return segments;
    }
}