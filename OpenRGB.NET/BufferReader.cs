using System;
using System.Text;

namespace OpenRGB.NET
{
    internal static class BufferReader
    {
        internal static int GetInt32(this byte[] buffer, ref int offset)
        {
            int value = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);
            return value;
        }

        internal static uint GetUInt32(this byte[] buffer, ref int offset)
        {
            uint value = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);
            return value;
        }

        internal static ushort GetUInt16(this byte[] buffer, ref int offset)
        {
            ushort value = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);
            return value;
        }

        internal static string GetString(this byte[] buffer, ref int offset)
        {
            ushort strLength = GetUInt16(buffer, ref offset);
            string value = Encoding.ASCII.GetString(buffer, offset, strLength - 1);
            offset += strLength;
            return value;
        }
    }
}
