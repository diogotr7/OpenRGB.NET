using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenRGB.NET
{
    public class OpenRGBLed
    {
        public string Name;
        public uint Value;

        public static OpenRGBLed[] Decode(byte[] buffer, ref int offset, ushort ledCount)
        {
            var leds = new OpenRGBLed[ledCount];

            for (int led = 0; led < ledCount; led++)
            {
                leds[led] = new OpenRGBLed
                {
                    Name = BufferReader.GetString(buffer, ref offset),
                    Value = BufferReader.GetUInt32(buffer, ref offset)
                };
            }

            return leds;
        }

        public override string ToString()
        {
            return $"Name: {Name}, Value: {Value}";
        }
    }
}
