using System;

namespace OpenRGB.NET;

/// <summary>
///     Flags representing the capabilities a mode can have.
/// </summary>
[Flags]
public enum ModeFlags : uint
{
    /// <summary>
    ///     This mode has no capabilities.
    /// </summary>
    None = 0,

    /// <summary>
    ///     This mode has a speed parameter.
    /// </summary>
    HasSpeed = 1 << 0,

    /// <summary>
    ///     This Mode has a Left-Right direction parameter.
    /// </summary>
    HasDirectionLR = 1 << 1,

    /// <summary>
    ///     This Mode has an Up-Down direction parameter.
    /// </summary>
    HasDirectionUD = 1 << 2,

    /// <summary>
    ///     This Mode has an Horizontal-Vertical direction parameter.
    /// </summary>
    HasDirectionHV = 1 << 3,

    /// <summary>
    ///     This Mode has a brightness parameter.
    /// </summary>
    HasBrightness = 1 << 4,

    /// <summary>
    ///     This Mode has per-LED colors.
    /// </summary>
    HasPerLedColor = 1 << 5,

    /// <summary>
    ///     This Mode has mode specific colors.
    /// </summary>
    HasModeSpecificColor = 1 << 6,

    /// <summary>
    ///     This Mode has a random color option.
    /// </summary>
    HasRandomColor = 1 << 7,
}