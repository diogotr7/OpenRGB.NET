using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal readonly struct PluginsReader : ISpanReader<Plugin[]>
{
    public static Plugin[] ReadFrom(ref SpanReader reader, ProtocolVersion? p = default, int? i = default, int? outerCount = default)
    {
        // ReSharper disable once UnusedVariable
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