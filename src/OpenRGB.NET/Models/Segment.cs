using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Represents a segment of a Zone.
/// </summary>
public class Segment
{
    private Segment(int index, string name, ZoneType type, uint start, uint ledCount)
    {
        Index = index;
        Name = name;
        Type = type;
        Start = start;
        LedCount = ledCount;
    }

    /// <summary>
    ///  The index of the segment.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    ///     The name of the segment chosen by the user.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The type of the zone the segment is part of.
    /// </summary>
    public ZoneType Type { get; }

    /// <summary>
    ///     The index of the first LED in the segment.
    /// </summary>
    public uint Start { get; }

    /// <summary>
    ///     The number of LEDs in the segment.
    /// </summary>
    public uint LedCount { get; }

    private static Segment ReadFrom(ref SpanReader reader, int index)
    {
        var name = reader.ReadLengthAndString();
        var type = (ZoneType)reader.Read<uint>();
        var start = reader.Read<uint>();
        var ledCount = reader.Read<uint>();

        return new Segment(index, name, type, start, ledCount);
    }

    internal static Segment[] ReadManyFrom(ref SpanReader reader, ushort segmentCount)
    {
        var segments = new Segment[segmentCount];

        for (var i = 0; i < segmentCount; i++)
            segments[i] = ReadFrom(ref reader, i);

        return segments;
    }
}