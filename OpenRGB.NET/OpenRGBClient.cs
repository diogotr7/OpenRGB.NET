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
    private const int MAX_PROTOCOL = 4;
    private readonly byte[] _headerBuffer = new byte[PacketHeader.Length];
    private readonly string _ip;
    private readonly string _name;
    private readonly Dictionary<uint, BlockingCollection<byte[]>> _pendingRequests;
    private readonly int _port;
    private readonly Socket _socket;
    private readonly int _timeout;

    /// <inheritdoc />
    public bool Connected => _socket?.Connected ?? false;

    /// <inheritdoc />
    public uint MaxSupportedProtocolVersion => MAX_PROTOCOL;

    /// <inheritdoc />
    public uint ClientProtocolVersion { get; }

    /// <inheritdoc />
    public uint CommonProtocolVersion { get; private set; }

    /// <inheritdoc />
    public event EventHandler<EventArgs>? DeviceListUpdated;

    #region Basic init methods

    /// <summary>
    ///     Sets all the needed parameters to connect to the server.
    ///     Connects to the server immediately unless autoconnect is set to false.
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="name"></param>
    /// <param name="autoconnect"></param>
    /// <param name="timeout"></param>
    /// <param name="protocolVersion"></param>
    public OpenRgbClient(string ip = "127.0.0.1", int port = 6742, string name = "OpenRGB.NET", bool autoconnect = true,
        int timeout = 1000, uint protocolVersion = MAX_PROTOCOL)
    {
        _ip = ip;
        _port = port;
        _name = name;
        _timeout = timeout;
        _pendingRequests = new Dictionary<uint, BlockingCollection<byte[]>>();
        SetupPendingRequests();
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;

        if (protocolVersion > MaxSupportedProtocolVersion)
            throw new ArgumentException("Client protocol version provided higher than supported.",
                nameof(protocolVersion));

        CommonProtocolVersion = uint.MaxValue;
        ClientProtocolVersion = protocolVersion;

        if (autoconnect) Connect();
    }

    private void SetupPendingRequests()
    {
        foreach (var item in Enum.GetValues<CommandId>())
            _pendingRequests[(uint)item] = new BlockingCollection<byte[]>();
    }

    /// <inheritdoc />
    public void Connect()
    {
        if (Connected)
            return;

        var result = _socket.BeginConnect(_ip, _port, null, null);

        result.AsyncWaitHandle.WaitOne(_timeout);

        if (_socket.Connected)
        {
            _socket.EndConnect(result);
            _socket.BeginReceive(_headerBuffer, 0, PacketHeader.Length, SocketFlags.None, OnReceive, null);
        }
        else
        {
            _socket.Close();
            throw new TimeoutException("Failed to connect to server.");
        }

        //null terminate before sending
        SendMessage(CommandId.SetClientName, Encoding.ASCII.GetBytes(_name + '\0'));

        CommonProtocolVersion = Math.Min(ClientProtocolVersion, GetServerProtocolVersion());
    }

    #endregion

    #region Basic Comms methods

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
        var packetSize = buffer.Length;
        var header = new PacketHeader(deviceId, (uint)command, (uint)packetSize);

        Span<byte> headerBuffer = stackalloc byte[PacketHeader.Length];
        var headerWriter = new SpanWriter(headerBuffer);

        header.WriteTo(ref headerWriter);
        var result = _socket.Send(headerBuffer);

        if (result != PacketHeader.Length)
            throw new IOException("Sent incorrect number of bytes when sending header in " + nameof(SendMessage));

        if (packetSize <= 0)
            return;

        result = _socket.SendFull(buffer);

        if (result != packetSize)
            throw new IOException("Sent incorrect number of bytes when sending data in " + nameof(SendMessage));
    }

    private byte[] SendMessageAndGetResponse(CommandId command, ReadOnlySpan<byte> buffer = default, uint deviceId = 0)
    {
        SendMessage(command, buffer, deviceId);

        if (!_pendingRequests[(uint)command].TryTake(out var outBuffer, _timeout))
            throw new TimeoutException($"Did not receive response to {command} in expected time of {_timeout} ms");

        return outBuffer;
    }

    private void OnReceive(IAsyncResult ar)
    {
        if (_socket is not { Connected: true } || _disposed) return;

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

            RestartReceive();
            //notify users to update device list
            DeviceListUpdated?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            //we then make a buffer that will receive the data
            var dataBuffer = new byte[header.DataLength];
            _socket.ReceiveFull(dataBuffer);

            _pendingRequests[header.Command].Add(dataBuffer);

            RestartReceive();
        }
    }

    private void RestartReceive()
    {
        if (_socket.Connected && !_disposed)
            _socket.BeginReceive(_headerBuffer, 0, PacketHeader.Length, SocketFlags.None, OnReceive, null);
    }

    #endregion

    #region Request Methods

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

        var response = SendMessageAndGetResponse(CommandId.RequestControllerData,
            BitConverter.GetBytes(CommonProtocolVersion), (uint)id);
        var responseReader = new SpanReader(response);
        return Device.ReadFrom(ref responseReader, ProtocolVersion.FromNumber(CommonProtocolVersion), id, this);
    }

    /// <inheritdoc />
    public Device[] GetAllControllerData()
    {
        var count = GetControllerCount();

        var array = new Device[count];
        for (var i = 0; i < count; i++)
            array[i] = GetControllerData(i);

        return array;
    }

    /// <inheritdoc />
    public string[] GetProfiles()
    {
        if (ClientProtocolVersion < 2)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        var buffer = SendMessageAndGetResponse(CommandId.RequestProfiles, BitConverter.GetBytes(CommonProtocolVersion));

        var reader = new SpanReader(buffer);
        var dataSize = reader.ReadUInt32();
        var count = reader.ReadUInt16();
        var profiles = new string[count];

        for (var i = 0; i < count; i++)
            profiles[i] = reader.ReadLengthAndString();

        return profiles;
    }

    private uint GetServerProtocolVersion()
    {
        uint serverVersion;

        _socket.ReceiveTimeout = 1000;
        try
        {
            serverVersion = BitConverter.ToUInt32(SendMessageAndGetResponse(CommandId.RequestProtocolVersion,
                BitConverter.GetBytes(ClientProtocolVersion)));
        }
        catch (SocketException e) when (e.SocketErrorCode == SocketError.TimedOut)
        {
            serverVersion = 0;
        }

        _socket.ReceiveTimeout = 0;

        return serverVersion;
    }

    #endregion

    #region Update Methods

    /// <inheritdoc />
    public void UpdateLeds(int deviceId, Color[] colors)
    {
        if (colors is null)
            throw new ArgumentNullException(nameof(colors));

        if (deviceId < 0)
            throw new ArgumentException(nameof(deviceId));

        var packetLength = (int)PacketFactory.UpdateLedsLength(colors.Length);
        var rent = ArrayPool<byte>.Shared.Rent(packetLength);
        try
        {
            var packet = rent.AsSpan(0, packetLength);
            var writer = new SpanWriter(packet);

            PacketFactory.UpdateLeds(ref writer, colors);

            SendMessage(CommandId.UpdateLeds, packet, (uint)deviceId);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    /// <inheritdoc />
    public void UpdateZone(int deviceId, int zoneId, Color[] colors)
    {
        if (colors is null)
            throw new ArgumentNullException(nameof(colors));

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
    public void SetCustomMode(int deviceId)
    {
        SendMessage(CommandId.SetCustomMode, null, (uint)deviceId);
    }

    /// <inheritdoc />
    public void LoadProfile(string profile)
    {
        if (ClientProtocolVersion < 2)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.LoadProfile, Encoding.ASCII.GetBytes(profile + '\0'));
    }

    /// <inheritdoc />
    public void SaveProfile(string profile)
    {
        if (ClientProtocolVersion < 2)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.SaveProfile, Encoding.ASCII.GetBytes(profile + '\0'));
    }

    /// <inheritdoc />
    public void DeleteProfile(string profile)
    {
        if (ClientProtocolVersion < 2)
            throw new NotSupportedException($"Not supported on protocol version {ClientProtocolVersion}");

        SendMessage(CommandId.DeleteProfile, Encoding.ASCII.GetBytes(profile + '\0'));
    }

    /// <inheritdoc />
    public void SetMode(int deviceId, int modeId,
        uint? speed = null,
        Direction? direction = null,
        Color[]? colors = null)
    {
        var targetDevice = GetControllerData(deviceId);

        if (modeId > targetDevice.Modes.Length)
            throw new ArgumentException(nameof(modeId));

        var targetMode = targetDevice.Modes[modeId];

        if (speed.HasValue)
        {
            if (!targetMode.HasFlag(ModeFlags.HasSpeed))
                throw new InvalidOperationException("Cannot set speed on a mode that doesn't use this parameter");

            targetMode.SetSpeed(speed.Value);
        }

        if (direction.HasValue)
        {
            if (!targetMode.HasFlag(ModeFlags.HasDirection))
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

    #endregion

    #region Dispose

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
            if (disposing)
            {
                _disposed = true;
                // Managed object only
                if (_socket != null && _socket.Connected)
                    try
                    {
                        _socket?.Close();
                        _socket?.Shutdown(SocketShutdown.Both);
                        _socket?.Dispose();
                    }
                    catch
                    {
                        //Don't throw in Dispose
                    }

                _disposed = true;
            }
    }

    /// <summary>
    ///     Disposes of the connection to the server.
    ///     To connect again, instantiate a new OpenRGBClient.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}