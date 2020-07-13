using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET
{
    public class OpenRGBZone
    {
        public string Name { get; private set; }
        public ZoneType Type { get; private set; }
        public uint LedCount { get; private set; }
        public uint LedsMin { get; private set; }
        public uint LedsMax { get; private set; }
        public OpenRGBMatrixMap MatrixMap { get; private set; }

        internal static OpenRGBZone[] Decode(byte[] buffer, ref int offset, ushort zoneCount)
        {
            var zones = new OpenRGBZone[zoneCount];

            for (int i = 0; i < zoneCount; i++)
            {
                zones[i] = new OpenRGBZone();

                zones[i].Name = buffer.GetString(ref offset);

                zones[i].Type = (ZoneType)buffer.GetUInt32(ref offset);

                zones[i].LedsMin = buffer.GetUInt32(ref offset);

                zones[i].LedsMax = buffer.GetUInt32(ref offset);

                zones[i].LedCount = buffer.GetUInt32(ref offset);

                var zoneMatrixLength = buffer.GetUInt16(ref offset);

                if (zoneMatrixLength > 0)
                    zones[i].MatrixMap = OpenRGBMatrixMap.Decode(buffer, ref offset);
                else
                    zones[i].MatrixMap = null;
            }

            return zones;
        }
    }
}
