namespace OpenRGB.NET;

/// <summary>
///     Enum representing the different types a zone can be.
/// </summary>
public enum ZoneType : uint
{
    /// <summary>
    ///     This zone represents one LED.
    /// </summary>
    Single,

    /// <summary>
    ///     This zone represents s sequence of LEDs in a line, like an LED strip.
    /// </summary>
    Linear,

    /// <summary>
    ///     This zone represents a matrix of LEDs in a grid, like a keyboard.
    /// </summary>
    Matrix
}