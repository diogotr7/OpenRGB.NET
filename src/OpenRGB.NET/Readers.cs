using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct PluginsReader : ISpanReader<Plugin[]>
{
    public static Plugin[] ReadFrom(ref SpanReader reader, ProtocolVersion p = default, int i = default)
    {
        var dataSize = reader.Read<uint>();
        
        var count = reader.Read<ushort>();
        var plugins = new Plugin[count];
        for (var j = 0; j < count; j++)
        {
            var name = reader.ReadLengthAndString();
            var description = reader.ReadLengthAndString();
            var version = reader.ReadLengthAndString();
            var index = reader.Read<uint>();
            var sdkVersion = reader.Read<int>();

            plugins[j] = new Plugin(name, description, version, index, sdkVersion);
        }

        return plugins;
    }
}

internal readonly struct ProfilesReader : ISpanReader<string[]>
{
    public static string[] ReadFrom(ref SpanReader reader, ProtocolVersion p = default, int i = default)
    {
        var dataSize = reader.Read<uint>();
        
        var count = reader.Read<ushort>();
        var profiles = new string[count];
        for (var j = 0; j < count; j++)
        {
            profiles[j] = reader.ReadLengthAndString();
        }

        return profiles;
    }
}

internal readonly struct DeviceReader : ISpanReader<Device>
{
    public static Device ReadFrom(ref SpanReader reader, ProtocolVersion protocol, int deviceIndex)
    {
        var dataSize = reader.Read<uint>();

        var deviceType = reader.Read<int>();
        var name = reader.ReadLengthAndString();
        var vendor = protocol.SupportsVendorString ? reader.ReadLengthAndString() : null;
        var description = reader.ReadLengthAndString();
        var version = reader.ReadLengthAndString();
        var serial = reader.ReadLengthAndString();
        var location = reader.ReadLengthAndString();
        var modeCount = reader.Read<ushort>();
        var activeMode = reader.Read<int>();
        var modes = Mode.ReadManyFrom(ref reader, modeCount, protocol);
        var zoneCount = reader.Read<ushort>();
        var zones = Zone.ReadManyFrom(ref reader, zoneCount, deviceIndex, protocol);
        var ledCount = reader.Read<ushort>();
        var leds = Led.ReadManyFrom(ref reader, ledCount);
        var colorCount = reader.Read<ushort>();
        var colors = Color.ReadManyFrom(ref reader, colorCount);

        return new Device(deviceIndex, (DeviceType)deviceType,
            name, vendor, description, version, serial, location,
            activeMode, modes, zones, leds, colors);
    }
}

internal readonly struct ProtocolVersionReader : ISpanReader<ProtocolVersion>
{
    public static ProtocolVersion ReadFrom(ref SpanReader reader, ProtocolVersion p = default, int i = default)
    {
        return ProtocolVersion.FromNumber(reader.Read<uint>());
    }
}

internal readonly struct PrimitiveReader<T> : ISpanReader<T> where T : unmanaged
{
    public static T ReadFrom(ref SpanReader reader, ProtocolVersion protocolVersion = default, int index = default) => reader.Read<T>();
}