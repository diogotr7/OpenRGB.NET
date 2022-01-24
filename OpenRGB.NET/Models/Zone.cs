using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;
using System.IO;
using System.Security;

namespace OpenRGB.NET.Models
{
    /// <summary>
    /// Zone class containing the name, type and size of a Zone.
    /// </summary>
    public class Zone
    {
        /// <summary>
        /// The ID of the zone.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// The ID of the zone's device
        /// </summary>
        public int DeviceID { get; private set; }

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
        /// <param name="reader"></param>
        /// <param name="zoneCount"></param>
        /// <param name="deviceID"></param>
        /// <param name="zoneID"></param>
        internal static Zone[] Decode(BinaryReader reader, ushort zoneCount, int deviceID)
        {
            var zones = new Zone[zoneCount];

            for (int i = 0; i < zoneCount; i++)
            {
                zones[i] = new Zone();

                zones[i].DeviceID = deviceID;

                zones[i].ID = i;

                zones[i].Name = reader.ReadLengthAndString();

                zones[i].Type = (ZoneType)reader.ReadUInt32();

                zones[i].LedsMin = reader.ReadUInt32();

                zones[i].LedsMax = reader.ReadUInt32();

                zones[i].LedCount = reader.ReadUInt32();

                var zoneMatrixLength = reader.ReadUInt16();

                if (zoneMatrixLength > 0)
                    zones[i].MatrixMap = MatrixMap.Decode(reader);
                else
                    zones[i].MatrixMap = null;
            }

            return zones;
        }
    }
}
