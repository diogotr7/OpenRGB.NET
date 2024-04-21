namespace OpenRGB.NET;

/// <summary>
///     Zone class containing the name, type and size of a Zone.
/// </summary>
public class Zone
{
    internal Zone(int index, int deviceIndex, string name, ZoneType type, uint ledCount, uint ledsMin, uint ledsMax, MatrixMap? matrixMap, Segment[] segments)
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
}