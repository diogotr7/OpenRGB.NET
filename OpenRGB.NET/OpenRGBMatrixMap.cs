using System;

namespace OpenRGB.NET
{
    public class OpenRGBMatrixMap
    {
        public uint height;
        public uint width;
        public uint[] map;

        public static OpenRGBMatrixMap Decode(byte[] buffer, ref int offset, ushort zoneMatrixLength)
        {
            var matx = new OpenRGBMatrixMap();

            matx.height = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);

            matx.width = BitConverter.ToUInt32(buffer, offset);
            offset += sizeof(uint);

            matx.map = new uint[matx.width * matx.height];

            for (int matrix_idx = 0; matrix_idx < (matx.height * matx.width); matrix_idx++)
            {
                matx.map[matrix_idx] = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);
            }

            return matx;
        }
    }
}
