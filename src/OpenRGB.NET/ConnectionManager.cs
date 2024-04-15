using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal sealed class ConnectionManager : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Socket _socket;
    private readonly FrozenDictionary<CommandId, BlockingCollection<byte[]>> _pendingRequests;
    private Task? _readLoopTask;
    
    public bool Connected => _socket.Connected;

    public ProtocolVersion CurrentProtocolVersion { get; private set; }

    public ConnectionManager()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true;//TODO: is this necessary?
        _pendingRequests = Enum.GetValues<CommandId>().ToFrozenDictionary(c => c, _ => new BlockingCollection<byte[]>());
    }

    public void Connect(string name, string ip, int port, int timeoutMs, uint protocolVersionNumber = 4)
    {
        if (Connected)
            return;

        _socket.Connect(ip, port, timeoutMs, _cancellationTokenSource.Token);
        _readLoopTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);

        Send(CommandId.SetClientName, 0, new OpenRgbString(name));
        
        var commonProtocolVersion = NegotiateProtocolVersion(protocolVersionNumber);
        CurrentProtocolVersion = ProtocolVersion.FromNumber(commonProtocolVersion);
    }

    public async Task ReadLoop()
    {
        while (!_cancellationTokenSource.IsCancellationRequested && _socket.Connected)
        {
            try
            {
                var header = await ReadHeader();
                if (header.Command == CommandId.DeviceListUpdated)
                {
                    //TODO: is this the best way to do this?
                    //DeviceListUpdated?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var dataBuffer = new byte[header.DataLength];
                    await _socket.ReceiveAllAsync(dataBuffer, _cancellationTokenSource.Token);
                    _pendingRequests[header.Command].Add(dataBuffer, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                //ignore
            }
        }
    }

    public async Task<PacketHeader> ReadHeader()
    {
        var rent = ArrayPool<byte>.Shared.Rent(PacketHeader.LENGTH);
        try
        {
            await _socket.ReceiveAllAsync(rent.AsMemory(0, PacketHeader.LENGTH), _cancellationTokenSource.Token);
            return PacketHeader.FromSpan(rent);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    public void Send<T>(CommandId command, uint deviceId, T data, ReadOnlySpan<byte> additionalData = default) where T : ISpanWritable
    {
        var dataLength = data.Length + additionalData.Length;
        var totalLength = PacketHeader.LENGTH + dataLength;
        var header = new PacketHeader(deviceId, command, (uint)dataLength);

        var rent = ArrayPool<byte>.Shared.Rent(totalLength);
        var buffer = rent.AsSpan(0, totalLength);
        var writer = new SpanWriter(buffer);

        header.WriteTo(ref writer);
        data.WriteTo(ref writer);
        writer.Write(additionalData);

        try
        {
            _socket.SendAll(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    public TResult Request<TRequest, TReader, TResult>(CommandId command, uint deviceId, TRequest requestData)
        where TRequest : ISpanWritable
        where TReader : ISpanReader<TResult>
    {
        Send(command, deviceId, requestData);
        var reader = new SpanReader(_pendingRequests[command].Take(_cancellationTokenSource.Token));
        //this deviceId here is a bit hacky, it's used because some Models store their own index
        return TReader.ReadFrom(ref reader, CurrentProtocolVersion, (int)deviceId);
    }

    private uint NegotiateProtocolVersion(uint maxSupportedProtocolVersion)
    {
        _socket.ReceiveTimeout = 1000;

        uint version;

        try
        {
            version = Request<Primitive<uint>, PrimitiveReader<uint>, Primitive<uint>>(CommandId.RequestProtocolVersion, 0, maxSupportedProtocolVersion);
        }
        catch (TimeoutException e)
        {
            version = 0;
        }

        _socket.ReceiveTimeout = 0;

        return Math.Min(version, maxSupportedProtocolVersion);
    }

    public void Dispose()
    {
        //todo
        _cancellationTokenSource.Cancel();
        try
        {
            _readLoopTask?.Wait();

        }
        catch
        {
            //ignored
        }
        _cancellationTokenSource.Dispose();
        _socket.Dispose();
        _readLoopTask?.Dispose();
    }
}