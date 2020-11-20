using OpenRGB.NET.Utils;
using System;
using System.IO;

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
        /// <param name="reader"></param>
        internal static MatrixMap Decode(BinaryReader reader)
        {
            var matx = new MatrixMap();

            matx.Height = reader.ReadUInt32();

            matx.Width = reader.ReadUInt32();

            matx.Matrix = new uint[matx.Height, matx.Width];

            for (int i = 0; i < matx.Height; i++)
            {
                for (int j = 0; j < matx.Width; j++)
                {
                    matx.Matrix[i, j] = reader.ReadUInt32();
                }
            }

            return matx;
        }
    }
}
