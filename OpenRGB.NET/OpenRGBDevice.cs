using OpenRGB.NET.Enums;

namespace OpenRGB.NET
{
    public class OpenRGBDevice
    {
        public OpenRGBDeviceType Type;
        public string Name;
        public string Description;
        public string Version;
        public string Serial;
        public string Location;
        public int ActiveMode;
        public OpenRGBMode[] Modes;
        public OpenRGBZone[] Zones;
        public OpenRGBLed[] Leds;
        public OpenRGBColor[] Colors;

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
            dev.ActiveMode = buffer.GetInt32(ref offset);
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
