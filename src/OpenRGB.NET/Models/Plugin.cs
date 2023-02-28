using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Represents a Plugin installed on the OpenRGB server.
/// </summary>
public class Plugin
{
    private Plugin(string name, string description, string version, uint index, int sdkVersion)
    {
        Name = name;
        Description = description;
        Version = version;
        Index = index;
        SdkVersion = sdkVersion;
    }

    /// <summary>
    ///     The name of the plugin.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    ///    The description of the plugin.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    ///     The version of the plugin.
    /// </summary>
    public string Version { get; }
    
    /// <summary>
    ///     The index of the plugin.
    /// </summary>
    public uint Index { get; }
    
    /// <summary>
    ///     The SDK version of the plugin.
    /// </summary>
    public int SdkVersion { get; }
    
    private static Plugin ReadFrom(ref SpanReader reader)
    {
        var name = reader.ReadLengthAndString();
        var description = reader.ReadLengthAndString();
        var version = reader.ReadLengthAndString();
        var index = reader.ReadUInt32();
        var sdkVersion = reader.ReadInt32();

        return new Plugin(name, description, version, index, sdkVersion);
    }

    internal static Plugin[] ReadManyFrom(ref SpanReader reader, ushort count)
    {
        var plugins = new Plugin[count];

        for (var i = 0; i < count; i++)
            plugins[i] = ReadFrom(ref reader);

        return plugins;
    }
}