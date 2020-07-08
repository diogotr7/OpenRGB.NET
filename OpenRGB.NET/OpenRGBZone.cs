using OpenRGB.NET.Enums;
using System.Collections.Generic;

namespace OpenRGB.NET
{
    public class OpenRGBZone
    {
        public string Name { get; private set; }
        public OpenRGBZoneType Type { get; private set; }
        public uint LedCount { get; private set; }
        public uint LedsMin { get; private set; }
        public uint LedsMax { get; private set; }
        public OpenRGBMatrixMap MatrixMap { get; private set; }

        internal static OpenRGBZone[] Decode(byte[] buffer, ref int offset, ushort zoneCount)
        {
            var zones = new List<OpenRGBZone>((int)zoneCount);

            for (int zone = 0; zone < zoneCount; zone++)
            {
                var newZone = new OpenRGBZone();

                newZone.Name = buffer.GetString(ref offset);

                newZone.Type = (OpenRGBZoneType)buffer.GetUInt32(ref offset);

                newZone.LedsMin = buffer.GetUInt32(ref offset);

                newZone.LedsMax = buffer.GetUInt32(ref offset);

                newZone.LedCount = buffer.GetUInt32(ref offset);

                var zoneMatrixLength = buffer.GetUInt16(ref offset);

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
