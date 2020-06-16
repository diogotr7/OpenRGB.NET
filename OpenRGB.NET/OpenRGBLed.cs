using System;
using System.Collections.Generic;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBLed
    {
        public string Name;
        public uint Value;

        public static OpenRGBLed[] Decode(byte[] buffer, ref int offset, ushort ledCount)
        {
            var leds = new List<OpenRGBLed>(ledCount);

            for (int led = 0; led < ledCount; led++)
            {
                var newLed = new OpenRGBLed();

                ushort ledNameLength = BitConverter.ToUInt16(buffer, offset);
                offset += sizeof(ushort);

                newLed.Name = Encoding.ASCII.GetString(buffer, offset, ledNameLength - 1);
                offset += ledNameLength;

                newLed.Value = BitConverter.ToUInt32(buffer, offset);
                offset += sizeof(uint);

                leds.Add(newLed);
            }

            return leds.ToArray();
        }

        public override string ToString()
        {
            return Name ?? "";
        }
    }
}
