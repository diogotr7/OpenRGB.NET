using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET.Utils
{
    internal static class BufferWriter
    {
        internal static void Set(this byte[] buffer, ref int offset, int data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, offset);
            offset += sizeof(int);
        }

        internal static void Set(this byte[] buffer, ref int offset, uint data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, offset);
            offset += sizeof(uint);
        }

        internal static void Set(this byte[] buffer, ref int offset, ushort data)
        {
            BitConverter.GetBytes(data).CopyTo(buffer, offset);
            offset += sizeof(ushort);
        }

        internal static void Set(this byte[] buffer, ref int offset, string data)
        {
            buffer.Set(ref offset, (ushort)(data.Length + 1));
            Encoding.ASCII.GetBytes(data + '\0').CopyTo(buffer, offset);
            offset += data.Length + 1;
        }

        internal static void Set(this byte[] buffer, ref int offset, byte[] data)
        {
            data.CopyTo(buffer, offset);
            offset += data.Length;
        }
    }
}
