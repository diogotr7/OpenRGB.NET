using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OpenRGB.NET.Utils;

internal static class SocketExtensions
{
    public static void Connect(this Socket socket, string host, int port, int timeoutMs, CancellationToken cancellationToken)
    {
        var result = socket.ConnectAsync(host, port);
        Task.WaitAny([result], timeoutMs, cancellationToken);

        if (socket.Connected)
            return;

        socket.Close();
        throw new TimeoutException("Could not connect to OpenRGB");
    }

    public static async Task ReceiveAllAsync(this Socket socket, Memory<byte> buffer, CancellationToken cancellationToken)
    {
        var recv = 0;
        while (recv < buffer.Length)
        {
            var received = await socket.ReceiveAsync(buffer[recv..], SocketFlags.None, cancellationToken);
            if (received == 0)
                break;

            recv += received;
        }
    }

    public static async Task SendAllAsync(this Socket socket, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken)
    {
        var sent = 0;
        while (sent < buffer.Length)
        {
            var sentBytes = await socket.SendAsync(buffer[sent..], SocketFlags.None, cancellationToken);
            sent += sentBytes;
        }
    }

    public static void SendAll(this Socket socket, ReadOnlySpan<byte> buffer)
    {
        var sent = 0;
        while (sent < buffer.Length)
        {
            var sentBytes = socket.Send(buffer[sent..]);
            sent += sentBytes;
        }
    }
}