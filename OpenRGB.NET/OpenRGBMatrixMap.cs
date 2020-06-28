using System;

namespace OpenRGB.NET
{
    public class OpenRGBMatrixMap
    {
        public uint Height;
        public uint Width;
        public uint[,] Matrix;

        public static OpenRGBMatrixMap Decode(byte[] buffer, ref int offset)
        {
            var matx = new OpenRGBMatrixMap();

            matx.Height = BufferReader.GetUInt32(buffer, ref offset);

            matx.Width = BufferReader.GetUInt32(buffer, ref offset);

            matx.Matrix = new uint[matx.Height, matx.Width];

            for (int i = 0; i < matx.Height; i++)
            {
                for (int j = 0; j < matx.Width; j++)
                {
                    matx.Matrix[i, j] = BufferReader.GetUInt32(buffer, ref offset);
                }
            }

            return matx;
        }
    }
}
