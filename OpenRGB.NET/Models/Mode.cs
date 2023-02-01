using System;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Mode class containing the parameters one mode has.
/// </summary>
public class Mode
{
    /// <summary>
    ///     The version the mode was decoded in.
    /// </summary>
    public ProtocolVersion ProtocolVersion { get; private init; }

    /// <summary>
    ///   The index of the mode.
    /// </summary>
    public int Index { get; private init; }

    /// <summary>
    ///     The name of the mode.
    /// </summary>
    public string Name { get; private init; }

    /// <summary>
    ///     Device specific value for this mode. Most likely not useful for the client.
    /// </summary>
    public int Value { get; private init; }

    /// <summary>
    ///     Flags containing the features this mode supports.
    /// </summary>
    public ModeFlags Flags { get; private init; }

    /// <summary>
    ///     The minimum speed value this mode supports.
    /// </summary>
    public uint SpeedMin { get; private init; }

    /// <summary>
    ///     The maximum speed value this mode supports.
    /// </summary>
    public uint SpeedMax { get; private init; }

    /// <summary>
    ///     The minimum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMin { get; private init; }

    /// <summary>
    ///     The maximum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMax { get; private init; }

    /// <summary>
    ///     The minimum number of colors this mode supports.
    /// </summary>
    public uint ColorMin { get; private init; }

    /// <summary>
    ///     The maximum number of colors this mode supports.
    /// </summary>
    public uint ColorMax { get; private init; }

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
    public ColorMode ColorMode { get; private init; }

    /// <summary>
    ///     The colors this mode uses for lighting.
    /// </summary>
    public Color[] Colors { get; private set; }

    /// <summary>
    ///     Determines if the feature is supported in the flags.
    /// </summary>
    public bool HasFlag(ModeFlags flag)
    {
        return (Flags & flag) != 0;
    }

    /// <summary>
    ///   Sets the speed of the mode.
    /// </summary>
    public void SetSpeed(uint newSpeed)
    {
        if (!HasFlag(ModeFlags.HasSpeed))
            throw new InvalidOperationException("This mode does not support speed.");
           
        Speed = newSpeed;
    }

    /// <summary>
    ///  Sets the brightness of the mode. 
    /// </summary>
    public void SetBrightness(uint newBrightness)
    {
        if (!HasFlag(ModeFlags.HasBrightness))
            throw new InvalidOperationException("This mode does not support brightness.");
        
        Brightness = newBrightness;
    }
    
    /// <summary>
    ///  Sets the direction of the mode.
    /// </summary>
    public void SetDirection(Direction newDirection)
    {
        if (!HasFlag(ModeFlags.HasDirectionHV) && !HasFlag(ModeFlags.HasDirectionUD) && !HasFlag(ModeFlags.HasDirectionLR))
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

    internal static Mode ReadFrom(ref SpanReader reader, ProtocolVersion protocolVersion, int index)
    {
        var name = reader.ReadLengthAndString();
        var modeValue = reader.ReadInt32();
        var modeFlags = (ModeFlags)reader.ReadUInt32();
        var speedMin = reader.ReadUInt32();
        var speedMax = reader.ReadUInt32();
        var brightMin = protocolVersion.SupportsBrightnessAndSaveMode ? reader.ReadUInt32() : 0;
        var brightMax = protocolVersion.SupportsBrightnessAndSaveMode ? reader.ReadUInt32() : 0;
        var colorMin = reader.ReadUInt32();
        var colorMax = reader.ReadUInt32();
        var speed = reader.ReadUInt32();
        var brightness = protocolVersion.SupportsBrightnessAndSaveMode ? reader.ReadUInt32() : 0;
        var direction = reader.ReadUInt32();
        var colorMode = (ColorMode)reader.ReadUInt32();
        var colorCount = reader.ReadUInt16();
        var colors = Color.ReadManyFrom(ref reader, colorCount);

        return new Mode()
        {
            ProtocolVersion = protocolVersion,
            Index = index,
            Name = name,
            Value = modeValue,
            Flags = modeFlags,
            SpeedMin = modeFlags.HasFlag(ModeFlags.HasSpeed) ? speedMin : 0,
            SpeedMax = modeFlags.HasFlag(ModeFlags.HasSpeed) ? speedMax : 0,
            BrightnessMin = modeFlags.HasFlag(ModeFlags.HasBrightness) ? brightMin : 0,
            BrightnessMax = modeFlags.HasFlag(ModeFlags.HasBrightness) ? brightMax : 0,
            ColorMin = modeFlags.HasFlag(ModeFlags.HasModeSpecificColor) ? colorMin : 0,
            ColorMax = modeFlags.HasFlag(ModeFlags.HasModeSpecificColor) ? colorMax : 0,
            Speed = modeFlags.HasFlag(ModeFlags.HasSpeed) ? speed : 0,
            Brightness = modeFlags.HasFlag(ModeFlags.HasBrightness) ? brightness : 0,
            Direction = modeFlags.HasFlag(ModeFlags.HasDirectionHV)
                        || modeFlags.HasFlag(ModeFlags.HasDirectionLR)
                        || modeFlags.HasFlag(ModeFlags.HasDirectionUD)
                ? (Direction)direction
                : Direction.None,
            ColorMode = colorMode,
            Colors = colors
        };
    }

    internal static Mode[] ReadManyFrom(ref SpanReader reader, ushort numModes, ProtocolVersion protocolVersion)
    {
        var modes = new Mode[numModes];

        for (var i = 0; i < numModes; i++)
            modes[i] = ReadFrom(ref reader, protocolVersion, i);

        return modes;
    }

    internal void WriteTo(ref SpanWriter writer)
    {
        writer.WriteLengthAndString(Name);
        writer.WriteInt32(Value);
        writer.WriteUInt32((uint)Flags);
        writer.WriteUInt32(SpeedMin);
        writer.WriteUInt32(SpeedMax);

        if (ProtocolVersion.SupportsBrightnessAndSaveMode)
        {
            writer.WriteUInt32(BrightnessMin);
            writer.WriteUInt32(BrightnessMax);
        }

        writer.WriteUInt32(ColorMin);
        writer.WriteUInt32(ColorMax);
        writer.WriteUInt32(Speed);

        if (ProtocolVersion.SupportsBrightnessAndSaveMode)
            writer.WriteUInt32(Brightness);

        writer.WriteUInt32((uint)Direction);
        writer.WriteUInt32((uint)ColorMode);
        writer.WriteUInt16((ushort)Colors.Length);

        foreach (var color in Colors)
            color.WriteTo(ref writer);
    }

    internal uint GetLength()
    {
        var size = (uint)(
            sizeof(int) * 2 +
            sizeof(uint) * 9 +
            sizeof(ushort) * 2 +
            sizeof(uint) * Colors.Length +
            Name.Length + 1);

        if (ProtocolVersion.SupportsBrightnessAndSaveMode) size += sizeof(uint) * 3;

        return size;
    }
}