using System;
using System.Text;

namespace OpenRGB.NET
{
    public class PacketHeader
    {
        public const int Size = 16;

        public uint DeviceId { get; }
        public uint Command { get; }
        public uint DataLength { get; }

        internal PacketHeader(uint deviceId, uint command, uint length)
        {
            DeviceId = deviceId;
            Command = command;
            DataLength = length;
        }

        internal byte[] Encode()
        {
            var arr = new byte[Size];

            Encoding.ASCII.GetBytes("ORGB").CopyTo(arr, 0);
            BitConverter.GetBytes(DeviceId).CopyTo(arr, 4);
            BitConverter.GetBytes(Command).CopyTo(arr, 8);
            BitConverter.GetBytes(DataLength).CopyTo(arr, 12);

            return arr;
        }

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
