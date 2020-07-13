using OpenRGB.NET.Enums;
using OpenRGB.NET.Utils;

namespace OpenRGB.NET
{
    public class Device
    {
        public DeviceType Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Version { get; private set; }
        public string Serial { get; private set; }
        public string Location { get; private set; }
        public int ActiveModeIndex { get; private set; }
        public Mode[] Modes { get; private set; }
        public Zone[] Zones { get; private set; }
        public Led[] Leds { get; private set; }
        public Color[] Colors { get; private set; }

        public Mode ActiveMode => Modes[ActiveModeIndex];

        internal static Device Decode(byte[] buffer)
        {
            var dev = new Device();
            int offset = sizeof(uint);

            dev.Type = (DeviceType)buffer.GetInt32(ref offset);

            dev.Name = buffer.GetString(ref offset);

            dev.Description = buffer.GetString(ref offset);

            dev.Version = buffer.GetString(ref offset);

            dev.Serial = buffer.GetString(ref offset);

            dev.Location = buffer.GetString(ref offset);

            var modeCount = buffer.GetUInt16(ref offset);
            dev.ActiveModeIndex = buffer.GetInt32(ref offset);
            dev.Modes = Mode.Decode(buffer, ref offset, modeCount);

            var zoneCount = buffer.GetUInt16(ref offset);
            dev.Zones = Zone.Decode(buffer, ref offset, zoneCount);

            var ledCount = buffer.GetUInt16(ref offset);
            dev.Leds = Led.Decode(buffer, ref offset, ledCount);

            var colorCount = buffer.GetUInt16(ref offset);
            dev.Colors = Color.Decode(buffer, ref offset, colorCount);

            return dev;
        }

        public override string ToString() => $"{Type}: {Name}";
    }
}
