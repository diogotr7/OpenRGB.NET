using System.Collections.Generic;

namespace OpenRGB.NET
{
    public class OpenRGBColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public OpenRGBColor(byte red = 0, byte green = 0, byte blue = 0)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        internal static OpenRGBColor[] Decode(byte[] buffer, ref int offset, ushort colorCount)
        {
            var colors = new List<OpenRGBColor>(colorCount);

            for (int i = 0; i < colorCount; i++)
            {
                colors.Add(new OpenRGBColor
                {
                    Red = buffer[offset],
                    Green = buffer[offset + 1],
                    Blue = buffer[offset + 2]
                    //Alpha = buffer[offset + 3]
                });
                offset += 4 * sizeof(byte);
            }
            return colors.ToArray();
        }

        internal byte[] Encode()
        {
            return new byte[]
            {
                Red,
                Green,
                Blue,
                0
            };
        }

        public override string ToString()
        {
            return $"R:{Red}, G:{Green}, B:{Blue} ";
        }
    }
}
