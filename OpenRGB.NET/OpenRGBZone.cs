using OpenRGB.NET.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBZone
    {
        public string Name;
        public OpenRGBZoneType Type;
        public OpenRGBLed[] Leds;
        public OpenRGBColor[] Colors;
        public uint LedCount;
        public uint LedsMin;
        public uint LedsMax;
        public OpenRGBMatrixMap MatrixMap;

        public static OpenRGBZone[] Decode(byte[] buffer, ref int offset, ushort zoneCount)
        {
            var zones = new List<OpenRGBZone>((int)zoneCount);

            for (int zone = 0; zone < zoneCount; zone++)
            {
                var newZone = new OpenRGBZone();

                newZone.Name = BufferReader.GetString(buffer, ref offset);

                newZone.Type = (OpenRGBZoneType)BufferReader.GetUInt32(buffer, ref offset);

                newZone.LedsMin = BufferReader.GetUInt32(buffer, ref offset);

                newZone.LedsMax = BufferReader.GetUInt32(buffer, ref offset);

                newZone.LedCount = BufferReader.GetUInt32(buffer, ref offset);

                var zoneMatrixLength = BufferReader.GetUInt16(buffer, ref offset);

                if (zoneMatrixLength > 0)
                    newZone.MatrixMap = OpenRGBMatrixMap.Decode(buffer, ref offset);
                else
                    newZone.MatrixMap = null;

                zones.Add(newZone);
            }

            return zones.ToArray();
        }
    }
}
