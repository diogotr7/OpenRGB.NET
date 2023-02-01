using System;
using System.Net.Sockets;

namespace OpenRGB.NET.Utils;

internal static class SocketExtensions
{
    internal static int ReceiveFull(this Socket socket, Span<byte> buffer)
    {
        var size = buffer.Length;
        var total = 0;

        while (total < size)
        {
            var recv = socket.Receive(buffer[total..]);
            if (recv == 0) break;
            total += recv;
        }

        return total;
    }

    internal static int SendFull(this Socket socket, ReadOnlySpan<byte> buffer)
    {
        var size = buffer.Length;
        var total = 0;

        while (total < size)
        {
            var recv = socket.Send(buffer[total..]);
            if (recv == 0) break;
            total += recv;
        }

        return total;
    }
}