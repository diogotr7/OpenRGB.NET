using System;
using System.Linq;
using System.Runtime.InteropServices;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Client for the OpenRGB SDK.
/// </summary>
public sealed class OpenRgbClient : IDisposable, IOpenRgbClient
{
    private const int MaxProtocolNumber = 4;
    private readonly string _name;
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;
    private readonly uint _protocolVersionNumber;
    private readonly ConnectionManager _manager;

    /// <inheritdoc />
    public bool Connected => _manager.Connected;

    /// <inheritdoc />
    public ProtocolVersion MaxSupportedProtocolVersion => ProtocolVersion.FromNumber(MaxProtocolNumber);

    /// <inheritdoc />
    public ProtocolVersion CurrentProtocolVersion => _manager.CurrentProtocolVersion;

    /// <inheritdoc />
    public event EventHandler<EventArgs>? DeviceListUpdated;

    /// <summary>
    ///     Sets all the needed parameters to connect to the server.
    ///     Connects to the server immediately unless autoConnect is set to false.
    /// </summary>
    public OpenRgbClient(string ip = "127.0.0.1",
        int port = 6742,
        string name = "OpenRGB.NET",
        bool autoConnect = true,
        int timeoutMs = 1000,
        uint protocolVersionNumber = MaxProtocolNumber)
    {
        _ip = ip;
        _port = port;
        _name = name;
        _timeoutMs = timeoutMs;
        _protocolVersionNumber = protocolVersionNumber;
        _manager = new();

        if (protocolVersionNumber > MaxProtocolNumber)
            throw new ArgumentException("Client protocol version provided higher than supported.",
                nameof(protocolVersionNumber));

        if (autoConnect) Connect();
    }

    /// <inheritdoc />
    public void Connect()
    {
        if (Connected)
            return;

        _manager.Connect(_name, _ip, _port, _timeoutMs, _protocolVersionNumber);
    }

    #region API

    /// <inheritdoc />
    public int GetControllerCount()
    {
        return _manager.Request<None, PrimitiveReader<int>, int>(CommandId.RequestControllerCount, 0, new None());
    }

    /// <inheritdoc />
    public Device GetControllerData(int deviceId)
    {
        if (deviceId < 0)
            throw new ArgumentException("Unexpected device Id", nameof(deviceId));

        return _manager.Request<ProtocolVersion, DeviceReader, Device>(CommandId.RequestControllerData, (uint)deviceId, _manager.CurrentProtocolVersion);
    }

    /// <inheritdoc />
    public Device[] GetAllControllerData()
    {
        var count = GetControllerCount();

        var controllers = new Device[count];
        for (var i = 0; i < count; i++)
            controllers[i] = GetControllerData(i);

        return controllers;
    }

