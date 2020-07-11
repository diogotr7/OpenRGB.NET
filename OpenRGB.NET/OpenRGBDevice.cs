using OpenRGB.NET.Enums;

namespace OpenRGB.NET
{
    public class OpenRGBDevice
    {
        public OpenRGBDeviceType Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Version { get; private set; }
        public string Serial { get; private set; }
        public string Location { get; private set; }
        public int ActiveModeIndex { get; private set; }
        public OpenRGBMode[] Modes { get; private set; }
        public OpenRGBZone[] Zones { get; private set; }
        public OpenRGBLed[] Leds { get; private set; }
        public OpenRGBColor[] Colors { get; private set; }

        public OpenRGBMode ActiveMode => Modes[ActiveModeIndex];

        internal static OpenRGBDevice Decode(byte[] buffer)
        {
            var dev = new OpenRGBDevice();
            int offset = sizeof(uint);

            dev.Type = (OpenRGBDeviceType)buffer.GetInt32(ref offset);

            dev.Name = buffer.GetString(ref offset);

            dev.Description = buffer.GetString(ref offset);

            dev.Version = buffer.GetString(ref offset);

            dev.Serial = buffer.GetString(ref offset);

            dev.Location = buffer.GetString(ref offset);

            var modeCount = buffer.GetUInt16(ref offset);
            dev.ActiveModeIndex = buffer.GetInt32(ref offset);
            dev.Modes = OpenRGBMode.Decode(buffer, ref offset, modeCount);

            var zoneCount = buffer.GetUInt16(ref offset);
            dev.Zones = OpenRGBZone.Decode(buffer, ref offset, zoneCount);

            var ledCount = buffer.GetUInt16(ref offset);
            dev.Leds = OpenRGBLed.Decode(buffer, ref offset, ledCount);

            var colorCount = buffer.GetUInt16(ref offset);
            dev.Colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

            return dev;
        }

        public override string ToString() => $"{Type}: {Name}";
    }
}
