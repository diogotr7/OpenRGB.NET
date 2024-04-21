using System;

namespace OpenRGB.NET;

/// <summary>
///     Interface for an OpenRGB SDK client.
/// </summary>
public interface IOpenRgbClient
{
    /// <summary>
    ///     Represents the connection status of the socket to the server
    /// </summary>
    bool Connected { get; }

    /// <summary>
    ///     The maximum protocol version this implementation supports
    /// </summary>
    ProtocolVersion MaxSupportedProtocolVersion { get; }
    
    /// <summary>
    ///     The protocol version to be used by this instance of <see cref="IOpenRgbClient" />
    /// </summary>
    ProtocolVersion ClientProtocolVersion { get; }

    /// <summary>
    ///     The minimum common protocol version between this client and the connected server. Only set after the first
    ///     <see cref="Connect" />
    /// </summary>
    ProtocolVersion CommonProtocolVersion { get; }

    /// <summary>
    ///     Triggered when the device list updates
    /// </summary>
    event EventHandler<EventArgs>? DeviceListUpdated;

    /// <summary>
    ///     Connects manually to the server. Only needs to be called if the constructor was called
    ///     with auto-connect set to false.
    /// </summary>
    void Connect();
    
    /// <summary>
    ///     Requests the controller count from the server.
    /// </summary>
    /// <returns>The amount of controllers.</returns>
    int GetControllerCount();
    
    /// <summary>
    ///     Requests the data block for a given controller index.
    /// </summary>
    /// <param name="deviceId">The index of the controller to request the data from.</param>
    /// <returns>The Device containing the decoded data for the controller with the given id.</returns>
    Device GetControllerData(int deviceId);

    /// <summary>
    ///     Requests the data for all the controllers detected by the server.
    /// </summary>
    /// <returns>An array with the information for all the devices.</returns>
    Device[] GetAllControllerData();

    /// <summary>
    ///     Requests existing profiles on the server
    /// </summary>
    /// <returns>An array with profile names</returns>
    string[] GetProfiles();

    /// <summary>
    ///     Loads the provided profile on the server
    /// </summary>
    /// <param name="profile">Name of the profile to load</param>
    void LoadProfile(string profile);

    /// <summary>
    ///     Deletes the provided profile on the server
    /// </summary>
    /// <param name="profile"> Name of the profile to delete</param>
    void DeleteProfile(string profile);

    /// <summary>
    ///     Saves the current state as a profile with the provided name
    /// </summary>
    /// <param name="profile"> Name of the profile to save</param>
    void SaveProfile(string profile);
    
    /// <summary>
    ///     Requests the list of plugins from the server.
    /// </summary>
    /// <returns></returns>
    Plugin[] GetPlugins();
    
    /// <summary>
    ///     Resizes the given zone to the given size.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="zoneId"></param>
    /// <param name="size"></param>
    void ResizeZone(int deviceId, int zoneId, int size);
    
    /// <summary>
    ///     Updates the LEDs for the given device.
    ///     Make sure the array has the correct number of LEDs.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="colors"></param>
    void UpdateLeds(int deviceId, ReadOnlySpan<Color> colors);
    
    /// <summary>
    ///     Updates the LEDs of a given device and zone.
    ///     Make sure the array has the correct number of LEDs for the zone.
    /// </summary>
    /// <param name="deviceId"></param>
    /// <param name="zoneId"></param>
    /// <param name="colors"></param>
    void UpdateZoneLeds(int deviceId, int zoneId, ReadOnlySpan<Color> colors);
    
    /// <summary>
    ///     Updates a single LED of a given device.
    /// </summary>
    /// <param name="deviceId">The id of the device to update</param>
    /// <param name="ledId">which led to update on the device</param>
    /// <param name="color">what color to set the led to</param>
    void UpdateSingleLed(int deviceId, int ledId, Color color);
    
    /// <summary>
    ///     Sets the mode of the specified device to "Custom".
    /// </summary>
    /// <param name="deviceId"></param>
    void SetCustomMode(int deviceId);

    /// <summary>
    ///     Sets the specified mode on the specified device.
    ///     Any optional parameters not set will be left as received from the server.
    /// </summary>
    /// <param name="deviceId">The id of the device to update</param>
    /// <param name="modeId">The id of the zone on the device yp update</param>
    /// <param name="speed">The optional speed value to set.</param>
    /// <param name="direction">The optional direction value to set</param>
    /// <param name="colors">The optional colors value to set.</param>
    void UpdateMode(int deviceId, int modeId, uint? speed = null, Direction? direction = null, Color[]? colors = default);

    /// <summary>
    ///     Sets the specified mode on the specified device and zone.
    /// </summary>
    /// <param name="deviceId">The id of the device to update</param>
    /// <param name="modeId">The id of the mode on the device to update</param>
    void SaveMode(int deviceId, int modeId);

    /// <summary>
    ///     Sends plugin-specific data to the server.
    /// </summary>
    /// <param name="pluginId">The id of the plugin to send the data to</param>
    /// <param name="pluginPacketType">The type of data to send to the plugin</param>
    /// <param name="data">The data sent to the plugin</param>
    void PluginSpecific(int pluginId, int pluginPacketType, ReadOnlySpan<byte> data);
}