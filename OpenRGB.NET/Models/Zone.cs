using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;
using System.Security;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Zone class containing the name, type and size of a Zone.
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// The name of the zone.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of the zone.
        /// </summary>
        public ZoneType Type { get; private set; }

        /// <summary>
        /// How many leds the zone has.
        /// </summary>
        public uint LedCount { get; private set; }

        /// <summary>
        /// Minimum number of leds in the zone
        /// </summary>
        public uint LedsMin { get; private set; }

        /// <summary>
        /// Maximum number of leds in the zone
        /// </summary>
        public uint LedsMax { get; private set; }

        /// <summary>
        /// A 2d Matrix containing the LED positions on the zone. Will be null if ZoneType is not ZoneType.MatrixMap
        /// </summary>
        public MatrixMap MatrixMap { get; private set; }

        /// <summary>
        /// Decodes a byte array into a Zone array.
        /// Increments the offset accordingly
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="zoneCount"></param>
        internal static Zone[] Decode(byte[] buffer, ref int offset, ushort zoneCount)
        {
            var zones = new Zone[zoneCount];

            for (int i = 0; i < zoneCount; i++)
            {
                zones[i] = new Zone();

                zones[i].Name = buffer.GetString(ref offset);

                zones[i].Type = (ZoneType)buffer.GetUInt32(ref offset);

                zones[i].LedsMin = buffer.GetUInt32(ref offset);

                zones[i].LedsMax = buffer.GetUInt32(ref offset);

                zones[i].LedCount = buffer.GetUInt32(ref offset);

                var zoneMatrixLength = buffer.GetUInt16(ref offset);

                if (zoneMatrixLength > 0)
                    zones[i].MatrixMap = MatrixMap.Decode(buffer, ref offset);
                else
                    zones[i].MatrixMap = null;
            }

            return zones;
        }
    }
}
