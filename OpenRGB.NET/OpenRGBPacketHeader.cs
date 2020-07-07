using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBPacketHeader
    {
        public const int Size = 16;

        public uint DeviceId;
        public uint Command;
        public uint DataLength;

        internal OpenRGBPacketHeader(uint deviceId, uint command, uint length)
        {
            DeviceId = deviceId;
            Command = command;
            DataLength = length;
        }

        internal byte[] Encode()
        {
            var ret = new List<byte>(Size);

            ret.AddRange(Encoding.ASCII.GetBytes("ORGB"));
            ret.AddRange(BitConverter.GetBytes(DeviceId));
            ret.AddRange(BitConverter.GetBytes(Command));
            ret.AddRange(BitConverter.GetBytes(DataLength));

            return ret.ToArray();
        }

        internal static OpenRGBPacketHeader Decode(byte[] buffer)
        {
            if (buffer.Length != Size)
                throw new Exception();
            if (Encoding.ASCII.GetString(buffer, 0, 4) != "ORGB")
                throw new Exception();

            return new OpenRGBPacketHeader(
                BitConverter.ToUInt32(buffer, 4),
                BitConverter.ToUInt32(buffer, 8),
                BitConverter.ToUInt32(buffer, 12)
            );
        }
    }
}
