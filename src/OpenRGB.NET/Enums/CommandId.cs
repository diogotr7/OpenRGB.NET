namespace OpenRGB.NET;

/// <summary>
///     Enum representing the various commands supported by the SDK server.
/// </summary>
public enum CommandId : uint
{
    /// <summary>
    ///     Request the number of device controllers from the server.
    /// </summary>
    RequestControllerCount = 0,

    /// <summary>
    ///     Request the data block of a controller from the server.
    /// </summary>
    RequestControllerData = 1,

    /// <summary>
    ///     Request the latest supported SDK server protocol version.
    /// </summary>
    RequestProtocolVersion = 40,

    /// <summary>
    ///     Send the server the name of the client.
    /// </summary>
    SetClientName = 50,

    /// <summary>
    ///     Indicates device list has been updated.
    /// </summary>
    DeviceListUpdated = 100,

    /// <summary>
    ///     Request list of profiles from the server.
    /// </summary>
    RequestProfiles = 150,

    /// <summary>
    ///     Saves the current profile to disk.
    /// </summary>
    SaveProfile = 151,

    /// <summary>
    ///     Load a given profile.
    /// </summary>
    LoadProfile = 152,

    /// <summary>
    ///     Delete a given profile.
    /// </summary>
    DeleteProfile = 153,
    
    /// <summary>
    ///     Request list of plugins from the server.
    /// </summary>
    RequestPlugins = 200,
    
    /// <summary>
    ///     Interact with a plugin installed on the server.
    /// </summary>
    PluginSpecific = 201,

    /// <summary>
    ///     Calls RGBController::ResizeZone() on the server. Not implemented.
    /// </summary>
    ResizeZone = 1000,

    /// <summary>
    ///     Calls RGBController::UpdateLEDs() on the server.
    /// </summary>
    UpdateLeds = 1050,

    /// <summary>
    ///     Calls RGBController::UpdateZoneLEDs() on the server.
    /// </summary>
    UpdateZoneLeds = 1051,

    /// <summary>
    ///     Calls RGBController::UpdateSingleLED() on the server. Not implemented.
    /// </summary>
    UpdateSingleLed = 1052,

    /// <summary>
    ///     Calls RGBController::SetCustomMode() on the server.
    /// </summary>
    SetCustomMode = 1100,

    /// <summary>
    ///     Calls RGBController::UpdateMode() on the server.
    /// </summary>
    UpdateMode = 1101,
    
    /// <summary>
    ///     Calls RGBController::SaveMode() on the server.
    /// </summary>
    SaveMode = 1102
}