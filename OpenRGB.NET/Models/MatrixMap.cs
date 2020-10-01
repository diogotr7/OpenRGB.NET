using OpenRGB.NET.Utils;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Matrix Map class for the matrix Zone type
    /// </summary>
    public class MatrixMap
    {
        /// <summary>
        /// The height of the matrix.
        /// </summary>
        public uint Height { get; private set; }

        /// <summary>
        /// The width of the matrix.
        /// </summary>
        public uint Width { get; private set; }

        /// <summary>
        /// The matrix.
        /// </summary>
        public uint[,] Matrix { get; private set; }

        /// <summary>
        /// Decodes a byte array into a matrix map
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
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
