using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{

    public class OpenRGBZone
    {
        public string name;
        public uint type;
        public OpenRGBLed[] leds;
        public OpenRGBColor[] colors;
        public uint startIndex;
        public uint ledsCount;
        public uint ledsMin;
        public uint ledsMax;
        public OpenRGBMatrixMap matrixMap;

        public static OpenRGBZone[] Decode(byte[] buffer, ref int offset, ushort zoneCount)
        {
            var zones = new List<OpenRGBZone>((int)zoneCount);

            for(int zone = 0; zone < zoneCount; zone++)
            {
                var newZone = new OpenRGBZone();
                
                ushort nameLength = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(ushort);

                newZone.name = Encoding.ASCII.GetString(buffer, offset, nameLength - 1);
                offset += nameLength;

                newZone.type = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newZone.ledsMin = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newZone.ledsMax = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                newZone.ledsCount = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                ushort zoneMatrixLength = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(ushort);

                if (zoneMatrixLength > 0)
                    newZone.matrixMap = OpenRGBMatrixMap.Decode(buffer, ref offset, zoneMatrixLength);
                else
                    newZone.matrixMap = null;

                zones.Add(newZone);
            }

            return zones.ToArray();
        }
    }
}
