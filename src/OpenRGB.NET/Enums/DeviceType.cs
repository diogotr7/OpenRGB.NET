namespace OpenRGB.NET;

/// <summary>
///     Enum representing the various device types supported.
/// </summary>
public enum DeviceType : uint
{
    /// <summary>
    ///     Motherboard device type.
    /// </summary>
    Motherboard,

    /// <summary>
    ///     RAM device type.
    /// </summary>
    Dram,

    /// <summary>
    ///     GPU device type.
    /// </summary>
    Gpu,

    /// <summary>
    ///     CPU device type.
    /// </summary>
    Cooler,

    /// <summary>
    ///     LED strip device type.
    /// </summary>
    Ledstrip,

    /// <summary>
    ///     Keyboard device type.
    /// </summary>
    Keyboard,

    /// <summary>
    ///     Mouse device type.
    /// </summary>
    Mouse,

    /// <summary>
    ///     Mousemat device type.
    /// </summary>
    Mousemat,

    /// <summary>
    ///     Headset device type.
    /// </summary>
    Headset,

    /// <summary>
    ///     Headset stand device type.
    /// </summary>
    HeadsetStand,

    /// <summary>
    ///     Gamepad device type.
    /// </summary>
    Gamepad,

    /// <summary>
    ///     Light device type.
    /// </summary>
    Light,

    /// <summary>
    ///     Speaker device type.
    /// </summary>
    Speaker,

    /// <summary>
    ///     Virtual device type.
    /// </summary>
    Virtual,

    /// <summary>
    ///     Unknown device type.
    /// </summary>
    Unknown
}