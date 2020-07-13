using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Matrix Map class for the matrix Zone type
    /// </summary>
    public class MatrixMap
    {
        public uint Height { get; private set; }
        public uint Width { get; private set; }
        public uint[,] Matrix { get; private set; }

        /// <summary>
        /// Decodes a byte array into a matrix map
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal static MatrixMap Decode(byte[] buffer, ref int offset)
        {
            var matx = new MatrixMap();

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
