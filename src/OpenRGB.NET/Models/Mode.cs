using System;

namespace OpenRGB.NET;

/// <summary>
///     Mode class containing the parameters one mode has.
/// </summary>
public class Mode
{
    internal Mode(ProtocolVersion protocolVersion, int index, string name, int value, ModeFlags flags, uint speedMin,
        uint speedMax, uint brightnessMin, uint brightnessMax, uint colorMin, uint colorMax, uint speed, 
        uint brightness, Direction direction, ColorMode colorMode, Color[] colors)
    {
        ProtocolVersion = protocolVersion;
        Index = index;
        Name = name;
        Value = value;
        Flags = flags;
        SpeedMin = SupportsSpeed ? speedMin : 0;
        SpeedMax = SupportsSpeed ? speedMax : 0;
        BrightnessMin = SupportsBrightness ? brightnessMin : 0;
        BrightnessMax = SupportsBrightness ? brightnessMax : 0;
        ColorMin = Flags.HasFlag(ModeFlags.HasModeSpecificColor) ? colorMin : 0;
        ColorMax = Flags.HasFlag(ModeFlags.HasModeSpecificColor) ? colorMax : 0;
        Speed = speed;
        Brightness = brightness;
        Direction = SupportsDirection ? direction : Direction.None;
        ColorMode = colorMode;
        Colors = colors;
    }

    /// <summary>
    ///     The version the mode was decoded in.
    /// </summary>
    public ProtocolVersion ProtocolVersion { get; }

    /// <summary>
    ///   The index of the mode.
    /// </summary>
    public int Index { get; }

    /// <summary>
    ///     The name of the mode.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Device specific value for this mode. Most likely not useful for the client.
    /// </summary>
    public int Value { get; }

    /// <summary>
    ///     Flags containing the features this mode supports.
    /// </summary>
    public ModeFlags Flags { get; }

    /// <summary>
    ///     The minimum speed value this mode supports.
    /// </summary>
    public uint SpeedMin { get; }

    /// <summary>
    ///     The maximum speed value this mode supports.
    /// </summary>
    public uint SpeedMax { get; }

    /// <summary>
    ///     The minimum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMin { get; }

    /// <summary>
    ///     The maximum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMax { get; }

    /// <summary>
    ///     The minimum number of colors this mode supports.
    /// </summary>
    public uint ColorMin { get; }

    /// <summary>
    ///     The maximum number of colors this mode supports.
    /// </summary>
    public uint ColorMax { get; }

    /// <summary>
    ///     The current speed value of this mode.
    /// </summary>
    public uint Speed { get; private set; }

    /// <summary>
    ///     The current brightness value of this mode.
    /// </summary>
    public uint Brightness { get; private set; }

    /// <summary>
    ///     The current direction of this mode.
    /// </summary>
    public Direction Direction { get; private set; }

    /// <summary>
    ///     Mode representing how the Colors are used for effects.
    /// </summary>
    public ColorMode ColorMode { get; }

    /// <summary>
    ///     The colors this mode uses for lighting.
    /// </summary>
    public Color[] Colors { get; private set; }
    
    /// <summary>
    ///     Whether this mode supports speed.
    /// </summary>
    public bool SupportsSpeed => Flags.HasFlag(ModeFlags.HasSpeed);
    
    /// <summary>
    ///     Whether this mode supports brightness.
    /// </summary>
    public bool SupportsBrightness => Flags.HasFlag(ModeFlags.HasBrightness);
    
    /// <summary>
    ///     Whether this mode supports direction.
    /// </summary>
    public bool SupportsDirection => Flags.HasFlag(ModeFlags.HasDirectionHV) || Flags.HasFlag(ModeFlags.HasDirectionUD) || Flags.HasFlag(ModeFlags.HasDirectionLR);

    /// <summary>
    ///   Sets the speed of the mode.
    /// </summary>
    public void SetSpeed(uint newSpeed)
    {
        if (!SupportsSpeed)
            throw new InvalidOperationException("This mode does not support speed.");
           
        Speed = newSpeed;
    }

    /// <summary>
    ///  Sets the brightness of the mode. 
    /// </summary>
    public void SetBrightness(uint newBrightness)
    {
        if (!SupportsBrightness)
            throw new InvalidOperationException("This mode does not support brightness.");
        
        Brightness = newBrightness;
    }
    
    /// <summary>
    ///  Sets the direction of the mode.
    /// </summary>
    public void SetDirection(Direction newDirection)
    {
        if (!SupportsDirection)
            throw new InvalidOperationException("This mode does not support direction.");
        
        Direction = newDirection;
    }

    /// <summary>
    /// Sets the colors of the mode.
    /// </summary>
    public void SetColors(Color[] newColors)
    {
        Colors = newColors;
    }
}