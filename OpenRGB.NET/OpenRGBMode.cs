using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBMode
    {
        public string name;
        public int value;
        public uint flags;
        public uint speedMin;
        public uint speedMax;
        public uint colorMin;
        public uint colorMax;
        public uint speed;
        public uint direction;
        public uint colorMode;
        public OpenRGBColor[] colors;

        public static OpenRGBMode[] Decode(byte[] buffer, ref int offset, uint numModes)
        {
            var modes = new List<OpenRGBMode>((int)numModes);

            for (int mode = 0; mode < numModes; mode++)
            {
                var newMode = new OpenRGBMode();
                ushort modeNameLength = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(ushort);

                newMode.name = Encoding.ASCII.GetString(buffer, offset, modeNameLength - 1);
                offset += modeNameLength;

                newMode.value = BitConverter.ToInt32(buffer, offset);
                offset += sizeof(int);

                newMode.flags = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.speedMin = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.speedMax = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.colorMin = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.colorMax = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.speed = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.direction = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newMode.colorMode = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                ushort colorCount = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(ushort);

                newMode.colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

                modes.Add(newMode);
            }

            return modes.ToArray();
        }
    }
}
