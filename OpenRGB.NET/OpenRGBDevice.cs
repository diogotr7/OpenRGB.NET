using OpenRGB.NET.Enums;
using System;
using System.Text;

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

        public static OpenRGBDevice Decode(byte[] buffer)
        {
            var a = new OpenRGBDevice();
            int offset = sizeof(uint);

            a.Type = (OpenRGBDeviceType)BufferReader.GetInt32(buffer, ref offset);

            a.Name = BufferReader.GetString(buffer, ref offset);

            a.Description = BufferReader.GetString(buffer, ref offset);

            a.Version = BufferReader.GetString(buffer, ref offset);

            a.Serial = BufferReader.GetString(buffer, ref offset);

            a.Location = BufferReader.GetString(buffer, ref offset);

            var modeCount = BufferReader.GetUInt16(buffer, ref offset);
            a.ActiveMode = BufferReader.GetInt32(buffer, ref offset);
            a.Modes = OpenRGBMode.Decode(buffer, ref offset, modeCount);

            var zoneCount = BufferReader.GetUInt16(buffer, ref offset);
            a.Zones = OpenRGBZone.Decode(buffer, ref offset, zoneCount);

            var ledCount = BufferReader.GetUInt16(buffer, ref offset);
            a.Leds = OpenRGBLed.Decode(buffer, ref offset, ledCount);

            var colorCount = BufferReader.GetUInt16(buffer, ref offset);
            a.Colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

            return a;
        }
    }
}
