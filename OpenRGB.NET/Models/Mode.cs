using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
/// Mode class containing the parameters one mode has.
/// </summary>
public class Mode
{
    /// <summary>
    /// The version the mode was decoded in.
    /// </summary>
    public ProtocolVersion ProtocolVersion { get; private set; }
    
    /// <summary>
    /// The name of the mode.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Device specific value for this mode. Most likely not useful for the client.
    /// </summary>
    public int Value { get; private set; }

    /// <summary>
    /// Flags containing the features this mode supports.
    /// </summary>
    public ModeFlags Flags { get; private set; }

    /// <summary>
    /// The minimum speed value this mode supports.
    /// </summary>
    public uint SpeedMin { get; private set; }

    /// <summary>
    /// The maximum speed value this mode supports.
    /// </summary>
    public uint SpeedMax { get; private set; }

    /// <summary>
    /// The minimum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMin { get; private set; }

    /// <summary>
    /// The maximum brightness value this mode supports.
    /// </summary>
    public uint BrightnessMax { get; private set; }

    /// <summary>
    /// The minimum number of colors this mode supports.
    /// </summary>
    public uint ColorMin { get; private set; }

    /// <summary>
    /// The maximum number of colors this mode supports.
    /// </summary>
    public uint ColorMax { get; private set; }

    /// <summary>
    /// The current speed value of this mode.
    /// </summary>
    public uint Speed { get; set; }

    /// <summary>
    /// The current brightness value of this mode.
    /// </summary>
    public uint Brightness { get; set; }

    /// <summary>
    /// The current direction of this mode.
    /// </summary>
    public Direction Direction { get; set; }

    /// <summary>
    /// Mode representing how the Colors are used for effects.
    /// </summary>
    public ColorMode ColorMode { get; private set; }

    /// <summary>
    /// The colors this mode uses for lighting.
    /// </summary>
    public Color[] Colors { get; set; }

    /// <summary>
    /// Determines if the feature is supported in the flags.
    /// </summary>
    public bool HasFlag(ModeFlags flag) => (Flags & flag) != 0;

    internal static Mode ReadFrom(ref SpanReader reader, ProtocolVersion protocolVersion)
    {
        var mode = new Mode();

        mode.ProtocolVersion = protocolVersion;
        
        mode.Name = reader.ReadLengthAndString();

        mode.Value = reader.ReadInt32();

        mode.Flags = (ModeFlags)reader.ReadUInt32();

        var speedMin = reader.ReadUInt32();

        var speedMax = reader.ReadUInt32();

        uint brightMin = 0;
        uint brightMax = 0;
        
        if (protocolVersion.SupportsBrightnessAndSaveMode)
        {
            brightMin = reader.ReadUInt32();
            brightMax = reader.ReadUInt32();
        }

        var colorMin = reader.ReadUInt32();

        var colorMax = reader.ReadUInt32();

        var speed = reader.ReadUInt32();

        uint brightness = 0;
        if (protocolVersion.SupportsBrightnessAndSaveMode)
        {
            brightness = reader.ReadUInt32();
        }

        var direction = reader.ReadUInt32();

        mode.ColorMode = (ColorMode)reader.ReadUInt32();

        ushort colorCount = reader.ReadUInt16();
        mode.Colors = Color.ReadManyFrom(ref reader, colorCount);

        if (mode.HasFlag(ModeFlags.HasSpeed))
        {
            mode.Speed = speed;
            mode.SpeedMin = speedMin;
            mode.SpeedMax = speedMax;
        }

        if (mode.HasFlag(ModeFlags.HasBrightness))
        {
            mode.Brightness = brightness;
            mode.BrightnessMin = brightMin;
            mode.BrightnessMax = brightMax;
        }

        if (mode.HasFlag(ModeFlags.HasModeSpecificColor))
        {
            mode.ColorMin = colorMin;
            mode.ColorMax = colorMax;
        }

        if (mode.HasFlag(ModeFlags.HasDirectionHV) ||
            mode.HasFlag(ModeFlags.HasDirectionLR) ||
            mode.HasFlag(ModeFlags.HasDirectionUD))
        {
            mode.Direction = (Direction)direction;
        }
        else
        {
            mode.Direction = Direction.None;
        }

        return mode;
    }
    
    internal static Mode[] ReadManyFrom(ref SpanReader reader, ushort numModes, ProtocolVersion protocolVersion)
    {
        var modes = new Mode[numModes];

        for (int i = 0; i < numModes; i++)
            modes[i] = ReadFrom(ref reader, protocolVersion);

        return modes;
    }

    internal void WriteTo(ref SpanWriter writer, uint protocolVersion)
    {
        writer.WriteLengthAndString(Name);
        writer.WriteInt32(Value);
        writer.WriteUInt32((uint)Flags);
        writer.WriteUInt32(SpeedMin);
        writer.WriteUInt32(SpeedMax);
        
        if (protocolVersion >= 3)
        {
            writer.WriteUInt32(BrightnessMin);
            writer.WriteUInt32(BrightnessMax);
        }
        writer.WriteUInt32(ColorMin);
        writer.WriteUInt32(ColorMax);
        writer.WriteUInt32(Speed);
        
        if (protocolVersion >= 3)
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

        if (ProtocolVersion.SupportsBrightnessAndSaveMode)
        {
            size += sizeof(uint) * 3;
        }

        return size;
    }
}