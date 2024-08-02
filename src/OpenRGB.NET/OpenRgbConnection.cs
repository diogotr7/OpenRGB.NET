using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET;

internal sealed class OpenRgbConnection : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Socket _socket;
    private readonly Dictionary<CommandId, BlockingCollection<byte[]>> _pendingRequests;
    private Task? _readLoopTask;

    public bool Connected => _socket.Connected;

    public ProtocolVersion CurrentProtocolVersion { get; private set; }

    private OpenRgbClient Client { get; }

    public OpenRgbConnection(OpenRgbClient openRgbClient)
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        _socket.NoDelay = true; //Send all data immediately, we rely on the order of packets
        _pendingRequests = Enum.GetValues(typeof(CommandId)).Cast<CommandId>()
            .ToDictionary(c => c, _ => new BlockingCollection<byte[]>());

        Client = openRgbClient;
    }

    public void Connect(string name, string ip, int port, int timeoutMs, uint protocolVersionNumber = 4)
    {
        if (Connected)
            return;

        _socket.Connect(ip, port, timeoutMs, _cancellationTokenSource.Token);
        _readLoopTask = Task.Run(ReadLoop, _cancellationTokenSource.Token);

        Send(CommandId.SetClientName, 0, new StringArg(name));

        var commonProtocolVersion = NegotiateProtocolVersion(protocolVersionNumber);
        CurrentProtocolVersion = ProtocolVersion.FromNumber(commonProtocolVersion);
    }

    private async Task ReadLoop()
    {
        var headerBuffer = new byte[PacketHeader.LENGTH];

        while (!_cancellationTokenSource.IsCancellationRequested && _socket.Connected)
        {
            try
            {
                await _socket.ReceiveAllAsync(headerBuffer, _cancellationTokenSource.Token);
                var header = PacketHeader.FromSpan(headerBuffer);

                if (header.Command == CommandId.DeviceListUpdated)
                {
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            Client.OnDeviceListUpdated();
                        }
                        catch
                        {
                            //ignored
                        }
                    });
                }
                else
                {
                    var dataBuffer = new byte[header.DataLength];
                    await _socket.ReceiveAllAsync(dataBuffer, _cancellationTokenSource.Token);
                    DebugDumpBuffer(dataBuffer, false, header.Command);
                    _pendingRequests[header.Command].Add(dataBuffer, _cancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                //ignore
            }
        }
    }


    public void Send<TRequest>(CommandId command, uint deviceId, TRequest requestData, ReadOnlySpan<byte> additionalData = default)
        where TRequest : ISpanWritable
    {
        var dataLength = requestData.Length + additionalData.Length;
        var totalLength = PacketHeader.LENGTH + dataLength;
        var header = new PacketHeader(deviceId, command, (uint)dataLength);

        var rent = ArrayPool<byte>.Shared.Rent(totalLength);
        var buffer = rent.AsSpan(0, totalLength);
        var writer = new SpanWriter(buffer);

        header.WriteTo(ref writer);
        requestData.WriteTo(ref writer);
        writer.Write(additionalData);

        try
        {
            DebugDumpBuffer(buffer, true, command);
            _socket.SendAll(buffer);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rent);
        }
    }

    private TResult Receive<TReader, TResult>(CommandId command, uint deviceId) where TReader : struct, ISpanReader<TResult>
    {
        var reader = new SpanReader(_pendingRequests[command].Take(_cancellationTokenSource.Token));
        //this deviceId here is a bit hacky, it's used because some Models store their own index
        return TReader.ReadFrom(ref reader, CurrentProtocolVersion, (int)deviceId);
    }

    public TResult Request<TArgument, TReader, TResult>(CommandId command, uint deviceId, TArgument requestData, ReadOnlySpan<byte> additionalData = default)
        where TArgument : ISpanWritable
        where TReader : struct, ISpanReader<TResult>
    {
        Send(command, deviceId, requestData, additionalData);
        return Receive<TReader, TResult>(command, deviceId);
    }

    private uint NegotiateProtocolVersion(uint maxSupportedProtocolVersion)
    {
        _socket.ReceiveTimeout = 1000;

        uint version;

        try
        {
            version = Request<Args<uint>, PrimitiveReader<uint>, uint>(CommandId.RequestProtocolVersion, 0,
                new Args<uint>(maxSupportedProtocolVersion));
        }
        catch (TimeoutException)
        {
            version = 0;
        }

        _socket.ReceiveTimeout = 0;

        return Math.Min(version, maxSupportedProtocolVersion);
    }

    public void Dispose()
    {
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

    [Conditional("DEBUG")]
    private static void DebugDumpBuffer(ReadOnlySpan<byte> buffer, bool sending, CommandId command)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "OpenRGB.NET");
        
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);
        
        var lastFileName = Directory.EnumerateFiles(directory).MaxBy(f => f);
        var lastFileNumber = lastFileName is null ? -1 :
            int.Parse(Path.GetFileNameWithoutExtension(lastFileName).Split('-')[0]);
        
        File.WriteAllBytes(Path.Combine(directory, $"{lastFileNumber + 1:D2}-{(sending ? "Send" : "Receive")}-{command}.bin"), buffer.ToArray());
    }
}