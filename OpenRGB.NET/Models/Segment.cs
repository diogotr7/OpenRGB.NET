using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Represents a segment of a Zone.
/// </summary>
public class Segment
{
    /// <summary>
    ///  The index of the segment.
    /// </summary>
    public int Index { get; private set; }
    
    /// <summary>
    ///     The name of the segment chosen by the user.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     The type of the zone the segment is part of.
    /// </summary>
    public ZoneType Type { get; private set; }

    /// <summary>
    ///     The index of the first LED in the segment.
    /// </summary>
    public uint Start { get; private set; }

    /// <summary>
    ///     The number of LEDs in the segment.
    /// </summary>
    public uint LedCount { get; private set; }

    internal static Segment ReadFrom(ref SpanReader reader, int index)
    {
        var segment = new Segment();

        segment.Index = index;
        segment.Name = reader.ReadLengthAndString();
        segment.Type = (ZoneType)reader.ReadUInt32();
        segment.Start = reader.ReadUInt32();
        segment.LedCount = reader.ReadUInt32();

        return segment;
    }

    internal static Segment[] ReadManyFrom(ref SpanReader reader, ushort segmentCount)
    {
        var segments = new Segment[segmentCount];

        for (var i = 0; i < segmentCount; i++)
            segments[i] = ReadFrom(ref reader, i);

        return segments;
    }
}