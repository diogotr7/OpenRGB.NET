namespace OpenRGB.NET;

/// <summary>
///     Device class containing all the info present in an OpenRGB RGBController
/// </summary>
public class Device
{
    internal Device(int index, DeviceType type, string name, string? vendor,
        string description, string version, string serial, string location, int activeModeIndex,
        Mode[] modes, Zone[] zones, Led[] leds, Color[] colors)
    {
        Index = index;
        Type = type;
        Name = name;
        Vendor = vendor;
        Description = description;
        Version = version;
        Serial = serial;
        Location = location;
        ActiveModeIndex = activeModeIndex;
        Modes = modes;
        Zones = zones;
        Leds = leds;
        Colors = colors;
    }

    /// <summary>
    ///     The index of the device.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The type of the device.
    /// </summary>
    public DeviceType Type { get; }

    /// <summary>
    ///     The name of the device.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     The vendor of the device. Will be null on protocol versions below 1.
    /// </summary>
    public string? Vendor { get; }

    /// <summary>
    ///     The description of device.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     The version of the device. Usually a firmware version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    ///     The serial number of the device.
    /// </summary>
    public string Serial { get; }

    /// <summary>
    ///     The location of the device. Usually the device file on the filesystem.
    /// </summary>
    public string Location { get; }

    /// <summary>
    ///     The index of the currently active mode on the device.
    /// </summary>
    public int ActiveModeIndex { get; }

    /// <summary>
    ///     The modes the device can be set to.
    /// </summary>
    public Mode[] Modes { get; }

    /// <summary>
    ///     The lighting zones present on the device.
    /// </summary>
    public Zone[] Zones { get; }

    /// <summary>
    ///     All the leds present on the device.
    /// </summary>
    public Led[] Leds { get; }

    /// <summary>
    ///     The colors of all the leds present on the device.
    /// </summary>
    public Color[] Colors { get; }

    /// <summary>
    ///     Shortcut for Modes[ActiveModeIndex], returns the currently active mode.
    /// </summary>
    public Mode ActiveMode => Modes[ActiveModeIndex];

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{Type}: {Name}";
    }
}
