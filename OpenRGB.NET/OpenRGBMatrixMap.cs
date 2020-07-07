namespace OpenRGB.NET
{
    public class OpenRGBMatrixMap
    {
        public uint Height;
        public uint Width;
        public uint[,] Matrix;

        internal static OpenRGBMatrixMap Decode(byte[] buffer, ref int offset)
        {
            var matx = new OpenRGBMatrixMap();

            matx.Height = buffer.GetUInt32(ref offset);

            matx.Width = buffer.GetUInt32(ref offset);

            matx.Matrix = new uint[matx.Height, matx.Width];

            for (int i = 0; i < matx.Height; i++)
            {
                for (int j = 0; j < matx.Width; j++)
                {
                    matx.Matrix[i, j] = buffer.GetUInt32(ref offset);
                }
            }

            return matx;
        }
    }
}
