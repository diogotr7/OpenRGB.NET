using System;

namespace OpenRGB.NET.Enums
{
    /// <summary>
    /// Flags representing the capabilities a mode can have.
    /// </summary>
    [Flags]
    public enum ModeFlags
    {
        /// <summary>
        /// This mode has no capabilities.
        /// </summary>
        None = 0,

        /// <summary>
        /// This mode has a speed parameter.
        /// </summary>
        HasSpeed = (1 << 0),

        /// <summary>
        /// This Mode has a Left-Right direction paramenter.
        /// </summary>
        HasDirectionLR = (1 << 1),

        /// <summary>
        /// This Mode has an Up-Down direction paramenter.
        /// </summary>
        HasDirectionUD = (1 << 2),

        /// <summary>
        /// This Mode has an Horizontal-Vertical direction paramenter.
        /// </summary>
        HasDirectionHV = (1 << 3),

        /// <summary>
        /// This Mode has a brightness paramenter.
        /// </summary>
        HasBrightness = (1 << 4),

        /// <summary>
        /// This Mode has per-LED colors.
        /// </summary>
        HasPerLedColor = (1 << 5),

        /// <summary>
        /// This Mode has mode specific colors.
        /// </summary>
        HasModeSpecificColor = (1 << 6),

        /// <summary>
        /// This Mode has a random color option.
        /// </summary>
        HasRandomColor = (1 << 7),

        /// <summary>
        /// This mode has a direction parameter in any orientation.
        /// </summary>
        HasDirection = HasDirectionLR & HasDirectionUD & HasDirectionHV
    }
}
