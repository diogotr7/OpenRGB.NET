using System;
using System.Text;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Packet Header class containing the command ID and the length of the data to be sent.
    /// </summary>
    internal struct PacketHeader
    {
        internal const int Size = 16;
        internal uint DeviceId { get; }
        internal uint Command { get; }
        internal uint DataLength { get; }

        internal PacketHeader(uint deviceId, uint command, uint length)
        {
            DeviceId = deviceId;
            Command = command;
            DataLength = length;
        }

        /// <summary>
        /// Converts the packet into a byte array to send to the server.
        /// </summary>
        /// <returns></returns>
        internal byte[] Encode()
        {
            var arr = new byte[Size];

            Encoding.ASCII.GetBytes("ORGB").CopyTo(arr, 0);
            BitConverter.GetBytes(DeviceId).CopyTo(arr, 4);
            BitConverter.GetBytes(Command).CopyTo(arr, 8);
            BitConverter.GetBytes(DataLength).CopyTo(arr, 12);

            return arr;
        }

        /// <summary>
        /// Decodes a byte array into a PacketHeader
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        internal static PacketHeader Decode(byte[] buffer)
        {
            if (buffer.Length != Size)
                throw new ArgumentException($"{nameof(buffer)} has length {buffer.Length}, should be {Size}");
            if (Encoding.ASCII.GetString(buffer, 0, 4) != "ORGB")
                throw new ArgumentException("Magic bytes \"ORGB\" were not found");

            return new PacketHeader(
                BitConverter.ToUInt32(buffer, 4),
                BitConverter.ToUInt32(buffer, 8),
                BitConverter.ToUInt32(buffer, 12)
            );
        }
    }
}