    /// <inheritdoc />
    public string[] GetProfiles()
    {
        if (!CurrentProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {CurrentProtocolVersion.Number}");

        return _manager.Request<None, ProfilesReader, string[]>(CommandId.RequestProfiles, 0, new None());
    }

    /// <inheritdoc />
    public Plugin[] GetPlugins()
    {
        if (!CurrentProtocolVersion.SupportsSegmentsAndPlugins)
            throw new NotSupportedException($"Not supported on protocol version {CurrentProtocolVersion.Number}");

        return _manager.Request<None, PluginsReader, Plugin[]>(CommandId.RequestPlugins, 0, new None());
    }

    /// <inheritdoc />
    public void ResizeZone(int deviceId, int zoneId, int size)
    {
        _manager.Send(CommandId.ResizeZone, (uint)deviceId, new Args<uint, uint>((uint)zoneId, (uint)size));
    }

    /// <inheritdoc />
    public void UpdateLeds(int deviceId, ReadOnlySpan<Color> colors)
    {
        if (colors.Length == 0)
            throw new ArgumentException("The colors span is empty.", nameof(colors));

        if (deviceId < 0)
            throw new ArgumentException("Invalid deviceId", nameof(deviceId));

        var bytes = MemoryMarshal.Cast<Color, byte>(colors);
        
        _manager.Send(CommandId.UpdateLeds, (uint)deviceId, new Args<uint, ushort>((uint)0, (ushort)colors.Length), bytes);
    }

    /// <inheritdoc />
    public void UpdateZoneLeds(int deviceId, int zoneId, ReadOnlySpan<Color> colors)
    {
        if (colors.Length == 0)
            throw new ArgumentException("The colors span is empty.", nameof(colors));

        if (deviceId < 0)
            throw new ArgumentException("Invalid device id.", nameof(deviceId));

        if (zoneId < 0)
            throw new ArgumentException("Invalid zone id", nameof(zoneId));

        var bytes = MemoryMarshal.Cast<Color, byte>(colors);

        _manager.Send(CommandId.UpdateZoneLeds, (uint)deviceId, new Args<uint, uint, ushort>(0, (uint)zoneId, (ushort)colors.Length), bytes);
    }

    /// <inheritdoc />
    public void UpdateSingleLed(int deviceId, int ledId, Color color)
    {
        if (deviceId < 0)
            throw new ArgumentException("Invalid device id.", nameof(deviceId));

        if (ledId < 0)
            throw new ArgumentException("Invalid led id", nameof(ledId));

        _manager.Send(CommandId.UpdateSingleLed, (uint)deviceId, new Args<uint, Color>((uint)ledId, color));
    }

    /// <inheritdoc />
    public void SetCustomMode(int deviceId)
    {
        _manager.Send(CommandId.SetCustomMode, (uint)deviceId, new None());
    }

    /// <inheritdoc />
    public void LoadProfile(string profile)
    {
        _manager.Send(CommandId.LoadProfile, 0, new OpenRgbString(profile));
    }

    /// <inheritdoc />
    public void SaveProfile(string profile)
    {
        _manager.Send(CommandId.SaveProfile, 0, new OpenRgbString(profile));
    }

    /// <inheritdoc />
    public void DeleteProfile(string profile)
    {
        _manager.Send(CommandId.DeleteProfile, 0, new OpenRgbString(profile));
    }

    /// <inheritdoc />
    public void UpdateMode(int deviceId, int modeId,
        uint? speed = null,
        Direction? direction = null,
        Color[]? colors = default)
    {
        var targetDevice = GetControllerData(deviceId);

        if (modeId > targetDevice.Modes.Length)
            throw new ArgumentException(nameof(modeId));

        var targetMode = targetDevice.Modes[modeId];

        if (speed.HasValue)
        {
            if (!targetMode.SupportsSpeed)
                throw new InvalidOperationException("Cannot set speed on a mode that doesn't use this parameter");

            targetMode.SetSpeed(speed.Value);
        }

        if (direction.HasValue)
        {
            if (!targetMode.SupportsDirection)
                throw new InvalidOperationException("Cannot set direction on a mode that doesn't use this parameter");

            targetMode.SetDirection(direction.Value);
        }

        if (colors != null)
        {
            if (colors.Length != targetMode.Colors.Length)
                throw new InvalidOperationException("Incorrect number of colors supplied");

            targetMode.SetColors(colors);
        }

        _manager.Send(CommandId.UpdateMode, (uint)deviceId, new ModeOperation(0, (uint)modeId, targetMode));
    }

    /// <inheritdoc />
    public void SaveMode(int deviceId, int modeId)
    {
        var targetDevice = GetControllerData(deviceId);

        if (modeId > targetDevice.Modes.Length)
            throw new ArgumentException(nameof(modeId));

        var targetMode = targetDevice.Modes[modeId];

        _manager.Send(CommandId.SaveMode, (uint)deviceId, new ModeOperation(0, (uint)modeId, targetMode));
    }

    /// <inheritdoc />
    public void PluginSpecific(int pluginId, int pluginPacketType, ReadOnlySpan<byte> data)
    {
        if (!CurrentProtocolVersion.SupportsSegmentsAndPlugins)
            throw new NotSupportedException($"Not supported on protocol version {CurrentProtocolVersion.Number}");

        _manager.Send(CommandId.PluginSpecific, (uint)pluginId, new Args<uint>((uint)pluginPacketType), data);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        _manager.Dispose();
    }
}