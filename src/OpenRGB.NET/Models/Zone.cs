using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Zone class containing the name, type and size of a Zone.
/// </summary>
public class Zone
{
    private Zone(int index, int deviceIndex, string name, ZoneType type, uint ledCount, uint ledsMin, uint ledsMax, MatrixMap? matrixMap, Segment[] segments)
    {
        Index = index;
        DeviceIndex = deviceIndex;
        Name = name;
        Type = type;
        LedCount = ledCount;
        LedsMin = ledsMin;
        LedsMax = ledsMax;
        MatrixMap = matrixMap;
        Segments = segments;
    }

    /// <summary>
    ///     The index of the zone.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The index of the zone's parent device
    /// </summary>
    public int DeviceIndex { get; }

    /// <summary>
    ///     The name of the zone.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The type of the zone.
    /// </summary>
    public ZoneType Type { get; }

    /// <summary>
    ///     How many leds the zone has.
    /// </summary>
    public uint LedCount { get; }

    /// <summary>
    ///     Minimum number of leds in the zone
    /// </summary>
    public uint LedsMin { get; }

    /// <summary>
    ///     Maximum number of leds in the zone
    /// </summary>
    public uint LedsMax { get; }

    /// <summary>
    ///     A 2d Matrix containing the LED positions on the zone. Will be null if ZoneType is not ZoneType.MatrixMap
    /// </summary>
    public MatrixMap? MatrixMap { get; }

    /// <summary>
    ///     A list of segments in the zone. Will be null if protocol version is below 4.
    /// </summary>
    public Segment[] Segments { get; }

    private static Zone ReadFrom(ref SpanReader reader, int deviceIndex, int zoneIndex, ProtocolVersion protocolVersion)
    {
        var name = reader.ReadLengthAndString();
        var type = (ZoneType)reader.Read<uint>();
        var ledsMin = reader.Read<uint>();
        var ledsMax = reader.Read<uint>();
        var ledCount = reader.Read<uint>();
        var zoneMatrixLength = reader.Read<ushort>();
        var matrixMap = zoneMatrixLength > 0 ? MatrixMap.ReadFrom(ref reader) : null;
        var segments = protocolVersion.SupportsSegmentsAndPlugins ? Segment.ReadManyFrom(ref reader, reader.Read<ushort>()) : [];

        return new Zone(zoneIndex, deviceIndex, name, type, ledCount, ledsMin, ledsMax, matrixMap, segments);
    }

    internal static Zone[] ReadManyFrom(ref SpanReader reader, ushort zoneCount, int deviceID, ProtocolVersion protocolVersion)
    {
        var zones = new Zone[zoneCount];

        for (var i = 0; i < zoneCount; i++)
            zones[i] = ReadFrom(ref reader, deviceID, i, protocolVersion);

        return zones;
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteLengthAndString(Name);
        writer.Write((uint)Type);
        writer.Write(LedsMin);
        writer.Write(LedsMax);
        writer.Write(LedCount);
        writer.Write((ushort)(MatrixMap?.Length ?? 0));
        MatrixMap?.WriteTo(ref writer);
    }
}