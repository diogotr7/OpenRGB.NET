using System;
using System.Net.Sockets;

namespace OpenRGB.NET.Utils;

internal static class SocketExtensions
{
    internal static void Connect(this Socket socket, string ip, int port, TimeSpan timeout)
    {
        var result = socket.BeginConnect(ip, port, null, null);

        if (result.AsyncWaitHandle.WaitOne(timeout, true))
        {
            socket.EndConnect(result);
        }
        else
        {
            socket.Close();
            throw new SocketException(10060); // Connection timed out.
        }
    }
}