using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

/// <summary>
///     Client for the OpenRGB SDK.
/// </summary>
public sealed class OpenRgbClient : IDisposable, IOpenRgbClient
{
    private const int MAX_PROTOCOL_NUMBER = 4;
    private readonly string _name;
    private readonly string _ip;
    private readonly int _port;
    private readonly int _timeoutMs;
    private readonly Socket _socket;
    private readonly byte[] _headerBuffer = new byte[PacketHeader.Length];
    private readonly Dictionary<uint, BlockingCollection<byte[]>> _pendingRequests;
    
    /// <inheritdoc />
    public bool Connected => _socket?.Connected ?? false;

    /// <inheritdoc />
    public ProtocolVersion MaxSupportedProtocolVersion => ProtocolVersion.FromNumber(MAX_PROTOCOL_NUMBER);

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
                        uint protocolVersionNumber = MAX_PROTOCOL_NUMBER)
    {
        _ip = ip;
        _port = port;
        _name = name;
        _timeoutMs = timeoutMs;
        
        _pendingRequests = new Dictionary<uint, BlockingCollection<byte[]>>();
        foreach (var item in Enum.GetValues<CommandId>())
            _pendingRequests[(uint)item] = new BlockingCollection<byte[]>();
        
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;

        if (protocolVersionNumber > MAX_PROTOCOL_NUMBER)
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

        BeginReceive();
        
        //null terminate before sending
        SendMessage(CommandId.SetClientName, Encoding.ASCII.GetBytes(_name + '\0'));

        var minimumCommonVersionNumber = Math.Min(ClientProtocolVersion.Number, GetServerProtocolVersion());
        CommonProtocolVersion = ProtocolVersion.FromNumber(minimumCommonVersionNumber);
    }

    /// <summary>
    ///     Sends a message to the server with the given command and buffer of data.
    ///     Takes care of sending a header packet first to tell the server how many bytes to read.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="buffer"></param>
    /// <param name="deviceId"></param>
    private void SendMessage(CommandId command, ReadOnlySpan<byte> buffer = default, uint deviceId = 0)
    {
        //we can send the header right away. it contains the command we are sending
        //and the size of the packet that follows
        var header = new PacketHeader(deviceId, (uint)command, (uint)buffer.Length);
        Span<byte> headerBuffer = stackalloc byte[PacketHeader.Length];
        
        var headerWriter = new SpanWriter(headerBuffer);
        header.WriteTo(ref headerWriter);

        if (_socket.Send(headerBuffer) != PacketHeader.Length)
            throw new IOException("Sent incorrect number of bytes when sending header in " + nameof(SendMessage));

        if (header.DataLength <= 0)
            return;

        if (_socket.SendFull(buffer) != header.DataLength)
            throw new IOException("Sent incorrect number of bytes when sending data in " + nameof(SendMessage));
    }

    private byte[] SendMessageAndGetResponse(CommandId command, ReadOnlySpan<byte> buffer = default, uint deviceId = 0)
    {
        SendMessage(command, buffer, deviceId);

        if (!_pendingRequests[(uint)command].TryTake(out var outBuffer, _timeoutMs))
            throw new TimeoutException($"Did not receive response to {command} in expected time of {_timeoutMs} ms");

        return outBuffer;
    }

    private void OnReceive(IAsyncResult ar)
    {
        if (_socket is not { Connected: true }) return;

        try
        {
            _socket.EndReceive(ar);
        }
        catch
        {
            //means socket is closed. throwing exception here would crash the host app. (this is separate thread)
            return;
        }

        //decode _headerBuffer into a header to know how many bytes we will receive next
        var reader = new SpanReader(_headerBuffer);
        var header = PacketHeader.ReadFrom(ref reader);

        if (header.Command == (uint)CommandId.DeviceListUpdated)
        {
            var dataBuffer = new byte[header.DataLength];
            _socket.ReceiveFull(dataBuffer);

            BeginReceive();
            //notify users to update device list
            DeviceListUpdated?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //we then make a buffer that will receive the data
            var dataBuffer = new byte[header.DataLength];
            _socket.ReceiveFull(dataBuffer);

            _pendingRequests[header.Command].Add(dataBuffer);

            BeginReceive();
        }
    }

    private void BeginReceive()
    {
        if (!_socket.Connected) return;
        
        _socket.BeginReceive(_headerBuffer, 0, PacketHeader.Length, SocketFlags.None, OnReceive, null);
    }

    /// <inheritdoc />
    public int GetControllerCount()
    {
        return (int)BitConverter.ToUInt32(SendMessageAndGetResponse(CommandId.RequestControllerCount), 0);
    }

    /// <inheritdoc />
    public Device GetControllerData(int id)
    {
        if (id < 0)
            throw new ArgumentException("Unexpected device Id", nameof(id));

        var response = SendMessageAndGetResponse(CommandId.RequestControllerData,BitConverter.GetBytes(CommonProtocolVersion.Number), (uint)id);
        var responseReader = new SpanReader(response);
        return Device.ReadFrom(ref responseReader, CommonProtocolVersion, id);
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
        
        Span<byte> protocolVersionBuffer = stackalloc byte[4];
        var protocolVersionWriter = new SpanWriter(protocolVersionBuffer);
        protocolVersionWriter.WriteUInt32(CommonProtocolVersion.Number);
        
        var buffer = SendMessageAndGetResponse(CommandId.RequestProfiles, protocolVersionBuffer);

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

        var buffer = SendMessageAndGetResponse(CommandId.RequestPlugins);

        var reader = new SpanReader(buffer);
        var count = reader.ReadUInt16();
        
        return Plugin.ReadManyFrom(ref reader, count);
    }

    private byte[] SendPluginSpecificData(int pluginIndex, ReadOnlySpan<byte> data)
    {
        //TODO
        return Array.Empty<byte>();
        return SendMessageAndGetResponse(CommandId.PluginSpecific, data, (uint)pluginIndex);
    }

    private uint GetServerProtocolVersion()
    {
        uint serverVersion;

        _socket.ReceiveTimeout = 1000;
        try
        {
            Span<byte> buffer = stackalloc byte[4];
            var writer = new SpanWriter(buffer);
            writer.WriteUInt32(ClientProtocolVersion.Number);
            
            var response = SendMessageAndGetResponse(CommandId.RequestProtocolVersion,buffer);
            serverVersion = BitConverter.ToUInt32(response);
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
        {
            serverVersion = 0;
        }

        _socket.ReceiveTimeout = 0;

        return serverVersion;
    }

    /// <inheritdoc />
    public void ResizeZone(int deviceId, int zoneId, int size)
    {
        Span<byte> packet = stackalloc byte[PacketFactory.ResizeZoneLength];
        var writer = new SpanWriter(packet);
        PacketFactory.ResizeZone(ref writer, (uint)zoneId, (uint)size);

        SendMessage(CommandId.ResizeZone, packet, (uint)deviceId);
    }

    /// <inheritdoc />
    public void UpdateLeds(int deviceId, ReadOnlySpan<Color> colors)
    {
        if (colors.Length == 0)
            throw new ArgumentException("The colors span is empty.", nameof(colors));

        if (deviceId < 0)
            throw new ArgumentException("Invalid deviceId", nameof(deviceId));

        var packetLength = (int)PacketFactory.UpdateLedsLength(colors.Length);
        var rent = ArrayPool<byte>.Shared.Rent(packetLength);
        try
        {
            var packet = rent.AsSpan(0, packetLength);
            var writer = new SpanWriter(packet);

            PacketFactory.UpdateLeds(ref writer, in colors);

            SendMessage(CommandId.UpdateLeds, packet, (uint)deviceId);
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

        var packetLength = (int)PacketFactory.UpdateZoneLedsLength(colors.Length);
        var rent = ArrayPool<byte>.Shared.Rent(packetLength);
        try
        {
            var packet = rent.AsSpan(0, packetLength);
            var writer = new SpanWriter(packet);

            PacketFactory.UpdateZoneLeds(ref writer, (uint)zoneId, colors);

            SendMessage(CommandId.UpdateZoneLeds, packet, (uint)deviceId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <inheritdoc />
    public void UpdateSingleLed(int deviceId, int ledId, Color color)
    {
        Span<byte> packet = stackalloc byte[PacketFactory.UpdateSingleLedLength];
        var writer = new SpanWriter(packet);
        PacketFactory.UpdateSingleLed(ref writer, (uint)ledId, in color);

        SendMessage(CommandId.UpdateSingleLed, packet, (uint)deviceId);
    }

    /// <inheritdoc />
    public void SetCustomMode(int deviceId)
    {
        SendMessage(CommandId.SetCustomMode, null, (uint)deviceId);
    }

    /// <inheritdoc />
    public void LoadProfile(string profile)
    {
        if (!CommonProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.LoadProfile, Encoding.ASCII.GetBytes(profile + '\0'));
    }

    /// <inheritdoc />
    public void SaveProfile(string profile)
    {
        if (!CommonProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.SaveProfile, Encoding.ASCII.GetBytes(profile + '\0'));
    }

    /// <inheritdoc />
    public void DeleteProfile(string profile)
    {
        if (!CommonProtocolVersion.SupportsProfileControls)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.DeleteProfile, Encoding.ASCII.GetBytes(profile + '\0'));
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

        var packetLength = (int)targetMode.GetLength();
        var rent = ArrayPool<byte>.Shared.Rent(packetLength);
        try
        {
            var packet = rent.AsSpan(0, packetLength);
            var writer = new SpanWriter(packet);

            targetMode.WriteTo(ref writer);

            SendMessage(CommandId.UpdateMode, packet, (uint)deviceId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }
    
    /// <inheritdoc />
    public void SaveMode(int deviceId, int modeId)
    {
        var targetDevice = GetControllerData(deviceId);

        if (modeId > targetDevice.Modes.Length)
            throw new ArgumentException(nameof(modeId));

        var targetMode = targetDevice.Modes[modeId];
        
        var packetLength = (int)targetMode.GetLength();
        var rent = ArrayPool<byte>.Shared.Rent(packetLength);
        try
        {
            var packet = rent.AsSpan(0, packetLength);
            var writer = new SpanWriter(packet);

            targetMode.WriteTo(ref writer);

            SendMessage(CommandId.SaveMode, packet, (uint)deviceId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _socket.Dispose();
    }
}