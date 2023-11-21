using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    private readonly Socket _socket;
    private readonly byte[] _headerBuffer;
    private Task? _readLoopTask;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Dictionary<CommandId, BlockingCollection<byte[]>> _pendingRequests;

    /// <inheritdoc />
    public bool Connected => _socket?.Connected ?? false;

    /// <inheritdoc />
    public ProtocolVersion MaxSupportedProtocolVersion => ProtocolVersion.FromNumber(MaxProtocolNumber);

    /// <inheritdoc />
    public ProtocolVersion ClientProtocolVersion { get; }

    /// <inheritdoc />
    public ProtocolVersion CommonProtocolVersion { get; private set; }

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
        _cancellationTokenSource = new CancellationTokenSource();
        _pendingRequests = Enum.GetValues(typeof(CommandId)).Cast<CommandId>().ToDictionary(c => c, _ => new BlockingCollection<byte[]>());
        _headerBuffer = new byte[PacketHeader.Length];
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;

        if (protocolVersionNumber > MaxProtocolNumber)
            throw new ArgumentException("Client protocol version provided higher than supported.",
                nameof(protocolVersionNumber));

        CommonProtocolVersion = ProtocolVersion.Invalid;
        ClientProtocolVersion = ProtocolVersion.FromNumber(protocolVersionNumber);

        if (autoConnect) Connect();
    }

    /// <inheritdoc />
    public void Connect()
    {
        if (Connected)
            return;

        _socket.Connect(_ip, _port, TimeSpan.FromMilliseconds(_timeoutMs));
        _readLoopTask = Task.Run(ReadLoop);

        var length = PacketHeader.Length + PacketFactory.GetStringOperationLength(_name);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        var packet = rent.AsSpan(0, length);

        PacketFactory.WriteStringOperation(packet, _name, CommandId.SetClientName);

        try
        {
            SendOrThrow(packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }

        var minimumCommonVersionNumber = Math.Min(ClientProtocolVersion.Number, GetServerProtocolVersion());
        CommonProtocolVersion = ProtocolVersion.FromNumber(minimumCommonVersionNumber);
    }
    
    private async Task ReadLoop()
    {
        try
        {
            while (!_cancellationTokenSource.IsCancellationRequested && Connected)
            {
                //todo: handle zero
                await _socket.ReceiveAsync(_headerBuffer, SocketFlags.None);

                var dataLength = ParseHeader();
                if (dataLength.Command == CommandId.DeviceListUpdated)
                {
                    //TODO: is this the best way to do this?
                    DeviceListUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var dataBuffer = new byte[dataLength.DataLength];
                    await _socket.ReceiveAsync(dataBuffer, SocketFlags.None);
                    _pendingRequests[dataLength.Command].Add(dataBuffer);
                }
            }
        }
        catch (TaskCanceledException)
        {
            //ignore
        }
    }
    
    private PacketHeader ParseHeader()
    {
        var reader = new SpanReader(_headerBuffer);
        return PacketHeader.ReadFrom(ref reader);
    }
    
    private void SendHeader(CommandId command, uint deviceId)
    {
        Span<byte> packet = stackalloc byte[PacketHeader.Length];
        var writer = new SpanWriter(packet);

        var header = new PacketHeader(deviceId, command, 0);
        header.WriteTo(ref writer);

        SendOrThrow(packet);
    }

    private byte[] SendHeaderAndGetResponse(CommandId command, uint deviceId)
    {
        SendHeader(command, deviceId);

        return GetResponse(command);
    }

    private byte[] GetResponse(CommandId command)
    {
        if (!_pendingRequests[command].TryTake(out var outBuffer, _timeoutMs))
            throw new TimeoutException($"Did not receive response to {command} in expected time of {_timeoutMs} ms");

        return outBuffer;
    }

    private void SendOrThrow(Span<byte> buffer)
    {
        var size = buffer.Length;
        var total = 0;

        while (total < size)
        {
            var recv = _socket.Send(buffer[total..]);
            if (recv == 0 && total != size)
            {
                throw new IOException("Sent incorrect number of bytes.");
            }

            total += recv;
        }
    }

    private uint GetServerProtocolVersion()
    {
        uint serverVersion;

        _socket.ReceiveTimeout = 1000;

        Span<byte> packet = stackalloc byte[PacketHeader.Length + PacketFactory.ProtocolVersionLength];
        PacketFactory.WriteProtocolVersion(packet, 0, CommonProtocolVersion.Number, CommandId.RequestProtocolVersion);

        try
        {
            SendOrThrow(packet);

            var response = GetResponse(CommandId.RequestProtocolVersion);
            serverVersion = BitConverter.ToUInt32(response);
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
        {
            serverVersion = 0;
        }

        _socket.ReceiveTimeout = 0;

        return serverVersion;
    }
    
    private void ProfileOperation(string profile, CommandId operation)
    {
        if (!CommonProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        var length = PacketHeader.Length + PacketFactory.GetStringOperationLength(profile);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        var packet = rent.AsSpan(0, length);

        PacketFactory.WriteStringOperation(packet, profile, operation);

        try
        {
            SendOrThrow(packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }
    
    private void ModeOperation(int deviceId, int modeId, Mode targetMode, CommandId operation)
    {
        var length = PacketHeader.Length + PacketFactory.GetModeOperationLength(targetMode);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        var packet = rent.AsSpan(0, length);

        PacketFactory.WriteModeOperation(packet, (uint)deviceId, (uint)modeId, targetMode, operation);

        try
        {
            SendOrThrow(packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    #region API

    /// <inheritdoc />
    public int GetControllerCount()
    {
        return (int)BitConverter.ToUInt32(SendHeaderAndGetResponse(CommandId.RequestControllerCount, 0), 0);
    }

    /// <inheritdoc />
    public Device GetControllerData(int deviceId)
    {
        if (deviceId < 0)
            throw new ArgumentException("Unexpected device Id", nameof(deviceId));

        Span<byte> packet = stackalloc byte[PacketHeader.Length + PacketFactory.ProtocolVersionLength];

        PacketFactory.WriteProtocolVersion(packet, (uint)deviceId, CommonProtocolVersion.Number, CommandId.RequestControllerData);

        SendOrThrow(packet);

        var response = GetResponse(CommandId.RequestControllerData);
        var responseReader = new SpanReader(response);
        return Device.ReadFrom(ref responseReader, CommonProtocolVersion, deviceId);
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
        if (!CommonProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        Span<byte> packet = stackalloc byte[PacketHeader.Length + PacketFactory.ProtocolVersionLength];

        PacketFactory.WriteProtocolVersion(packet, 0, CommonProtocolVersion.Number, CommandId.RequestProfiles);

        SendOrThrow(packet);

        var buffer = GetResponse(CommandId.RequestProfiles);

        var reader = new SpanReader(buffer);
        var dataSize = reader.ReadUInt32();
        var count = reader.ReadUInt16();
        var profiles = new string[count];

        for (var i = 0; i < count; i++)
            profiles[i] = reader.ReadLengthAndString();

        return profiles;
    }

    /// <inheritdoc />
    public Plugin[] GetPlugins()
    {
        if (!CommonProtocolVersion.SupportsSegmentsAndPlugins)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion.Number}");

        var buffer = SendHeaderAndGetResponse(CommandId.RequestPlugins, 0);
        var reader = new SpanReader(buffer);
        var count = reader.ReadUInt16();

        return Plugin.ReadManyFrom(ref reader, count);
    }

    /// <inheritdoc />
    public void ResizeZone(int deviceId, int zoneId, int size)
    {
        Span<byte> packet = stackalloc byte[PacketFactory.ResizeZoneLength];

        PacketFactory.WriteResizeZone(packet, (uint)deviceId, (uint)zoneId, (uint)size);

        SendOrThrow(packet);
    }

    /// <inheritdoc />
    public void UpdateLeds(int deviceId, ReadOnlySpan<Color> colors)
    {
        if (colors.Length == 0)
            throw new ArgumentException("The colors span is empty.", nameof(colors));

        if (deviceId < 0)
            throw new ArgumentException("Invalid deviceId", nameof(deviceId));

        var length = PacketHeader.Length + PacketFactory.GetUpdateLedsLength(colors.Length);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        var packet = rent.AsSpan(0, length);

        PacketFactory.WriteUpdateLeds(packet, deviceId, colors);

        try
        {
            SendOrThrow(packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
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

        var length = PacketHeader.Length + PacketFactory.GetUpdateZoneLedsLength(colors.Length);
        var rent = ArrayPool<byte>.Shared.Rent(length);
        var packet = rent.AsSpan(0, length);

        PacketFactory.WriteUpdateZoneLeds(packet, (uint)deviceId, (uint)zoneId, colors);

        try
        {
            SendOrThrow(packet);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <inheritdoc />
    public void UpdateSingleLed(int deviceId, int ledId, Color color)
    {
        if (deviceId < 0)
            throw new ArgumentException("Invalid device id.", nameof(deviceId));

        if (ledId < 0)
            throw new ArgumentException("Invalid led id", nameof(ledId));

        Span<byte> packet = stackalloc byte[PacketHeader.Length + PacketFactory.UpdateSingleLedLength];

        PacketFactory.WriteUpdateSingleLed(packet, (uint)deviceId, (uint)ledId, color);

        SendOrThrow(packet);
    }

    /// <inheritdoc />
    public void SetCustomMode(int deviceId)
    {
        SendHeader(CommandId.SetCustomMode, (uint)deviceId);
    }

    /// <inheritdoc />
    public void LoadProfile(string profile)
    {
        ProfileOperation(profile, CommandId.LoadProfile);
    }

    /// <inheritdoc />
    public void SaveProfile(string profile)
    {
        ProfileOperation(profile, CommandId.SaveProfile);
    }

    /// <inheritdoc />
    public void DeleteProfile(string profile)
    {
        ProfileOperation(profile, CommandId.DeleteProfile);
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

        ModeOperation(deviceId, modeId, targetMode, CommandId.UpdateMode);
    }

    /// <inheritdoc />
    public void SaveMode(int deviceId, int modeId)
    {
        var targetDevice = GetControllerData(deviceId);

        if (modeId > targetDevice.Modes.Length)
            throw new ArgumentException(nameof(modeId));

        var targetMode = targetDevice.Modes[modeId];

        ModeOperation(deviceId, modeId, targetMode, CommandId.SaveMode);
    }

    #endregion

    /// <inheritdoc />
    public void Dispose()
    {
        try
        {
            _cancellationTokenSource.Cancel();
            _readLoopTask?.Wait();
            _socket.Dispose();
        }
        catch
        {
            // ignored
        }
    }
}