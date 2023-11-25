using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace OpenRGB.NET.Utils;

internal static class SocketExtensions
{
    public static void Connect(this Socket socket, string host, int port, int timeoutMs)
    {
        var result = socket.ConnectAsync(host, port);               
        Task.WaitAny(new[] { result }, timeoutMs);
        
        if (socket.Connected)
            return;
        
        socket.Close();
        throw new TimeoutException("Could not connect to OpenRGB");
    }
}