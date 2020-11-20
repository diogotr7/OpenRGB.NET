using System;
using System.IO;
using System.Text;

namespace OpenRGB.NET.Utils
{
    /// <summary>
    /// Utility methods to read and write strings into / from a buffer in the format OpenRGB expects
    /// </summary>
    internal static class BinaryStringExtensions
    {
        internal static string ReadLengthAndString(this BinaryReader reader)
        {
            ushort length = reader.ReadUInt16();
            return Encoding.ASCII.GetString(reader.ReadBytes(length), 0, length - 1);
        }

        internal static void WriteLengthAndString(this BinaryWriter writer, string s)
        {
            writer.Write((ushort)(s.Length + 1));
            writer.Write(Encoding.ASCII.GetBytes(s + '\0'));
        }
    }
}
