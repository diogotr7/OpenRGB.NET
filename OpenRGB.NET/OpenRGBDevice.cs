using System;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBDevice
    {
        public int type;
        public string name;
        public string desc;
        public string version;
        public string serial;
        public string location;
        public int activeMode;
        public OpenRGBMode[] modes;
        public OpenRGBZone[] zones;
        public OpenRGBLed[] leds;
        public OpenRGBColor[] colors;

        public byte[] Encode()
        {
            return new byte[0];
        }

        public static OpenRGBDevice Decode(byte[] buffer)
        {
            var a = new OpenRGBDevice();
            int offset = sizeof(uint);

            a.type = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);
            //name
            ushort nameLength = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.name = Encoding.ASCII.GetString(buffer, offset, nameLength - 1);
            offset += nameLength;
            //desc
            ushort descLength = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.desc = Encoding.ASCII.GetString(buffer, offset, descLength - 1);
            offset += descLength;
            //version
            ushort versLength = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.version = Encoding.ASCII.GetString(buffer, offset, versLength - 1);
            offset += versLength;
            //serial
            ushort serialLength = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.serial = Encoding.ASCII.GetString(buffer, offset, serialLength - 1);
            offset += serialLength;
            //location
            ushort locLength = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.location = Encoding.ASCII.GetString(buffer, offset, locLength - 1);
            offset += locLength;
            //number of modes
            ushort modeCount = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.activeMode = BitConverter.ToInt32(buffer, offset);
            offset += sizeof(int);

            a.modes = OpenRGBMode.Decode(buffer, ref offset, modeCount);

            ushort zoneCount = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.zones = OpenRGBZone.Decode(buffer, ref offset, zoneCount);

            ushort ledCount = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.leds = OpenRGBLed.Decode(buffer, ref offset, ledCount);

            ushort colorCount = BitConverter.ToUInt16(buffer, offset);
            offset += sizeof(ushort);

            a.colors = OpenRGBColor.Decode(buffer, ref offset, colorCount);

            return a;
        }
    }
}
