using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Zone class containing the name, type and size of a Zone.
/// </summary>
public class Zone
{
    private Zone(IOpenRgbClient client, int index, int deviceIndex, string name, ZoneType type, uint ledCount, uint ledsMin, uint ledsMax, MatrixMap? matrixMap, Segment[] segments)
    {
        Client = client;
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
    ///     The owning OpenRGBClient of the device.
    /// </summary>
    public IOpenRgbClient Client { get; }

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

    private static Zone ReadFrom(ref SpanReader reader, int deviceIndex, int zoneIndex, IOpenRgbClient client,
        ProtocolVersion protocolVersion)
    {
        var name = reader.ReadLengthAndString();
        var type = (ZoneType)reader.ReadUInt32();
        var ledsMin = reader.ReadUInt32();
        var ledsMax = reader.ReadUInt32();
        var ledCount = reader.ReadUInt32();
        var zoneMatrixLength = reader.ReadUInt16();
        var matrixMap = zoneMatrixLength > 0 ? MatrixMap.ReadFrom(ref reader) : null;
        var segments = protocolVersion.SupportsSegments ? Segment.ReadManyFrom(ref reader, reader.ReadUInt16()) : Array.Empty<Segment>();

        return new Zone(client, zoneIndex, deviceIndex, name, type, ledCount, ledsMin, ledsMax, matrixMap, segments);
    }

    internal static Zone[] ReadManyFrom(ref SpanReader reader, ushort zoneCount, IOpenRgbClient client, int deviceID,
        ProtocolVersion protocolVersion)
    {
        var zones = new Zone[zoneCount];

        for (var i = 0; i < zoneCount; i++)
            zones[i] = ReadFrom(ref reader, deviceID, i, client, protocolVersion);

        return zones;
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteLengthAndString(Name);
        writer.WriteUInt32((uint)Type);
        writer.WriteUInt32(LedsMin);
        writer.WriteUInt32(LedsMax);
        writer.WriteUInt32(LedCount);
        writer.WriteUInt16((ushort)(MatrixMap?.Length ?? 0));
        MatrixMap?.WriteTo(ref writer);
    }

    /// <summary>
    ///     Calls UpdateZone(DeviceID, ID, colors) on the corresponding client.
    /// </summary>
    public void Update(Color[] colors)
    {
        Client.UpdateZone(DeviceIndex, Index, colors);
    }
}