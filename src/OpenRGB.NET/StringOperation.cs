using System.Runtime.CompilerServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct None : ISpanWritable
{
    public int Length => 0;
    public void WriteTo(ref SpanWriter writer) { }
}

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
        var duplicatePacketLength = reader.Read<uint>();

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

internal readonly struct OpenRgbString(string value) : ISpanWritable, ISpanReader<string>
{
    public string Value { get; } = value;
    public int Length => Value.Length + 1;
    public void WriteTo(ref SpanWriter writer) => writer.Write(Value);
    public static string ReadFrom(ref SpanReader reader, ProtocolVersion protocolVersion = default, int index = default) => reader.ReadLengthAndString();
    public static implicit operator string(OpenRgbString openRgbString) => openRgbString.Value;
    public static implicit operator OpenRgbString(string value) => new(value);
}

internal readonly struct Primitive<T>(T value) : ISpanWritable where T : unmanaged
{
    public T Value { get; } = value;
    public int Length => Unsafe.SizeOf<T>();
    public void WriteTo(ref SpanWriter writer) => writer.Write(Value);
    public static implicit operator T(Primitive<T> operation) => operation.Value;
    public static implicit operator Primitive<T> (T value) => new(value);
}

internal readonly struct PrimitiveReader<T> : ISpanReader<Primitive<T>> where T : unmanaged
{
    public static Primitive<T> ReadFrom(ref SpanReader reader, ProtocolVersion protocolVersion = default, int index = default) => new(reader.Read<T>());
}

internal readonly struct Args<T1, T2> : ISpanWritable where T1 : ISpanWritable where T2 : ISpanWritable
{
    public T1 Arg1 { get; }
    public T2 Arg2 { get; }

    public Args(T1 arg1, T2 arg2)
    {
        Arg1 = arg1;
        Arg2 = arg2;
    }

    public int Length => Arg1.Length + Arg2.Length;

    public void WriteTo(ref SpanWriter writer)
    {
        Arg1.WriteTo(ref writer);
        Arg2.WriteTo(ref writer);
    }
}

internal readonly struct Args<T1, T2, T3> : ISpanWritable where T1 : ISpanWritable where T2 : ISpanWritable where T3 : ISpanWritable
{
    public T1 Arg1 { get; }
    public T2 Arg2 { get; }
    public T3 Arg3 { get; }

    public Args(T1 arg1, T2 arg2, T3 arg3)
    {
        Arg1 = arg1;
        Arg2 = arg2;
        Arg3 = arg3;
    }

    public int Length => Arg1.Length + Arg2.Length + Arg3.Length;

    public void WriteTo(ref SpanWriter writer)
    {
        Arg1.WriteTo(ref writer);
        Arg2.WriteTo(ref writer);
        Arg3.WriteTo(ref writer);
    }
}
