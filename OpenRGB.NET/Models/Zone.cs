using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Zone class containing the name, type and size of a Zone.
/// </summary>
public class Zone
{
    /// <summary>
    ///     The owning OpenRGBClient of the device.
    /// </summary>
    public IOpenRgbClient Client { get; private set; }

    /// <summary>
    ///     The index of the zone.
    /// </summary>
    public int Index { get; private set; }

    /// <summary>
    ///     The index of the zone's parent device
    /// </summary>
    public int DeviceIndex { get; private set; }

    /// <summary>
    ///     The name of the zone.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    ///     The type of the zone.
    /// </summary>
    public ZoneType Type { get; private set; }

    /// <summary>
    ///     How many leds the zone has.
    /// </summary>
    public uint LedCount { get; private set; }

    /// <summary>
    ///     Minimum number of leds in the zone
    /// </summary>
    public uint LedsMin { get; private set; }

    /// <summary>
    ///     Maximum number of leds in the zone
    /// </summary>
    public uint LedsMax { get; private set; }

    /// <summary>
    ///     A 2d Matrix containing the LED positions on the zone. Will be null if ZoneType is not ZoneType.MatrixMap
    /// </summary>
    public MatrixMap MatrixMap { get; private set; }

    /// <summary>
    ///     A list of segments in the zone. Will be null if protocol version is below 4.
    /// </summary>
    public Segment[] Segments { get; private set; }

    internal static Zone ReadFrom(ref SpanReader reader, int deviceIndex, int zoneIndex, IOpenRgbClient client,
        ProtocolVersion protocolVersion)
    {
        var zone = new Zone
        {
            Client = client,
            DeviceIndex = deviceIndex,
            Index = zoneIndex,
            Name = reader.ReadLengthAndString(),
            Type = (ZoneType)reader.ReadUInt32(),
            LedsMin = reader.ReadUInt32(),
            LedsMax = reader.ReadUInt32(),
            LedCount = reader.ReadUInt32()
        };

        var zoneMatrixLength = reader.ReadUInt16();

        zone.MatrixMap = zoneMatrixLength > 0 ? MatrixMap.ReadFrom(ref reader) : null;

        if (protocolVersion.SupportsSegments)
        {
            var segmentCount = reader.ReadUInt16();
            zone.Segments = Segment.ReadManyFrom(ref reader, segmentCount);
        }

        return zone;
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