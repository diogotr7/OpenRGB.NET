using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace OpenRGB.NET.Utils
{
    internal static class SocketExtensions
    {
        internal static int ReceiveFull(this Socket socket, byte[] buffer)
        {
            var size = buffer.Length;
            var total = 0;

            while (total < size)
            {
                var recv = socket.Receive(buffer, total, size - total, SocketFlags.None);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
            }
            return total;
        }

        internal static int SendFull(this Socket socket, byte[] buffer)
        {
            var size = buffer.Length;
            var total = 0;

            while (total < size)
            {
                var recv = socket.Send(buffer, total, size - total, SocketFlags.None);
                if (recv == 0)
                {
                    break;
                }
                total += recv;
            }
            return total;
        }
    }
}
