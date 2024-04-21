namespace OpenRGB.NET;

/// <summary>
///     Represents a Plugin installed on the OpenRGB server.
/// </summary>
public class Plugin
{
    internal Plugin(string name, string description, string version, uint index, int sdkVersion)
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
}