namespace OpenRGB.NET;

/// <summary>
///     Enum representing how a specific Mode uses
///     the colors assigned to it.
/// </summary>
public enum ColorMode : uint
{
    /// <summary>
    ///     The mode does not use any color.
    /// </summary>
    None,

    /// <summary>
    ///     The mode sets each led to a specific color.
    /// </summary>
    PerLed,

    /// <summary>
    ///     The mode uses one color, specific to the effect.
    /// </summary>
    ModeSpecific,

    /// <summary>
    ///     The mode has no set color, uses random colors instead.
    /// </summary>
    Random
}